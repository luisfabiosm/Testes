using Adapters.Outbound.DBAdapter.Model;
using Domain.Core.Models.SPA;
using Domain.Core.Enums;
using Domain.Core.Base;

namespace Domain.Core.Ports.Outbound
{
    public interface ISPARepository
    {
        ValueTask IniciarSPATransacao(int agencia, int posto, SPATransacao spaTransacao);
        ValueTask<BaseReturn> ExecutaDB(SPATransacao spaTransacao);
        ValueTask<EnumSPASituacaoTransacao> RecuperarSituacao(SPATransacao spaTransacao);
        Task<IEnumerable<RetornoSPX>> ExecutarSPXIdentificaCartao(ParametrosSPX parametros);
        Task<IEnumerable<RetornoSPX>> ExecutarSPXSenhaSilabica(ParametrosSPX parametros);
    }
}
