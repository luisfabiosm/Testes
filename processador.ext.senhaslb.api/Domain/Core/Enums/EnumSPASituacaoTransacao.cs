namespace Domain.Core.Enums
{
    public enum EnumSPASituacaoTransacao : int
    {
        ErroConfiguracao = -1,
        Iniciada = 0,
        Executada = 1,
        Confirmada = 2,
        Cancelada = 9
    }
}
