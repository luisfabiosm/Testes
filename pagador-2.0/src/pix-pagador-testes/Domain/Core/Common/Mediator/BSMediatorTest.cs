using AutoFixture.Xunit2;
using Domain.Core.Common.Mediator;
using Domain.Core.Common.ResultPattern;
using Domain.Core.Common.Transaction;
using Domain.Core.Models.Response;
using Domain.Core.Ports.Outbound;
using Domain.UseCases.Pagamento.RegistrarOrdemPagamento;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using pix_pagador_testes.TestUtilities.Builders;
using pix_pagador_testes.TestUtilities.Fixtures;


namespace pix_pagador_testes.Domain.Core.Common.Mediator;

public class BSMediatorTest
{
    private BSMediator _testClass;
    private Mock<IServiceProvider> _mockServiceProvider;
    private Mock<IBSRequestHandler<TestRequest, TestResponse>> _mockHandler;

    public BSMediatorTest()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockHandler = new Mock<IBSRequestHandler<TestRequest, TestResponse>>();

        _mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IBSRequestHandler<TestRequest, TestResponse>)))
            .Returns(_mockHandler.Object);

        _testClass = new BSMediator(_mockServiceProvider.Object);
    }

    [Fact]
    public void CanConstruct()
    {
        // Act
        var instance = new BSMediator(_mockServiceProvider.Object);

        // Assert
        Assert.NotNull(instance);
    }



    [Fact]
    public async Task SendCallsCorrectHandler()
    {
        // Arrange
        var request = new TestRequest { Data = "test-data" };
        var expectedResponse = new TestResponse { Result = "test-result" };
        var cancellationToken = CancellationToken.None;

        _mockHandler
            .Setup(h => h.Handle(request, cancellationToken))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _testClass.Send<TestRequest, TestResponse>(request, cancellationToken);

        // Assert
        Assert.Equal(expectedResponse, result);
        _mockHandler.Verify(h => h.Handle(request, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task SendWithoutCancellationTokenUsesDefault()
    {
        // Arrange
        var request = new TestRequest { Data = "test-data" };
        var expectedResponse = new TestResponse { Result = "test-result" };

        _mockHandler
            .Setup(h => h.Handle(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _testClass.Send<TestRequest, TestResponse>(request);

        // Assert
        Assert.Equal(expectedResponse, result);
        _mockHandler.Verify(h => h.Handle(request, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task SendThrowsWhenHandlerNotFound()
    {
        // Arrange
        var request = new TestRequest { Data = "test-data" };

        _mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IBSRequestHandler<TestRequest, TestResponse>)))
            .Returns(null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _testClass.Send<TestRequest, TestResponse>(request));

        Assert.Contains("Nenhum handler encontrado", exception.Message);
        Assert.Contains("TestRequest", exception.Message);
        Assert.Contains("IBSRequestHandler", exception.Message);
    }

    [Fact]
    public async Task SendCachesHandlerType()
    {
        // Arrange
        var request1 = new TestRequest { Data = "test-data-1" };
        var request2 = new TestRequest { Data = "test-data-2" };
        var response = new TestResponse { Result = "test-result" };

        _mockHandler
            .Setup(h => h.Handle(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        await _testClass.Send<TestRequest, TestResponse>(request1);
        await _testClass.Send<TestRequest, TestResponse>(request2);

        // Assert - GetService should be called twice (once for each request)
        _mockServiceProvider.Verify(
            sp => sp.GetService(typeof(IBSRequestHandler<TestRequest, TestResponse>)),
            Times.Exactly(2));
    }

    // Test classes for testing
    public class TestRequest : IBSRequest<TestResponse>
    {
        public string Data { get; set; }
    }

    public class TestResponse
    {
        public string Result { get; set; }
    }
}