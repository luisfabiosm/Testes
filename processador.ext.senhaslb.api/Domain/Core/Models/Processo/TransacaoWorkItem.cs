using Domain.UseCases.ProcessarTransacaoSenhaSilabica;

namespace Domain.Core.Models.Processo
{
    public class TransacaoWorkItem
    {
        public TransacaoSenhaSilabica Transacao { get; }
        public CancellationToken CancellationToken { get; }
        public string ActivityId { get; }

        public TransacaoWorkItem(TransacaoSenhaSilabica transacao, string parentActivity, CancellationToken cancellationToken)
        {
            Transacao = transacao;
            ActivityId = parentActivity;
            CancellationToken = cancellationToken;
        }
    }
}
