using Adapters.Outbound.SenhaAlfaAdapter.Models;
using Refit;

namespace Domain.Core.Ports.Outbound
{
    [Headers("Accept: application/json", "Content-Type: application/json")]

    public interface IRefitClientSA
    {
        [Post("/GerarSaidaSenha")]
        Task<GerarSaidaSenhaResponse> GerarSaidaSenha([Body] SenhaAlfaRequest request);

        [Post("/TestarSenha")]
        Task<TestarSenhaResponse> TestarSenha([Body] SenhaAlfaRequest request);
    }
}
