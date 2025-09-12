using Domain.Core.Common.Serialization;
using Domain.Core.Common.Transaction;

namespace Domain.Core.Models.Response
{
    public record JDPIRegistrarOrdemDevolucaoResponse : BaseTransactionResponse
    {
        public bool pixInterno { get; set; }
        public JDPIRegistrarOrdemDevolucaoResponse()
        {
            
        }
        public JDPIRegistrarOrdemDevolucaoResponse(string result)
        {

            var _result = result.FromJsonOptimized<JDPIRegistrarOrdemDevolucaoResponse>(JsonOptions.Default);
            if (_result == null) return;

            pixInterno = _result.pixInterno;


        }
    }
}
