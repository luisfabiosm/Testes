using Domain.Core.Common.Base;
using Domain.Core.Ports.Outbound;
using Domain.Core.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using System.Data;
using Xunit;

namespace pix_pagador_testes.Domain.Core.Common.Base
{
    public class BaseSQLRepositoryTest : IDisposable
    {
        private TestablBaseSQLRepository _testClass;
        private Mock<ISQLConnectionAdapter> _mockSQLConnectionAdapter;
        private Mock<ILoggingAdapter> _mockLoggingAdapter;
        private Mock<IServiceProvider> _mockServiceProvider;
        private Mock<IOptions<DBSettings>> _mockDBSettings;
        private Mock<IDbConnection> _mockDbConnection;

        public BaseSQLRepositoryTest()
        {
            //_mockLoggingAdapter = new Mock<ILoggingAdapter>();
            //_mockServiceProvider = new Mock<IServiceProvider>();
            //_mockServiceProvider.Setup(x => x.GetService(typeof(ILoggingAdapter))).Returns(_mockLoggingAdapter.Object);
            //_testClass = new TestableBaseService(_mockServiceProvider.Object);

            _mockSQLConnectionAdapter = new Mock<ISQLConnectionAdapter>();
            _mockLoggingAdapter = new Mock<ILoggingAdapter>();
            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockDBSettings = new Mock<IOptions<DBSettings>>();
            _mockDbConnection = new Mock<IDbConnection>();

            var dbSettings = new DBSettings
            {
                ServerUrl = "test-server",
                Database = "test-database",
                Username = "test-user",
                Password = "test-password",
                CommandTimeout = 30,
                ConnectTimeout = 10
            };
            _mockDBSettings.Setup(x => x.Value).Returns(dbSettings);

            _mockServiceProvider.Setup(x => x.GetService(typeof(ILoggingAdapter))).Returns(_mockLoggingAdapter.Object);
            _mockServiceProvider.Setup(x => x.GetService(typeof(ISQLConnectionAdapter))).Returns(_mockSQLConnectionAdapter.Object);
            _mockServiceProvider.Setup(x => x.GetService(typeof(IOptions<DBSettings>))).Returns(_mockDBSettings.Object);

            _testClass = new TestablBaseSQLRepository(_mockServiceProvider.Object);
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new TestablBaseSQLRepository(_mockServiceProvider.Object);

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public void CannotConstructWithNullServiceProvider()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new TestablBaseSQLRepository(null));
        }

        [Fact]
        public void DBConnectionIsInitializedCorrectly()
        {
            // Assert
            Assert.Equal(_mockSQLConnectionAdapter.Object, _testClass.GetDBConnection());
        }

        [Fact]
        public void DBSettingsIsInitializedCorrectly()
        {
            // Assert
            Assert.Equal(_mockDBSettings.Object, _testClass.GetDBSettings());
        }

        [Fact]
        public void SessionIsInitializedCorrectly()
        {
            // Assert
            Assert.Null(_testClass.GetSession()); // Initially null
        }

        [Fact]
        public void DisposeClosesConnectionWhenOpen()
        {
            // Arrange
            _mockDbConnection.Setup(x => x.State).Returns(ConnectionState.Open);
            _testClass.SetSession(_mockDbConnection.Object);

            // Act
            _testClass.Dispose();

            // Assert
            _mockDbConnection.Verify(x => x.Close(), Times.Once);
            _mockDbConnection.Verify(x => x.Dispose(), Times.Once);
        }

        [Fact]
        public void DisposeDoesNotCloseConnectionWhenClosed()
        {
            // Arrange
            _mockDbConnection.Setup(x => x.State).Returns(ConnectionState.Closed);
            _testClass.SetSession(_mockDbConnection.Object);

            // Act
            _testClass.Dispose();

            // Assert
            _mockDbConnection.Verify(x => x.Close(), Times.Never);
            _mockDbConnection.Verify(x => x.Dispose(), Times.Once);
        }

        [Fact]
        public void FinalizerBehaviorDoesNotCleanManagedResources()
        {
            // Arrange
            _mockDbConnection.Setup(x => x.State).Returns(ConnectionState.Open);
            _testClass.SetSession(_mockDbConnection.Object);

            // Act - Simular comportamento do finalizador
            _testClass.TestDispose(false);

            // Assert - Finalizador não deve limpar recursos gerenciados
            _mockDbConnection.Verify(x => x.Close(), Times.Never);
            _mockDbConnection.Verify(x => x.Dispose(), Times.Never);

            // Verificar que subsequent calls também não fazem cleanup
            _testClass.TestDispose(false);
            _mockDbConnection.Verify(x => x.Close(), Times.Never);
            _mockDbConnection.Verify(x => x.Dispose(), Times.Never);
        }

        [Fact]
        public void DisposeCanBeCalledMultipleTimes()
        {
            // Arrange
            _mockDbConnection.Setup(x => x.State).Returns(ConnectionState.Open);
            _testClass.SetSession(_mockDbConnection.Object);

            // Act
            _testClass.Dispose();
            _testClass.Dispose(); // Should not throw

            // Assert - Dispose should only be called once
            _mockDbConnection.Verify(x => x.Close(), Times.Once);
            _mockDbConnection.Verify(x => x.Dispose(), Times.Once);
        }

        [Fact]
        public void DisposeHandlesNullSession()
        {
            // Act
            _testClass.Dispose(); // Should not throw when session is null

            // Assert - No exceptions should be thrown
            Assert.True(true);
        }

        [Fact]
        public void DisposePreventsFinalizerExecution()
        {
            // Arrange
            _mockDbConnection.Setup(x => x.State).Returns(ConnectionState.Open);
            _testClass.SetSession(_mockDbConnection.Object);

            // Act - Dispose explícito primeiro
            _testClass.Dispose();

            // Simular tentativa de finalização depois
            _testClass.TestDispose(false);

            // Assert - Só o primeiro Dispose deve ter efeito
            _mockDbConnection.Verify(x => x.Close(), Times.Once);
            _mockDbConnection.Verify(x => x.Dispose(), Times.Once);
        }

        public void Dispose()
        {
            _testClass?.Dispose();
        }

        // Testable implementation to access protected members
        private class TestablBaseSQLRepository : BaseSQLRepository
        {
            public TestablBaseSQLRepository(IServiceProvider serviceProvider) : base(serviceProvider)
            {
            }

            public ISQLConnectionAdapter GetDBConnection()
            {
                return _dbConnection;
            }

            public IOptions<DBSettings> GetDBSettings()
            {
                return _dbsettings;
            }

            public IDbConnection GetSession()
            {
                return _session;
            }

            public void SetSession(IDbConnection session)
            {
                _session = session;
            }

            public void TestDispose(bool disposing)
            {
                Dispose(disposing);
            }
        }
    }
}
