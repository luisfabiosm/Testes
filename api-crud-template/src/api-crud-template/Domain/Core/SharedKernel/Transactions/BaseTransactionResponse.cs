namespace Domain.Core.SharedKernel.Transactions
{
    public record BaseTransactionResponse
    {
        public string CorrelationId { get; set; }

        public string chvAutorizador { get; set; }

    }
}
