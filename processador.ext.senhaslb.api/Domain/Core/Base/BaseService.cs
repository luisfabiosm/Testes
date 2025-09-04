using Domain.Core.Ports.Outbound;
using Domain.Core.Exceptions;
using Domain.Core.Models.SPA;
using System.Data.SqlClient;
using System.Diagnostics;
using Domain.Core.Enums;

namespace Domain.Core.Base
{
    public class BaseService
    {
        #region variáveis

        internal readonly string SPA_OPENSHIFT_SOURCE_IN_ERROR = "@ProcessadorSPA";
        internal readonly string MSG_TRAN_SEPCAMPOS = ((char)11).ToString();

        #endregion

        public ILogger<BaseService>? _logger;
        protected readonly IOtlpServicePort _otlpSource;

        public BaseService(IServiceProvider serviceProvider)
        {
            _otlpSource = serviceProvider.GetRequiredService<IOtlpServicePort>();
        }

        public void LogInformation(string operation, string information)
        {
            _otlpSource.LogInformation(operation, information);
        }

        public void LogError(string operation, string error)
        {
            _otlpSource.LogError(operation, error);
        }

        public void LogError(string operation, BaseReturn baseReturn)
        {
            using (var activity = _otlpSource.GetOTLPSource($"ERRO {baseReturn.InternalError!.Value.Tipo}", ActivityKind.Internal))
            {
                activity?.SetStatus(ActivityStatusCode.Error);
                activity?.SetTag("OrigemErro", operation);
                activity?.SetTag("MensagemErro", baseReturn.InternalError.Value.Mensagem);
                activity?.SetTag("CodigoErro", baseReturn.InternalError.Value.Codigo);
                activity?.SetTag("TipoErro", (int)baseReturn.InternalError.Value.Tipo);
            }
        }

        public void LogError(string operation, Exception ex)
        {
            using (var activity = _otlpSource.GetOTLPSource("ERRO", ActivityKind.Internal))
            {
                activity?.SetStatus(ActivityStatusCode.Error);
                if (ex is SPAException spaEx)
                {
                    activity?.SetTag("OrigemErro", ex.Source);
                    activity?.SetTag("MensagemErro", spaEx.Error.Mensagem);
                    activity?.SetTag("CodigoErro", spaEx.Error.Codigo);
                    activity?.SetTag("TipoErro", (int)spaEx.Error.Tipo);
                    return;
                }

                activity?.SetTag("OrigemErro", operation);
                activity?.SetTag("MensagemErro", ex.Message);
                activity?.SetTag("CodigoErro", 0);
                activity?.SetTag("TipoErro", (int)EnumSPATipoErroInterno.Geral);
            }
        }

        protected SPAException handleError(Exception ex, string methodOnError)
        {
            if (ex is SqlException sqlEx)
            {
                var spaEx = new SPAException(new SPAError(sqlEx));
                spaEx.Origem = $"SPAOperadorService@{methodOnError}@{ex.Source}";
                return spaEx;
            }

            if (ex is SPAException)
            {
                var spaEx = (SPAException)ex;
                spaEx.Origem = $"SPAOperadorService@{methodOnError}";
                return spaEx;
            }

            return new SPAException(ex, methodOnError);
        }
    }
}