using System.Data.SqlClient;
using System.Data;

namespace Domain.Core.Models.SPA
{
    public record SPAParametro
    {
        internal SqlParameter? _oSQLParameter = new SqlParameter();

        public SPAParametro(SqlParameter parameter, int indice, bool reservado = false)
        {
            this.Indice = indice;
            this._oSQLParameter = parameter;
            this.Reservado = reservado;
        }

        public SPAParametro(int indice, object valor, bool reservado = false)
        {
            this.Indice = indice;
            this.Valor = valor;
            this.Reservado = reservado;
        }

        public int Indice { get; set; }
        public bool Reservado { get; set; }
        public string Nome => _oSQLParameter!.ParameterName;
        public SqlDbType Tipo => _oSQLParameter!.SqlDbType;
        public ParameterDirection Direcao => _oSQLParameter!.Direction;

        public SqlParameter RecuperarSqlParameterInput()
        {
            return new SqlParameter(_oSQLParameter!.ParameterName,
                                    _oSQLParameter.SqlDbType,
                                    _oSQLParameter.Size,
                                    _oSQLParameter.Direction,
                                    _oSQLParameter.IsNullable,
                                    _oSQLParameter.Precision,
                                    _oSQLParameter.Scale, _oSQLParameter.SourceColumn,
                                    _oSQLParameter.SourceVersion,
                                    _oSQLParameter.Value);
        }

        public SqlParameter RecuperarSqlParameterOutput()
        {
            var x = new SqlParameter(_oSQLParameter!.ParameterName,
                                        _oSQLParameter.SqlDbType,
                                        _oSQLParameter.Size);

            x.Direction = _oSQLParameter.Direction;

            return x;
        }

        public object Valor
        {
            get => unbindParameter();
            set => bindParameter(value);
        }

        private object unbindParameter()
        {
            try
            {
                if (IsValueNullOrEmpty())
                {
                    return this.Indice <= 24
                        ? GetDefaultValueForType(_oSQLParameter!.SqlDbType)
                        : _oSQLParameter!.Value;
                }

                return ProcessValueByDbType();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Erro ao recuperar o parâmetro {_oSQLParameter!.ParameterName} com conteúdo {_oSQLParameter.Value?.ToString()}", ex);
            }
        }

        private bool IsValueNullOrEmpty()
        {
            return string.IsNullOrEmpty(_oSQLParameter!.Value?.ToString());
        }

        private object ProcessValueByDbType()
        {
            return _oSQLParameter!.SqlDbType switch
            {
                SqlDbType.Bit => ProcessBit(),
                SqlDbType.SmallInt or SqlDbType.Int => ProcessInteger(),
                SqlDbType.TinyInt or SqlDbType.Binary => ProcessTinyInt(),
                SqlDbType.Decimal => _oSQLParameter.Value,
                SqlDbType.Float => ProcessFloat(),
                SqlDbType.SmallDateTime or SqlDbType.DateTime => ProcessDateTime(),
                SqlDbType.VarChar or SqlDbType.Char or SqlDbType.NChar or SqlDbType.NVarChar => ProcessString(),
                _ => _oSQLParameter.Value
            };
        }

        #region Processamento

        private object ProcessBit()
        {
            var value = _oSQLParameter!.Value.ToString()!.ToLower();
            return (value == "0" || value == "false") ? 0 : 1;
        }

        private object ProcessInteger()
        {
            var value = _oSQLParameter!.Value.ToString();
            return (value == "0,00" || value == "") ? 0 : _oSQLParameter.Value;
        }

        private object ProcessTinyInt()
        {
            var value = _oSQLParameter!.Value.ToString();
            return (value == "0,00" || value == "") ? 0 : Convert.ToInt16(_oSQLParameter.Value);
        }

        private object ProcessFloat()
        {
            return Convert.ToDouble(_oSQLParameter!.Value).ToString("F2");
        }

        private object ProcessDateTime()
        {
            var value = _oSQLParameter!.Value.ToString();
            return value == "" ? new DateTime(1900, 1, 1) : DateTime.Parse(value!).ToString("dd/MM/yyyy HH:mm:ss");
        }

        private object ProcessString()
        {
            var value = _oSQLParameter!.Value?.ToString();
            return string.IsNullOrEmpty(value)
                ? GetDefaultValueForType(_oSQLParameter.SqlDbType)
                : _oSQLParameter.Value!;
        }

        #endregion

