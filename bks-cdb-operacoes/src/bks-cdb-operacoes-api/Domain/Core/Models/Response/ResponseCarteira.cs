using Domain.Core.Common.Serialization;
using Domain.Core.Common.Transaction;
using Domain.Core.Models.Response.Error;

namespace Domain.Core.Models.Response
{
    public record ResponseCarteira : BaseTransactionResponse
    {
        public List<ResultResponseCarteira> Result { get; set; }


        public ResponseCarteira(string result)
        {
            if (string.IsNullOrWhiteSpace(result))
            {
                Result = new List<ResultResponseCarteira>();
                return;
            }

            // Processa formato delimitado
            Result = ParseDelimitedData(result);
        }

        private List<ResultResponseCarteira> ParseDelimitedData(string data)
        {
            var resultList = new List<ResultResponseCarteira>();

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
                        var item = new ResultResponseCarteira
                        {
                            // Campo 1: CodigoCarteira
                            CodCar = ParseInt(fields[0]),
                            
                            nomCar = fields[1]?.Trim(),
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
    public record ResultResponseCarteira
    {
        public int CodCar { get; set; }
        public string? nomCar { get; set; }
    }
}