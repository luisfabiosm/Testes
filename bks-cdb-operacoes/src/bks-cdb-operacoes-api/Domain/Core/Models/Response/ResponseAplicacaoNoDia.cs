
using Domain.Core.Common.Transaction;

namespace Domain.Core.Models.Response
{
    public record ResponseAplicacaoNoDia : BaseTransactionResponse
    {
        public List<ResultResponseAplicacaoNoDia> Result { get; set; }


        public ResponseAplicacaoNoDia(string result)
        {
            if (string.IsNullOrWhiteSpace(result))
            {
                Result = new List<ResultResponseAplicacaoNoDia>();
                return;
            }

          
            // Processa formato delimitado
            Result = ParseDelimitedData(result);
        }


        private List<ResultResponseAplicacaoNoDia> ParseDelimitedData(string data)
        {
            var resultList = new List<ResultResponseAplicacaoNoDia>();

            // Delimitador final de registro
            const string recordDelimiter = "!@";

            // Separa os registros
            var records = data.Split(new[] { recordDelimiter }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var record in records)
            {
                if (string.IsNullOrWhiteSpace(record))
                    continue;

                // Separa os campos por pipe |
                var fields = record.Split('|');

                // Verifica se tem a quantidade correta de campos (6 campos esperados)
                if (fields.Length >= 6)
                {
                    try
                    {
                        var item = new ResultResponseAplicacaoNoDia
                        {
                            // Campo 1: CodigoCarteira
                            Codcar = ParseInt(fields[0]),

                            // Campo 2: Numero identificador da operação (usamos como NumOpe)
                            NumOpe = ParseInt(fields[1]),

                            // Campo 3: IdentificacaoPapel
                            idComPap = fields[2]?.Trim(),

                            // Campo 4: Numest não vem no layout, deixamos null
                            Numest = ParseInt(fields[3]),

                            // Campo 5: Valor
                            VrFinIda = ParseDecimal(fields[4]),

                            // Campo 6: DataAtual (usamos como Dtapl)
                            Dtapl = ParseDate(fields[5]),

                            // Campo 7: DataVencimento
                            Dtvcto = ParseDate(fields[6]),

                            // Campo 8: NSUAutenticacao
                            NSUAut = ParseInt(fields[7]),    
                        };

                        resultList.Add(item);
                    }
                    catch (Exception ex)
                    {
                        // Log do erro ou tratamento conforme necessário
                        // Pode continuar processando outros registros
                        Console.WriteLine($"Erro ao processar registro: {record}. Erro: {ex.Message}");
                    }
                }
            }

            return resultList;
        }

    }
    public record ResultResponseAplicacaoNoDia
    {
        public int Codcar { get; set; }
        public int NumOpe { get; set; }
        public string? idComPap { get; set; }
        public int Numest { get; set; }
        public decimal VrFinIda { get; set; } 
        public DateTime Dtapl { get; set; }
        public DateTime Dtvcto { get; set; }
        public int NSUAut { get; set; }
    }
}
