using Domain.Core.Common.ResultPattern;
using Domain.Core.Common.Serialization;
using Domain.Core.Common.Transaction;


namespace Domain.Core.Models.Response;

public record ResponsePapelDispAplic : BaseTransactionResponse
{
    public List<ResultPapelDispAplic> Result { get; set; }



    public ResponsePapelDispAplic(string result)
    {
        if (string.IsNullOrWhiteSpace(result))
        {
            Result = new List<ResultPapelDispAplic>();
            return;
        }


        // Processa formato delimitado
        Result = ParseDelimitedData(result);
    }


    private List<ResultPapelDispAplic> ParseDelimitedData(string data)
    {
        var resultList = new List<ResultPapelDispAplic>();

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
                    var item = new ResultPapelDispAplic
                    {
                        // Campo 1: CodigoCarteira
                        Codcar = ParseInt(fields[0]),
                        // Campo 2: IdentificacaoPapel
                        IdComPap = fields[1]?.Trim(),
                        // Campo 3: NOME CARTEIRA
                        nomCar = fields[2]?.Trim(),
                        // Campo 4: 
                        vr_min = ParseDecimal(fields[3]),
                        // Campo 5: 
                        vr_min_adi = ParseDecimal(fields[4]),
                        // Campo 6: 
                        vr_rsg_min = ParseDecimal(fields[5]),
                        // Campo 7: 
                        vr_min_pmc = ParseDecimal(fields[6]),
                        // Campo 8: 
                        sldcli = ParseDecimal(fields[7]),
                        // Campo 9: 
                        pubasemi = ParseDecimal(fields[8]),
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
public record ResultPapelDispAplic
{

    public int Codcar { get; set; }
    public string? IdComPap { get; set; }
    public string nomCar { get; set; }
    public decimal vr_min { get; set; }
    public decimal vr_min_adi { get; set; }
    public decimal vr_rsg_min { get; set; }
    public decimal vr_min_pmc { get; set; }
    public decimal sldcli { get; set; }
    public decimal pubasemi { get; set; }

}
