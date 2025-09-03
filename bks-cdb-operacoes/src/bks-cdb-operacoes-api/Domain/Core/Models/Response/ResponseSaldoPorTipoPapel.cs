using Domain.Core.Common.Serialization;
using Domain.Core.Common.Transaction;
using Domain.Core.Models.Response.Error;

namespace Domain.Core.Models.Response;


public record ResponseSaldoPorTipoPapel : BaseTransactionResponse
{
    public List<ResultResponseSaldoPorTipoPapel> Result { get; set; }


    public ResponseSaldoPorTipoPapel(string result)
    {
        if (string.IsNullOrWhiteSpace(result))
        {
            Result = new List<ResultResponseSaldoPorTipoPapel>();
            return;
        }

        // Processa formato delimitado
        Result = ParseDelimitedData(result);
    }

    private List<ResultResponseSaldoPorTipoPapel> ParseDelimitedData(string data)
    {
        var resultList = new List<ResultResponseSaldoPorTipoPapel>();

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
                    var item = new ResultResponseSaldoPorTipoPapel
                    {
                        // Campo 1: CodigoCarteira
                        Codcar = ParseInt(fields[0]),

                        // Campo 3: IdentificacaoPapel
                        IdComPap = fields[1]?.Trim(),

                        Salatu = fields[2]?.Trim(),

                        salbr = fields[3]?.Trim(),

                        vr_rsg_min = ParseDecimal(fields[4]),

                        // Campo 5: Valor
                        vr_min_pmc = ParseDecimal(fields[5]),
           
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


public record ResultResponseSaldoPorTipoPapel
{
    public int Codcar { get; set; }
    public string? IdComPap { get; set; }
    public string? Salatu { get; set; }
    public string? salbr { get; set; }
    public decimal? vr_rsg_min { get; set; }
    public decimal? vr_min_pmc { get; set; }
}