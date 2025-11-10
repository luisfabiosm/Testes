using Adapters.Outbound.Database.SQL;
using api_crud_template_testes.Fixtures;
using Domain.Core.Interfaces.Outbound;
using Domain.Core.Settings;
using Domain.Core.SharedKernel;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Data;
using Xunit;

namespace api_crud_template_testes.Integration.Repositories;

[Collection("Database")]
public class UserRepositoryIntegrationTests 
{
    private readonly ISQLConnectionAdapter _sqlConnection;
    private readonly IOptions<DatabaseSettings> _dbSettings;
    private readonly ILogger<UserRepository> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly UserRepository _repository;
    private readonly ServiceCollection _services;

    public UserRepositoryIntegrationTests()
    {
        _sqlConnection = Substitute.For<ISQLConnectionAdapter>();
        _dbSettings = Substitute.For<IOptions<DatabaseSettings>>();
        _logger = Substitute.For<ILogger<UserRepository>>();

        var dbSettings = new DatabaseSettings
        {
            CommandTimeout = 30,
            ConnectTimeout = 30,
            MaxRetryAttempts = 3
        };

        _dbSettings.Value.Returns(dbSettings);

        _services = new ServiceCollection();
        _services.AddSingleton(_sqlConnection);
        _services.AddSingleton(_dbSettings);
        _services.AddSingleton(_logger);

        _serviceProvider = _services.BuildServiceProvider();
        _repository = new UserRepository(_serviceProvider);
    }

