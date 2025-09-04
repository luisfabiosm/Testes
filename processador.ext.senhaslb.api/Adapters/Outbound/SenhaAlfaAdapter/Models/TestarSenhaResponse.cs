using Domain.Core.Enums;

namespace Adapters.Outbound.SenhaAlfaAdapter.Models
{
    public class TestarSenhaResponse
    {
        public int validationResult { get; set; }
        public EnumStatus returnCode { get; set; }
    }
}
