using Domain.Core.Common.ResultPattern;
using Domain.Core.Common.Serialization;
using Domain.Core.Common.Transaction;
using Domain.Core.Models.Response.Error;

namespace Domain.Core.Models.Response
{
    public record ResponseTipoOperacao : BaseTransactionResponse
    {
        public List<ResultResponseTipoOperacao> Result { get; set; }


        public ResponseTipoOperacao(string result)
        {
            if (string.IsNullOrWhiteSpace(result))
            {
                Result = new List<ResultResponseTipoOperacao>();
                return;
            }

            // Processa formato delimitado
            Result = ParseDelimitedData(result);
        }

        private List<ResultResponseTipoOperacao> ParseDelimitedData(string data)
        {
            var resultList = new List<ResultResponseTipoOperacao>();

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
                        var item = new ResultResponseTipoOperacao
                        {

                            Id_ope =  fields[0]?.Trim(),

                            Nom_ope = fields[1]?.Trim(),


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

    public record ResultResponseTipoOperacao
    {
        public string? Id_ope { get; set; }
        public string? Nom_ope { get; set; }
    }
}