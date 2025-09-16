

namespace Domain.Core.SharedKernel.Transactions
{
    public abstract record BaseTransaction
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
