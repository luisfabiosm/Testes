using Adapters.Outbound.SenhaAlfaAdapter.Models;

namespace Domain.Core.Ports.Outbound
{
    public interface ISAServicePort
    {
        public Task<GerarSaidaSenhaResponse> ExecutarApiExtGerarSaidaSenha(string msgOut);
        public Task<TestarSenhaResponse> ExecutarApiExtTestarSenha(string msgOut);
    }
}
