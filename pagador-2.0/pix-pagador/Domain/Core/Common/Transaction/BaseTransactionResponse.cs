namespace Domain.Core.Common.Transaction
{
    public record BaseTransactionResponse
    {
        public string CorrelationId { get; set; }

        public string chvAutorizador { get; set; }

    }
}
