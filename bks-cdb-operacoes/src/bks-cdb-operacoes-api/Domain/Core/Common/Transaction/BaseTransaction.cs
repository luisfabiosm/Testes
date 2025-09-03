using Domain.Core.Common.Mediator;
using Domain.Core.Enum;

namespace Domain.Core.Common.Transaction
{
    public abstract record BaseTransaction<TResponse> : IBSRequest<TResponse>
    {
        public  int Code { get; init; }
        public string CorrelationId { get; set; } = string.Empty; // Será definido nos endpoints
        public  int canal { get; set; }
        public  string chaveIdempotencia { get; set; }
        public int pintContaAg { get; set; }
        public int pintConta { get; set; }
        public int ptinTitularidade { get; set; }
        private EnumTipoLista ptinTipoLista { get; set; }

        public void SetTipoLista(EnumTipoLista tpLista)
        {
            this.ptinTipoLista = tpLista;
        }
        public string pvchListaCarteira { get; set; }

        public BaseTransaction()
        {

        }

        public abstract string getTransactionSerialization();

    }
}
