using Domain.UseCases.ProcessarTransacaoSenhaSilabica;
using System.Diagnostics;

namespace Domain.Core.Ports.UseCases
{
    public interface IProcessadorSenhaSilabicaPort
    {
        Task ProcessarTransacao(TransacaoSenhaSilabica transacao, Activity parentActivity, CancellationToken cancellationToken);
    }
}
