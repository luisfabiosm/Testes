using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using Testcontainers.MsSql;

namespace pix_pagador_testes.TestUtilities.Fixtures;

public class DatabaseFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _sqlServerContainer;

    public string ConnectionString => _sqlServerContainer.GetConnectionString();
    public IConfiguration Configuration { get; private set; }

    public DatabaseFixture()
    {
        _sqlServerContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("Test123!")
            .WithEnvironment("ACCEPT_EULA", "Y")
            .WithCleanUp(true)
            .Build();

        Configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = ConnectionString,
                ["DatabaseSettings:CommandTimeout"] = "30",
                ["DatabaseSettings:RetryCount"] = "3"
            })
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _sqlServerContainer.StartAsync();
        await InitializeDatabase();
    }

    public async Task DisposeAsync()
    {
        await _sqlServerContainer.StopAsync();
        await _sqlServerContainer.DisposeAsync();
    }

    private async Task InitializeDatabase()
    {
        using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();

        // Create test database schema
        var createTablesScript = @"
            -- Criar tabela de transações PIX
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='PIX_TRANSACOES' AND xtype='U')
            CREATE TABLE PIX_TRANSACOES (
                Id BIGINT IDENTITY(1,1) PRIMARY KEY,
                IdReqSistemaCliente NVARCHAR(100) NOT NULL,
                EndToEndId NVARCHAR(50) UNIQUE,
                Valor DECIMAL(18,2) NOT NULL,
                Status NVARCHAR(20) NOT NULL,
                DataCriacao DATETIME2 DEFAULT GETDATE(),
                DataAtualizacao DATETIME2 DEFAULT GETDATE(),
                CorrelationId NVARCHAR(100),
                Canal NVARCHAR(20),
                ChaveIdempotencia NVARCHAR(100)
            );

            -- Criar tabela de devoluções
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='PIX_DEVOLUCOES' AND xtype='U')
            CREATE TABLE PIX_DEVOLUCOES (
                Id BIGINT IDENTITY(1,1) PRIMARY KEY,
                IdDevolucao NVARCHAR(100) NOT NULL,
                IdTransacaoOriginal BIGINT NOT NULL,
                EndToEndIdOriginal NVARCHAR(50) NOT NULL,
                Valor DECIMAL(18,2) NOT NULL,
                Motivo NVARCHAR(500),
                Status NVARCHAR(20) NOT NULL,
                DataCriacao DATETIME2 DEFAULT GETDATE(),
                CorrelationId NVARCHAR(100),
                FOREIGN KEY (IdTransacaoOriginal) REFERENCES PIX_TRANSACOES(Id)
            );

            -- Criar índices
            CREATE INDEX IX_PIX_TRANSACOES_EndToEndId ON PIX_TRANSACOES(EndToEndId);
            CREATE INDEX IX_PIX_TRANSACOES_Status ON PIX_TRANSACOES(Status);
            CREATE INDEX IX_PIX_DEVOLUCOES_EndToEndIdOriginal ON PIX_DEVOLUCOES(EndToEndIdOriginal);
        ";

        await connection.ExecuteAsync(createTablesScript);

        // Insert test data
        var insertTestDataScript = @"
            -- Inserir dados de teste
            INSERT INTO PIX_TRANSACOES (IdReqSistemaCliente, EndToEndId, Valor, Status, CorrelationId, Canal)
            VALUES 
                ('REQ123456789', 'E12345678202412041200202412040001', 100.50, 'EFETIVADO', 'CORR-001', 'WEB'),
                ('REQ987654321', 'E98765432109876543210987654321098', 250.00, 'REGISTRADO', 'CORR-002', 'MOBILE'),
                ('REQ555666777', 'E55566677788899900112233445566778', 75.25, 'CANCELADO', 'CORR-003', 'API');

            INSERT INTO PIX_DEVOLUCOES (IdDevolucao, IdTransacaoOriginal, EndToEndIdOriginal, Valor, Motivo, Status, CorrelationId)
            VALUES 
                ('DEV123456', 1, 'E12345678202412041200202412040001', 50.25, 'Devolução parcial', 'EFETIVADO', 'CORR-DEV-001');
        ";

        await connection.ExecuteAsync(insertTestDataScript);
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

        await connection.ExecuteAsync(@"
            DELETE FROM PIX_DEVOLUCOES;
            DELETE FROM PIX_TRANSACOES;
            DBCC CHECKIDENT ('PIX_TRANSACOES', RESEED, 0);
            DBCC CHECKIDENT ('PIX_DEVOLUCOES', RESEED, 0);
        ");
    }
}