using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using Testcontainers.MsSql;

namespace pix_pagador_testes.TestUtilities.Fixtures;

public class SqlServerTestFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _sqlServerContainer;

    public string ConnectionString => _sqlServerContainer.GetConnectionString();
    public IConfiguration Configuration { get; private set; }

    public SqlServerTestFixture()
    {
        // Configurar container SQL Server usando a nova API
        _sqlServerContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("Test123!@#")
            .WithEnvironment("ACCEPT_EULA", "Y")
            .WithEnvironment("MSSQL_PID", "Express")
            .WithCleanUp(true)
            .WithAutoRemove(true)
            .Build();

        // Configuração que será atualizada após o container iniciar
        Configuration = CreateConfiguration();
    }

    public async Task InitializeAsync()
    {
        try
        {
            // Iniciar container SQL Server
            await _sqlServerContainer.StartAsync();

            // Aguardar container estar completamente pronto
            await WaitForSqlServerToBeReady();

            // Recriar configuração com connection string atualizada
            Configuration = CreateConfiguration();

            // Criar esquema do banco de dados
            await CreateDatabaseSchema();

            // Inserir dados de teste iniciais
            await SeedTestData();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Falha ao inicializar SQL Server container: {ex.Message}", ex);
        }
    }

    public async Task DisposeAsync()
    {
        try
        {
            await _sqlServerContainer.StopAsync();
            await _sqlServerContainer.DisposeAsync();
        }
        catch (Exception ex)
        {
            // Log error mas não falhe o dispose
            Console.WriteLine($"Erro ao fazer dispose do SQL Server container: {ex.Message}");
        }
    }

    private IConfiguration CreateConfiguration()
    {
        var connectionString = _sqlServerContainer?.GetConnectionString() ??
                              "Server=localhost;Database=TestDB;Integrated Security=true;";

        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = connectionString,
                ["ConnectionStrings:ReadOnlyConnection"] = connectionString,
                ["DatabaseSettings:CommandTimeout"] = "30",
                ["DatabaseSettings:RetryCount"] = "3",
                ["DatabaseSettings:MaxPoolSize"] = "100",
                ["DatabaseSettings:MinPoolSize"] = "5"
            })
            .Build();
    }

    private async Task WaitForSqlServerToBeReady()
    {
        var maxAttempts = 30;
        var delay = TimeSpan.FromSeconds(2);

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                using var connection = new SqlConnection(ConnectionString);
                await connection.OpenAsync();

                var result = await connection.QueryFirstOrDefaultAsync<int>("SELECT 1");
                if (result == 1)
                {
                    Console.WriteLine($"✅ SQL Server container está pronto após {attempt} tentativas");
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⏳ Tentativa {attempt}/{maxAttempts} - Aguardando SQL Server: {ex.Message}");

                if (attempt == maxAttempts)
                {
                    throw new TimeoutException($"SQL Server não ficou pronto após {maxAttempts} tentativas");
                }

                await Task.Delay(delay);
            }
        }
    }

    private async Task CreateDatabaseSchema()
    {
        using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();

        var createSchemaScript = @"
            -- Criar database específico para testes
            IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'PIXPagadorTestDB')
            BEGIN
                CREATE DATABASE PIXPagadorTestDB;
            END;
            
            USE PIXPagadorTestDB;
            
            -- Criar tabela de transações PIX
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='PIX_TRANSACOES' AND xtype='U')
            CREATE TABLE PIX_TRANSACOES (
                Id BIGINT IDENTITY(1,1) PRIMARY KEY,
                IdReqSistemaCliente NVARCHAR(100) NOT NULL,
                EndToEndId NVARCHAR(50) UNIQUE,
                Valor DECIMAL(18,2) NOT NULL,
                Status NVARCHAR(20) NOT NULL,
                TpIniciacao INT NOT NULL DEFAULT 1,
                PrioridadePagamento INT NULL,
                Chave NVARCHAR(200) NOT NULL,
                Canal NVARCHAR(20) NULL,
                ChaveIdempotencia NVARCHAR(100) NULL,
                DataCriacao DATETIME2 DEFAULT GETDATE(),
                DataAtualizacao DATETIME2 DEFAULT GETDATE(),
                CorrelationId NVARCHAR(100) NULL,
                DadosPagadorJson NVARCHAR(MAX) NULL,
                DadosRecebedorJson NVARCHAR(MAX) NULL,
                INDEX IX_PIX_TRANSACOES_EndToEndId NONCLUSTERED (EndToEndId),
                INDEX IX_PIX_TRANSACOES_Status NONCLUSTERED (Status),
                INDEX IX_PIX_TRANSACOES_IdReqSistemaCliente NONCLUSTERED (IdReqSistemaCliente),
                INDEX IX_PIX_TRANSACOES_DataCriacao NONCLUSTERED (DataCriacao)
            );

            -- Criar tabela de devoluções
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='PIX_DEVOLUCOES' AND xtype='U')
            CREATE TABLE PIX_DEVOLUCOES (
                Id BIGINT IDENTITY(1,1) PRIMARY KEY,
                IdDevolucao NVARCHAR(100) NOT NULL UNIQUE,
                IdTransacaoOriginal BIGINT NOT NULL,
                EndToEndIdOriginal NVARCHAR(50) NOT NULL,
                IdReqSistemaCliente NVARCHAR(100) NOT NULL,
                Valor DECIMAL(18,2) NOT NULL,
                Motivo NVARCHAR(500) NULL,
                Status NVARCHAR(20) NOT NULL,
                DataCriacao DATETIME2 DEFAULT GETDATE(),
                DataAtualizacao DATETIME2 DEFAULT GETDATE(),
                CorrelationId NVARCHAR(100) NULL,
                FOREIGN KEY (IdTransacaoOriginal) REFERENCES PIX_TRANSACOES(Id),
                INDEX IX_PIX_DEVOLUCOES_EndToEndIdOriginal NONCLUSTERED (EndToEndIdOriginal),
                INDEX IX_PIX_DEVOLUCOES_IdDevolucao NONCLUSTERED (IdDevolucao),
                INDEX IX_PIX_DEVOLUCOES_Status NONCLUSTERED (Status)
            );

            -- Criar tabela de logs de auditoria
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='PIX_AUDITORIA' AND xtype='U')
            CREATE TABLE PIX_AUDITORIA (
                Id BIGINT IDENTITY(1,1) PRIMARY KEY,
                EndToEndId NVARCHAR(50) NOT NULL,
                Operacao NVARCHAR(50) NOT NULL,
                StatusAnterior NVARCHAR(20) NULL,
                StatusNovo NVARCHAR(20) NOT NULL,
                DadosJson NVARCHAR(MAX) NULL,
                UsuarioId NVARCHAR(100) NULL,
                DataOperacao DATETIME2 DEFAULT GETDATE(),
                CorrelationId NVARCHAR(100) NULL,
                INDEX IX_PIX_AUDITORIA_EndToEndId NONCLUSTERED (EndToEndId),
                INDEX IX_PIX_AUDITORIA_DataOperacao NONCLUSTERED (DataOperacao)
            );
        ";

        await connection.ExecuteAsync(createSchemaScript);
        Console.WriteLine("✅ Schema do banco de dados criado com sucesso");
    }

    private async Task SeedTestData()
    {
        using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();

        var seedDataScript = @"
            USE PIXPagadorTestDB;
            
            -- Limpar dados existentes
            DELETE FROM PIX_AUDITORIA;
            DELETE FROM PIX_DEVOLUCOES;
            DELETE FROM PIX_TRANSACOES;
            
            -- Reset identity
            DBCC CHECKIDENT ('PIX_TRANSACOES', RESEED, 0);
            DBCC CHECKIDENT ('PIX_DEVOLUCOES', RESEED, 0);
            DBCC CHECKIDENT ('PIX_AUDITORIA', RESEED, 0);
            
            -- Inserir dados de teste padrão
            INSERT INTO PIX_TRANSACOES (
                IdReqSistemaCliente, EndToEndId, Valor, Status, TpIniciacao, 
                Chave, Canal, CorrelationId, DadosPagadorJson, DadosRecebedorJson
            ) VALUES 
                (
                    'TEST_REQ_001', 
                    'E12345678202412041200202412040001', 
                    100.50, 
                    'EFETIVADO', 
                    1,
                    'test@pix.com.br', 
                    'TEST', 
                    'TEST-CORR-001',
                    '{""cpfCnpj"":""12345678901"",""nome"":""João Test""}',
                    '{""cpfCnpj"":""98765432100"",""nome"":""Maria Test""}'
                ),
                (
                    'TEST_REQ_002', 
                    'E98765432109876543210987654321098', 
                    250.00, 
                    'REGISTRADO', 
                    2,
                    'test2@pix.com.br', 
                    'TEST', 
                    'TEST-CORR-002',
                    '{""cpfCnpj"":""11111111111"",""nome"":""Pedro Test""}',
                    '{""cpfCnpj"":""22222222222"",""nome"":""Ana Test""}'
                ),
                (
                    'TEST_REQ_003', 
                    'E55566677788899900112233445566778', 
                    75.25, 
                    'CANCELADO', 
                    1,
                    '+5511999999999', 
                    'TEST', 
                    'TEST-CORR-003',
                    '{""cpfCnpj"":""33333333333"",""nome"":""Carlos Test""}',
                    '{""cpfCnpj"":""44444444444"",""nome"":""Julia Test""}'
                );

            -- Inserir uma devolução de teste
            INSERT INTO PIX_DEVOLUCOES (
                IdDevolucao, IdTransacaoOriginal, EndToEndIdOriginal, 
                IdReqSistemaCliente, Valor, Motivo, Status, CorrelationId
            ) VALUES 
                (
                    'DEV_TEST_001', 
                    1, 
                    'E12345678202412041200202412040001',
                    'DEV_TEST_REQ_001', 
                    50.25, 
                    'Devolução de teste', 
                    'EFETIVADO', 
                    'DEV-TEST-CORR-001'
                );

            -- Inserir logs de auditoria
            INSERT INTO PIX_AUDITORIA (
                EndToEndId, Operacao, StatusAnterior, StatusNovo, 
                UsuarioId, CorrelationId, DadosJson
            ) VALUES 
                (
                    'E12345678202412041200202412040001',
                    'REGISTRAR',
                    NULL,
                    'REGISTRADO',
                    'test_user',
                    'TEST-CORR-001',
                    '{""operacao"":""registrar"",""valor"":100.50}'
                ),
                (
                    'E12345678202412041200202412040001',
                    'EFETIVAR',
                    'REGISTRADO',
                    'EFETIVADO',
                    'test_user',
                    'TEST-CORR-001',
                    '{""operacao"":""efetivar"",""valor"":100.50}'
                );
        ";

        await connection.ExecuteAsync(seedDataScript);
        Console.WriteLine("✅ Dados de teste inseridos com sucesso");
    }

    public async Task<IDbConnection> CreateConnectionAsync()
    {
        var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        return connection;
    }

    public async Task CleanupAsync()
    {
        using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();

        var cleanupScript = @"
            USE PIXPagadorTestDB;
            
            DELETE FROM PIX_AUDITORIA;
            DELETE FROM PIX_DEVOLUCOES;
            DELETE FROM PIX_TRANSACOES;
            
            DBCC CHECKIDENT ('PIX_TRANSACOES', RESEED, 0);
            DBCC CHECKIDENT ('PIX_DEVOLUCOES', RESEED, 0);
            DBCC CHECKIDENT ('PIX_AUDITORIA', RESEED, 0);
        ";

        await connection.ExecuteAsync(cleanupScript);
        Console.WriteLine("🧹 Dados de teste limpos com sucesso");
    }

    public async Task<bool> IsHealthyAsync()
    {
        try
        {
            using var connection = new SqlConnection(ConnectionString);
            await connection.OpenAsync();

            var result = await connection.QueryFirstOrDefaultAsync<int>("SELECT 1");
            return result == 1;
        }
        catch
        {
            return false;
        }
    }
}