        private void bindParameter(object value)
        {
            try
            {
                //Somente fixos
                if (this.Indice <= 24)
                {
                    bind(value);
                    return;
                }
                else if ((this.Indice > 24) && (!string.IsNullOrWhiteSpace(value.ToString())))
                {
                    #pragma warning disable CS0252
                    if (this.Direcao == ParameterDirection.Input && this.Valor == "")
                    {
                        _oSQLParameter!.Value = value;
                        return;
                    }
                    #pragma warning restore CS0252

                    bind(value);

                    if (_oSQLParameter!.SqlDbType == SqlDbType.Decimal)
                        _oSQLParameter.SqlDbType = SqlDbType.Float;

                    return;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Erro ao preencher o parâmetro {_oSQLParameter!.ParameterName} com conteúdo {_oSQLParameter.Value ?? "null"}", ex);
            }
        }

        private void bind(object value)
        {
            _oSQLParameter!.Value = _oSQLParameter.SqlDbType switch
            {
                SqlDbType.Bit => (value.ToString()!.Equals("0") ? 0 : 1),
                SqlDbType.SmallInt or SqlDbType.Int => (value.Equals("0,00")) ? 0 : value,
                SqlDbType.TinyInt or SqlDbType.Binary => (value.Equals("0,00")) ? 0 : Convert.ToInt16(value),
                SqlDbType.Decimal => string.IsNullOrWhiteSpace(value.ToString()) ? value : ParseDecimal(value.ToString()!),
                SqlDbType.SmallDateTime or SqlDbType.DateTime => DateTime.ParseExact(value.ToString()!, "dd/MM/yyyy HH:mm:ss", null),
                _ => value
            };
        }

        public decimal ParseDecimal(string input)
        {
            input = input.Trim();

            if (input.EndsWith(".00"))
                input = input.Substring(0, input.Length - 3);

            if (input.EndsWith(".0"))
                input = input.Substring(0, input.Length - 2);

            return decimal.TryParse(input, out decimal result) ? Math.Abs(result) : 0;
        }

        private object GetDefaultValueForType(SqlDbType sqlDbType)
        {
            return sqlDbType switch
            {
                SqlDbType.SmallDateTime or SqlDbType.DateTime => new DateTime(1900, 01, 01),
                SqlDbType.VarChar or SqlDbType.Char or SqlDbType.NChar or SqlDbType.NVarChar => "",
                SqlDbType.Decimal => 0,
                SqlDbType.Bit => 0,
                _ => new Int32()
            };
        }

        ~SPAParametro()
        {
            this._oSQLParameter = null;
        }
    }

    public class ParametrosOutput
    {
        public string? pvchTrilha2 { get; set; }
        public string? pnumPAN { get; set; }
        public int pbitCartaoBanpara { get; set; }
        public int pbitSMARTCARD { get; set; }
        public string? psmlBanco { get; set; }
        public string? psmlProduto { get; set; }
        public string? psmlSubProduto { get; set; }
        public string? psmlContaAg { get; set; }
        public int ptinPostoAg { get; set; }
        public string? pnumConta { get; set; }
        public string? pvchTitular { get; set; }
        public int ptinTitular { get; set; }
        public int ptinVia { get; set; }
        public string? pchrValidade { get; set; }
        public string? pvchSenha { get; set; }
        public string? pchrTipoPessoa { get; set; }
        public string? psmlProdutoCliente { get; set; }
        public string? pvchDescAgencia { get; set; }
        public int ptinQtdProdVinculado { get; set; }
        public string? pvchListaProdutos { get; set; }
        public int pbitCartaoAtivo { get; set; }
        public int ptinSituacao2 { get; set; }
        public string? pchrOperacao { get; set; }
        public string? pvchCPF { get; set; }
        public string? pvchDOC { get; set; }
        public string? pvchNascimento { get; set; }
        public int ptinTipoSenha { get; set; }
        public string? pchrSituacao { get; set; }
        public string? pchrRecadAlfa { get; set; }
        public string? pchrRecadCliente { get; set; }
        public string? pchrPctTarifa { get; set; }
        public string? pvchTPD { get; set; }
        public string? pchrTPS { get; set; }
        public string? pchrP90 { get; set; }
        public string? pvchMsg { get; set; }
        public string? pvchSequencia1 { get; set; }
        public string? pvchSequencia2 { get; set; }
        public string? pvchSequencia3 { get; set; }
        public string? pvchGrupo { get; set; }
        public int pintToken { get; set; }
        public string? pchrSolicSC { get; set; }
        public string? pvchListaBeneficio { get; set; }
        public string? pvchDtCadAlfa { get; set; }
        public string? pvchDtVldAlfa { get; set; }
        public string? pchrIndBloqueio { get; set; }
        public int ptinIndBloqCadastro { get; set; }
        public int pbitMultiplo { get; set; }
        public string? pvchTelefone { get; set; }
        public string? pbitCartaoAdicional { get; set; }
        public string? pvchMsgBloqueio { get; set; }
        public string? pchrSolicMultiplo { get; set; }
        public string? pvchMsgNotificacao { get; set; }
        public string? pvchMsgOUT { get; set; }
    }
}
