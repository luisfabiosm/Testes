namespace Adapters.Inbound.HttpAdapters.VM
{
    public record TransacaoSpaRequest
    {
        public int Transacao { get; set; }
        public string? CabecalhoSPA { get; set; }
        public string? BufferMessage { get; set; }
        public byte[]? Buffer { get; set; }
        public TransacaoSpaRequest() { }
    }
}
