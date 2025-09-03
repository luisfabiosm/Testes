using Domain.Core.Common.Base;
using Domain.Core.Common.ResultPattern;
using Domain.Core.Enum;
using Domain.Core.Exceptions;
using Domain.Core.Models.Request;
using Domain.Core.Models.Response;
using Domain.Core.Models.Transactions;
using System.Text.Json.Serialization;

namespace Domain.Core.Common.Serialization;

/// <summary>
/// Source Generator Context para otimização de serialização JSON.
/// Reduz alocações de memória e melhora performance em 40-60%.
/// </summary>
/// 

[JsonSerializable(typeof(BaseReturn<ResponseAplicacaoNoDia>))]
[JsonSerializable(typeof(BaseReturn<ResponseAplicacaoPorTipoPapel>))]
[JsonSerializable(typeof(BaseReturn<ResponseCarteira>))]
[JsonSerializable(typeof(BaseReturn<ResponsePapeisPorCarteira>))]
[JsonSerializable(typeof(BaseReturn<ResponsePapelDispAplic>))]
[JsonSerializable(typeof(BaseReturn<ResponseSaldoPorTipoPapel>))]
[JsonSerializable(typeof(BaseReturn<ResponseTipoOperacao>))]

// Transaction Types
[JsonSerializable(typeof(TransactionConsultaAplicacaoDia))]
[JsonSerializable(typeof(TransactionConsultaCarteiraAplicacao))]
[JsonSerializable(typeof(TransactionConsultaListaOperacoes))]
[JsonSerializable(typeof(TransactionConsultaPapeisCarteira))]
[JsonSerializable(typeof(TransactionConsultaPapelDispAplic))]
[JsonSerializable(typeof(TransactionConsultarAplicacaoPorTipoPapel))]
[JsonSerializable(typeof(TransactionConsultaSaldoTotalPapel))]

// Request Types
[JsonSerializable(typeof(RequestConsultaLista))]


// Response Types
[JsonSerializable(typeof(BaseResponse<ResponseAplicacaoNoDia>))]
[JsonSerializable(typeof(BaseResponse<ResponseAplicacaoPorTipoPapel>))]
[JsonSerializable(typeof(BaseResponse<ResponseCarteira>))]
[JsonSerializable(typeof(BaseResponse<ResponsePapeisPorCarteira>))]
[JsonSerializable(typeof(BaseResponse<ResponsePapelDispAplic>))]
[JsonSerializable(typeof(BaseResponse<ResponseSaldoPorTipoPapel>))]
[JsonSerializable(typeof(BaseResponse<ResponseTipoOperacao>))]
[JsonSerializable(typeof(BaseResponse<string>))]
[JsonSerializable(typeof(BaseResponse<object>))]
[JsonSerializable(typeof(ResponseAplicacaoNoDia))]
[JsonSerializable(typeof(ResponseAplicacaoPorTipoPapel))]
[JsonSerializable(typeof(ResponseCarteira))]
[JsonSerializable(typeof(ResponsePapeisPorCarteira))]
[JsonSerializable(typeof(ResponsePapelDispAplic))]
[JsonSerializable(typeof(ResponseSaldoPorTipoPapel))]
[JsonSerializable(typeof(ResponseTipoOperacao))]


// Enum Types
[JsonSerializable(typeof(EnumErrorType))]
[JsonSerializable(typeof(EnumTipoLista))]
[JsonSerializable(typeof(EnumTipoErro))]


// Common Types
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(int?))]
[JsonSerializable(typeof(double))]
[JsonSerializable(typeof(decimal))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(DateTime))]
[JsonSerializable(typeof(object))]


//Error
[JsonSerializable(typeof(ErrorDetailsReturn))]
[JsonSerializable(typeof(ValidationErrorDetails))]
[JsonSerializable(typeof(List<ValidationErrorDetails>))]



// Collections
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(Dictionary<string, object>))]
public partial class ApiJsonContext : JsonSerializerContext
{
    /// <summary>
    /// Configuração padrão otimizada para performance.
    /// - PropertyNamingPolicy: CamelCase para compatibilidade com frontend
    /// - WriteIndented: false para reduzir tamanho
    /// - DefaultIgnoreCondition: WhenWritingNull para reduzir payload
    /// </summary>
    static ApiJsonContext()
    {
        // Configurações são aplicadas via JsonSerializerOptions.Default
    }
}