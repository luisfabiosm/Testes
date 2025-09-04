using System.ComponentModel;

namespace Domain.Core.Enums
{
    public enum EnumMetodoAcao : int
    {
        [Description("Validar")]
        ACAO_VALIDAR = 0,
        [Description("Executar")]
        ACAO_EXECUTAR = 1,
        [Description("Confirmar")]
        ACAO_CONFIRMAR = 2,
        [Description("Cancelar")]
        ACAO_CANCELAR = 3,
        [Description("Estornar")]
        ACAO_ESTORNAR = 4
    }
}
