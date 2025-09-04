using Domain.UseCases.ProcessarTransacaoSenhaSilabica;
using Adapters.Inbound.HttpAdapters.VM;

namespace Domain.Core.Ports.Inbound
{
    public interface IMappingToDomainPort
    {
        TransacaoSenhaSilabica ToTransacaoSPA(TransacaoSpaRequest request);
        string ToSerializedTransacaoSPA(TransacaoSpaRequest request);
    }
}
