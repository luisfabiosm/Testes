namespace Domain.Core.Base
{
    public abstract record BaseTransacao
    {
        public int Codigo { get; set; }
        public ReadOnlyMemory<char> MensagemIN { get; set; }
        public ReadOnlyMemory<char> MensagemOUT { get; set; }
    }
}
