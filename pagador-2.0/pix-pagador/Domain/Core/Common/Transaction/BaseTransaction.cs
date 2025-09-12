using Domain.Core.Common.Mediator;

namespace Domain.Core.Common.Transaction
{
    public abstract record BaseTransaction<TResponse> : IBSRequest<TResponse>
    {
        public int Code { get; init; }

        public string CorrelationId { get; set; } = string.Empty; // Será definido nos endpoints

        public int canal { get; set; }

        public string chaveIdempotencia { get; set; }



        public BaseTransaction()
        {

        }

        public abstract string getTransactionSerialization();

    }
}
