using System.ComponentModel;

namespace Domain.Core.Enums
{
    public enum EnumAcao : int
    {
        [Description("Validar")]
        ACAO_VALIDAR = 0,
        [Description("Executar")]
        ACAO_EXECUTAR = 3,
        [Description("Confirmar")]
        ACAO_CONFIRMAR = 4,
        [Description("Cancelar")]
        ACAO_CANCELAR = 8,
        [Description("Registrar")]
        ACAO_REGISTRAR = 16
    }
}