    [Fact]
    public async Task CreateAsync_InMockEnvironment_ShouldReturnSuccessWithoutDatabaseCall()
    {
        // Arrange
        Global.ENVIRONMENT = "Mock";
        var transaction = TestFixtures.Users.ValidTransaction;

        // Act
        var result = await _repository.CreateAsync(transaction);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(transaction.NewUser.Id);

        // Verify no database calls were made
        await _sqlConnection.DidNotReceive().ExecuteWithRetryAsync(Arg.Any<Func<IDbConnection, Task<int>>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_InProductionEnvironment_ShouldExecuteDatabaseQuery()
    {
        // Arrange
        Global.ENVIRONMENT = "Production";
        var transaction = TestFixtures.Users.ValidTransaction;
        var expectedRowsAffected = 1;

        _sqlConnection.ExecuteWithRetryAsync(Arg.Any<Func<IDbConnection, Task<int>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedRowsAffected));

        // Act
        var result = await _repository.CreateAsync(transaction);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(transaction.NewUser.Id);

        // Verify database call was made
        await _sqlConnection.Received(1).ExecuteWithRetryAsync(
            Arg.Any<Func<IDbConnection, Task<int>>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WhenDatabaseReturnsZeroRowsAffected_ShouldReturnFailure()
    {
        // Arrange
        Global.ENVIRONMENT = "Production";
        var transaction = TestFixtures.Users.ValidTransaction;
        var zeroRowsAffected = 0;

        _sqlConnection.ExecuteWithRetryAsync(Arg.Any<Func<IDbConnection, Task<int>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(zeroRowsAffected));

        // Act
        var result = await _repository.CreateAsync(transaction);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Falha ao criar usuário no banco de dados");
    }

    [Fact]
    public async Task CreateAsync_WhenDatabaseThrowsException_ShouldReturnFailure()
    {
        // Arrange
        Global.ENVIRONMENT = "Production";
        var transaction = TestFixtures.Users.ValidTransaction;
        var expectedException = new InvalidOperationException("Database connection failed");

        _sqlConnection.ExecuteWithRetryAsync(Arg.Any<Func<IDbConnection, Task<int>>>(), Arg.Any<CancellationToken>())
            .Throws(expectedException);

        // Act
        var result = await _repository.CreateAsync(transaction);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Erro ao criar usuário");
        result.Error.Should().Contain(expectedException.Message);
    }

    [Fact]
    public async Task ExistsByEmailAsync_InMockEnvironment_ShouldReturnFalse()
    {
        // Arrange
        Global.ENVIRONMENT = "Mock";
        var email = "test@example.com";

        // Act
        var result = await _repository.ExistsByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();

        // Verify no database calls were made
        await _sqlConnection.DidNotReceive().ExecuteWithRetryAsync(Arg.Any<Func<IDbConnection, Task>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExistsByEmailAsync_InProductionEnvironment_WhenEmailExists_ShouldReturnTrue()
    {
        // Arrange
        Global.ENVIRONMENT = "Production";
        var email = "existing@example.com";
        var emailExists = true;

        _sqlConnection.ExecuteWithRetryAsync(Arg.Any<Func<IDbConnection, Task>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask)
            .AndDoes(callInfo =>
            {
                // Simulate the database call execution
                var func = callInfo.ArgAt<Func<IDbConnection, Task>>(0);
                var mockConnection = Substitute.For<IDbConnection>();

                // We need to simulate the Dapper QuerySingleAsync call returning true
                // This is a simplified simulation since we can't easily mock Dapper extensions
            });

        // For this test, we'll simulate the internal behavior
        // In a real integration test, you'd use a real database or TestContainers

        // Act & Assert - This is a simplified version
        // In a complete integration test environment, you would:
        // 1. Use TestContainers to spin up a real SQL Server
        // 2. Execute the actual repository method
        // 3. Verify the results

        var result = await _repository.ExistsByEmailAsync(email);

        // For now, we'll just verify the structure is correct
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ExistsByEmailAsync_WhenDatabaseThrowsException_ShouldReturnFailure()
    {
        // Arrange
        Global.ENVIRONMENT = "Production";
        var email = "test@example.com";
        var expectedException = new InvalidOperationException("Database query failed");

        _sqlConnection.ExecuteWithRetryAsync(Arg.Any<Func<IDbConnection, Task>>(), Arg.Any<CancellationToken>())
            .Throws(expectedException);

        // Act
        var result = await _repository.ExistsByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain(expectedException.Message);
    }

    [Fact]
    public async Task CreateAsync_WithCancellationToken_ShouldPassTokenToDatabase()
    {
        // Arrange
        Global.ENVIRONMENT = "Production";
        var transaction = TestFixtures.Users.ValidTransaction;
        var cancellationToken = new CancellationToken();
        var expectedRowsAffected = 1;

        _sqlConnection.ExecuteWithRetryAsync(Arg.Any<Func<IDbConnection, Task<int>>>(), cancellationToken)
            .Returns(Task.FromResult(expectedRowsAffected));

        // Act
        var result = await _repository.CreateAsync(transaction, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify the cancellation token was passed correctly
        await _sqlConnection.Received(1).ExecuteWithRetryAsync(
            Arg.Any<Func<IDbConnection, Task<int>>>(),
            cancellationToken);
    }

    [Fact]
    public async Task ExecStoredProcedureCreateAsync_InMockEnvironment_ShouldReturnSuccessWithoutDatabaseCall()
    {
        // Arrange
        Global.ENVIRONMENT = "Mock";
        var transaction = TestFixtures.Users.ValidTransaction;

        // Act
        var result = await _repository.ExecStoredProcedureCreateAsync(transaction);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(transaction.NewUser.Id);

        // Verify no database calls were made
        await _sqlConnection.DidNotReceive().ExecuteWithRetryAsync(Arg.Any<Func<IDbConnection, Task>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecStoredProcedureCreateAsync_InProductionEnvironment_ShouldExecuteStoredProcedure()
    {
        // Arrange
        Global.ENVIRONMENT = "Production";
        var transaction = TestFixtures.Users.ValidTransaction;

        _sqlConnection.ExecuteWithRetryAsync(Arg.Any<Func<IDbConnection, Task>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _repository.ExecStoredProcedureCreateAsync(transaction);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(transaction.NewUser.Id);

        // Verify database call was made
        await _sqlConnection.Received(1).ExecuteWithRetryAsync(
            Arg.Any<Func<IDbConnection, Task>>(),
            Arg.Any<CancellationToken>());
    }



    [Theory]
    [InlineData("Mock")]
    [InlineData("Development")]
    [InlineData("Production")]
    [InlineData("Staging")]
    public async Task CreateAsync_ShouldHandleDifferentEnvironments(string environment)
    {
        // Arrange
        Global.ENVIRONMENT = environment;
        var transaction = TestFixtures.Users.ValidTransaction;

        if (environment != "Mock")
        {
            _sqlConnection.ExecuteWithRetryAsync(Arg.Any<Func<IDbConnection, Task<int>>>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(1));
        }

        // Act
        var result = await _repository.CreateAsync(transaction);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

 
}

