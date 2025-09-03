

using Domain.Core.Enum;

namespace Domain.Core.Exceptions
{

    public class BusinessException : Exception
    {

        public int ErrorCode { get; } = 400;

        public ErrorDetailsReturn BusinessError = null;


        public BusinessException(string mensagem) : base(mensagem)
        {

        }

        public static BusinessException Create(string mensagem, int codigo, string origem = "API")
        {
            var _bexception = new BusinessException(mensagem);
            _bexception.BusinessError = new ErrorDetailsReturn
            {
                codErro = codigo,
                msgErro = mensagem,
                origemErro = origem,
                tipoErro = (int)EnumTipoErro.NEGOCIO
            };
            return _bexception;
        }

        public static BusinessException Create(ErrorDetailsReturn spsReturn)
        {
            var _bexception = new BusinessException(spsReturn.msgErro);
            _bexception.BusinessError = spsReturn;
            return _bexception;
        }





    }
}