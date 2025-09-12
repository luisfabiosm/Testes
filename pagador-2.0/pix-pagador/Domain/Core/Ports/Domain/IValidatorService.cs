using Domain.Core.Enum;
using Domain.Core.Exceptions;
using Domain.Core.Models.JDPI;

namespace Domain.Core.Ports.Domain
{
    public interface IValidatorService
    {
        (List<ErrorDetails> Errors, bool IsValid) ValidarPagador(JDPIDadosConta pagador);

        (List<ErrorDetails> Errors, bool IsValid) ValidarRecebedor(JDPIDadosConta pagador);

        (List<ErrorDetails> Errors, bool IsValid) ValidardtHrOp(string dtHrOp);


        (List<ErrorDetails> Errors, bool IsValid) ValidarMotivo(string motivo);


        (List<ErrorDetails> Errors, bool IsValid) ValidarValor(double valor);

        (List<ErrorDetails> Errors, bool IsValid) ValidarChaveIdempotencia(string chaveIdempotencia);

        (List<ErrorDetails> Errors, bool IsValid) ValidarEndToEndIdOriginal(string endToEndIdOriginal);

        (List<ErrorDetails> Errors, bool IsValid) ValidarEndToEndId(string endToEndId);

        (List<ErrorDetails> Errors, bool IsValid) ValidarCodigoDevolucao(string codigoDevolucao);

        (List<ErrorDetails> Errors, bool IsValid) ValidarIdReqSistemaCliente(string idReqSistemaCliente);

        (List<ErrorDetails> Errors, bool IsValid) ValidarTpIniciacao(EnumTpIniciacao tpIniciacao);

        (List<ErrorDetails> Errors, bool IsValid) ValidarPrioridadePagamento(EnumPrioridadePagamento? prioridadePagamento);

        (List<ErrorDetails> Errors, bool IsValid) ValidarTipoPrioridadePagamento(EnumTpPrioridadePagamento? tpPrioridadePagamento);

        (List<ErrorDetails> Errors, bool IsValid) ValidarFinalidade(EnumTipoFinalidade? finalidade);

        (List<ErrorDetails> Errors, bool IsValid) ValidarValorDevolucao(double valorDevolucao);


    }
}
