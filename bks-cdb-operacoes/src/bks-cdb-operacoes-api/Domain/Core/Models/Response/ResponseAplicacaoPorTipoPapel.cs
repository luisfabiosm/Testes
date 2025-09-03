using Domain.Core.Common.Serialization;
using Domain.Core.Common.Transaction;
using Domain.Core.Models.Response.Error;

namespace Domain.Core.Models.Response
{
    public record ResponseAplicacaoPorTipoPapel : BaseTransactionResponse
    {
        public List<ResultResponseAplicacaoPorTipoPapel> Result { get; set; }


        public ResponseAplicacaoPorTipoPapel(string result)
        {
            if (string.IsNullOrWhiteSpace(result))
            {
                Result = new List<ResultResponseAplicacaoPorTipoPapel>();
                return;
            }


            // Processa formato delimitado
            Result = ParseDelimitedData(result);
        }


        private List<ResultResponseAplicacaoPorTipoPapel> ParseDelimitedData(string data)
        {
            var resultList = new List<ResultResponseAplicacaoPorTipoPapel>();

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
                        var item = new ResultResponseAplicacaoPorTipoPapel
                        {
                            // Campo 1: CodigoCarteira
                            Codcar = ParseInt(fields[0]),

                            // Campo 2: IdentificacaoPapel
                            idComPap = fields[1]?.Trim(),

                            // Campo 3: Numero identificador da operação (usamos como NumOpe)
                            NumOpe = ParseInt(fields[2]),

                            // Campo 4: Numest não vem no layout, deixamos null
                            Numest = ParseInt(fields[3]),

                            // Campo 5:
                            Salatu = ParseDecimal(fields[4]),

                            // Campo 6: DataAtual (usamos como Dtapl)
                            Dtapl = ParseDate(fields[5]),

                            // Campo 7: DataVencimento
                            Dtvcto = ParseDate(fields[6]),

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
    public record ResultResponseAplicacaoPorTipoPapel
    {
        public int Codcar { get; set; }
        public string? idComPap { get; set; }
        public int NumOpe { get; set; }
        public int Numest { get; set; }
        public decimal Salatu { get; set; }
        public DateTime Dtapl { get; set; }
        public DateTime Dtvcto { get; set; }
    }
}
