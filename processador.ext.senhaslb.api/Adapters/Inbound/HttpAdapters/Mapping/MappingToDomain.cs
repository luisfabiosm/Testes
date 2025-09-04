using Domain.UseCases.ProcessarTransacaoSenhaSilabica;
using Adapters.Inbound.HttpAdapters.VM;
using Domain.Core.Ports.Inbound;
using W3Socket.Core.Models.SPA;
using Newtonsoft.Json;
using System.Text;

namespace Adapters.Inbound.HttpAdapters.Mapping
{
    public class MappingToDomain : IMappingToDomainPort
    {
        private readonly List<string> _errors = new List<string>();
        public IReadOnlyList<string> Errors => _errors;

        public TransacaoSenhaSilabica ToTransacaoSPA(TransacaoSpaRequest request)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding windows1252 = Encoding.GetEncoding("Windows-1252");

            try
            {
                Validate(request);
            }
            catch (Exception ex)
            {
                throw new Domain.Core.Exceptions.HttpRequestException(ex.Message);
            }
            return new TransacaoSenhaSilabica
            {
                Codigo = request.Transacao,
                CabecalhoSPA = JsonConvert.DeserializeObject<tSPACabecalho>(request.CabecalhoSPA!),
                BufferMessage = windows1252.GetBytes(request.BufferMessage!),
            };
        }

        public string ToSerializedTransacaoSPA(TransacaoSpaRequest request)
        {
            return JsonConvert.SerializeObject(ToTransacaoSPA(request));
        }

        public void Validate(TransacaoSpaRequest request)
        {
            _errors.RemoveAll(item => item == null);

            if (_errors.Count > 0)
                throw new Domain.Core.Exceptions.HttpRequestException(_errors);
        }
    }
}
