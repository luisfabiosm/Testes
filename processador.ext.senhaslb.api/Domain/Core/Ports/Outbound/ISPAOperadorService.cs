using Adapters.Outbound.DBAdapter.Model;
using Domain.Core.Models.SPA;
using Domain.Core.Enums;
using Domain.Core.Base;

namespace Domain.Core.Ports.Outbound
{
    public interface ISPAOperadorService
    {
        SPATransacao GetTransacaoAtiva();
        EnumAcao RecuperarAcao();
        Task<BaseReturn> IniciarTransacao(BaseTransacao transacao, string[]? dadosTransacao = null);
        Task<IEnumerable<RetornoSPX>> MontarChamadaSPX(ParametrosSPX parametros);
        Task ValidarTransacao();
        Task ExecutarTransacao();
        Task ConfirmarTransacao();
        Task CancelarTransacao();
        string GetRetornoSPA();
        Task RecuperaSituacao();
    }
}
