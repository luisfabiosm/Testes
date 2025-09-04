using W3Socket.Core.Models.SPA;
using Domain.Core.Models.SPA;
using System.Diagnostics;
using Domain.Core.Enums;
using Domain.Core.Base;
using System.Text;

namespace Domain.UseCases.ProcessarTransacaoSenhaSilabica
{
    public record TransacaoSenhaSilabica : BaseTransacao
    {
        #region variáveis

        private byte[]? _bufferMessage;
        private tSPACabecalho _cabecalhoSPA;
        private MAgentMensagem? _mAgentMessageIn;
        public static readonly string MSG_TRAN_SEPCAMPOS = ((char)11).ToString();
        private string? _correlationId;

        #endregion 

        public MAgentMensagem? SPAMensagemOUT { get; internal set; }
        public EnumMetodoAcao MetodoAcao => _mAgentMessageIn!.MetodoAcao;
        public MAgentMensagem SPAMensagemIN => _mAgentMessageIn!;
        public Activity? TranActivity { get; set; }

        public string CorrelationId
        {
            get
            {
                return _correlationId!;
            }
            set
            {
                _correlationId = value;
            }
        }

        public int Cracha { get; internal set; }
        public int OrigemTimeout { get; internal set; }
        public TransacaoSenhaSilabica() { }

        public TransacaoSenhaSilabica(string mensagem)
        {
            MensagemIN = mensagem.AsMemory(); ;
            MensagemOUT = null;
            Cracha = CabecalhoSPA.cracha;
            OrigemTimeout = CabecalhoSPA.timeOut;
        }

        public ReadOnlySpan<byte> BufferMessage
        {
            get => _bufferMessage;
            set
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                Encoding windows1252 = Encoding.GetEncoding("Windows-1252");

                _bufferMessage = value.ToArray(); // Se necessário, armazene a memória em uma matriz de bytes.
                MensagemIN = windows1252.GetString(value).AsMemory();

                _mAgentMessageIn = new MAgentMensagem(MensagemIN.Span);
                Codigo = _mAgentMessageIn.Transacao;
            }
        }

        public tSPACabecalho CabecalhoSPA
        {
            get { return _cabecalhoSPA; }
            set
            {
                _cabecalhoSPA = value;
                Cracha = CabecalhoSPA.cracha;
                OrigemTimeout = CabecalhoSPA.timeOut;
            }
        }
    }
}