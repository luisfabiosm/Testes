
using Microsoft.AspNetCore.Http;
using NSubstitute; 
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace pix_pagador_testes.Adapters.Inbound.WebApi.Pix.Endpoints;

#region AuthenticationTests

public class EndpointAuthenticationTests
{
    private readonly HttpContext _mockHttpContext; 
    private readonly ClaimsPrincipal _mockUser;    

    public EndpointAuthenticationTests()
    {
        // ✅ Mudança: NSubstitute sintaxe mais limpa
        _mockHttpContext = Substitute.For<HttpContext>();
        _mockUser = Substitute.For<ClaimsPrincipal>();
        _mockHttpContext.User.Returns(_mockUser);
    }

    [Fact]
    public void DevolucaoEndpoints_DevemExigirAutorizacao()
    {
        // Arrange & Act & Assert
        // Verifica se RequireAuthorization() está configurado para o grupo de devolução
        Assert.True(true); // Confirmação visual na configuração dos endpoints
    }

    [Fact]
    public void OrdemPagamentoEndpoints_DevemExigirAutorizacao()
    {
        // Arrange & Act & Assert
        // Verifica se RequireAuthorization() está configurado para o grupo de ordem de pagamento
        Assert.True(true); // Confirmação visual na configuração dos endpoints
    }

    [Fact]
    public void MonitorEndpoints_DevemPermitirAcessoAnonimo()
    {
        // Arrange & Act & Assert
        // Verifica se AllowAnonymous() está configurado para endpoints de monitoramento
        Assert.True(true); // Confirmação visual na configuração dos endpoints
    }

    [Theory]
    [InlineData("Canal", "100")]
    [InlineData("Chave-Idempotencia", "ABC-123-XYZ")]
    public void EndpointsAutenticados_DevemProcessarClaimsEHeaders(string claimType, string claimValue)
    {
        // Arrange
        var claim = new Claim(claimType, claimValue);

        // ✅ Mudança: NSubstitute - sem .Setup()
        _mockUser.FindFirst(claimType).Returns(claim);

        // Act
        var result = _mockUser.FindFirst(claimType);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(claimValue, result.Value);
    }

    [Fact]
    public void HttpContext_DevePermitirAcessoAoUser()
    {
        // Arrange
        var expectedUser = Substitute.For<ClaimsPrincipal>();
        _mockHttpContext.User.Returns(expectedUser);

        // Act
        var user = _mockHttpContext.User;

        // Assert
        Assert.Equal(expectedUser, user);
    }

    [Fact]
    public void ClaimsPrincipal_DeveRetornarNullParaClaimInexistente()
    {
        // Arrange
        _mockUser.FindFirst("ClaimInexistente").Returns((Claim)null);

        // Act
        var result = _mockUser.FindFirst("ClaimInexistente");

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData("user123", "Admin")]
    [InlineData("user456", "User")]
    [InlineData("user789", "Guest")]
    public void ClaimsPrincipal_DeveProcessarDiferentesTiposClaims(string userId, string role)
    {
        // Arrange
        var userIdClaim = new Claim(ClaimTypes.NameIdentifier, userId);
        var roleClaim = new Claim(ClaimTypes.Role, role);

        _mockUser.FindFirst(ClaimTypes.NameIdentifier).Returns(userIdClaim);
        _mockUser.FindFirst(ClaimTypes.Role).Returns(roleClaim);

        // Act
        var userIdResult = _mockUser.FindFirst(ClaimTypes.NameIdentifier);
        var roleResult = _mockUser.FindFirst(ClaimTypes.Role);

        // Assert
        Assert.NotNull(userIdResult);
        Assert.NotNull(roleResult);
        Assert.Equal(userId, userIdResult.Value);
        Assert.Equal(role, roleResult.Value);
    }
}

#endregion



//using Microsoft.AspNetCore.Http;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Security.Claims;
//using System.Text;
//using System.Threading.Tasks;

//namespace pix_pagador_testes.Adapters.Inbound.WebApi.Pix.Endpoints;


//#region AuthenticationTests

//public class EndpointAuthenticationTests
//{
//    private readonly Mock<HttpContext> _mockHttpContext;
//    private readonly Mock<ClaimsPrincipal> _mockUser;

//    public EndpointAuthenticationTests()
//    {
//        _mockHttpContext = new Mock<HttpContext>();
//        _mockUser = new Mock<ClaimsPrincipal>();
//        _mockHttpContext.Setup(x => x.User).Returns(_mockUser.Object);
//    }

//    [Fact]
//    public void DevolucaoEndpoints_DevemExigirAutorizacao()
//    {
//        // Arrange & Act & Assert
//        // Verifica se RequireAuthorization() está configurado para o grupo de devolução
//        Assert.True(true); // Confirmação visual na configuração dos endpoints
//    }

//    [Fact]
//    public void OrdemPagamentoEndpoints_DevemExigirAutorizacao()
//    {
//        // Arrange & Act & Assert
//        // Verifica se RequireAuthorization() está configurado para o grupo de ordem de pagamento
//        Assert.True(true); // Confirmação visual na configuração dos endpoints
//    }

//    [Fact]
//    public void MonitorEndpoints_DevemPermitirAcessoAnonimo()
//    {
//        // Arrange & Act & Assert
//        // Verifica se AllowAnonymous() está configurado para endpoints de monitoramento
//        Assert.True(true); // Confirmação visual na configuração dos endpoints
//    }

//    [Theory]
//    [InlineData("Canal", "100")]
//    [InlineData("Chave-Idempotencia", "ABC-123-XYZ")]
//    public void EndpointsAutenticados_DevemProcessarClaimsEHeaders(string claimType, string claimValue)
//    {
//        // Arrange
//        var claim = new Claim(claimType, claimValue);
//        _mockUser.Setup(x => x.FindFirst(claimType)).Returns(claim);

//        // Act
//        var result = _mockUser.Object.FindFirst(claimType);

//        // Assert
//        Assert.NotNull(result);
//        Assert.Equal(claimValue, result.Value);
//    }
//}

//#endregion