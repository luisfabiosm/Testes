using System.Text.Json;
using System.Text.Json.Serialization;

namespace Domain.Core.Common.Serialization;


public static class JsonOptions
{
    /// <summary>
    /// Opções padrão para serialização JSON com Source Generators.
    /// Configuração otimizada para alta performance e baixo uso de memória.
    /// </summary>
    public static readonly JsonSerializerOptions Default = new()
    {
        // Performance Optimizations
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false, // Reduz tamanho do payload
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, // Reduz payload

        // Type Info Resolver com Source Generators (CRÍTICO para performance)
        TypeInfoResolver = ApiJsonContext.Default,

        // Configurações de Converter
        Converters =
            {
                new ByteToIntConverter(),
                //new JsonStringEnumConverter(JsonNamingPolicy.CamelCase), // Enums como string
            },

        // Configurações de Encoding
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        


        // Configurações de Parsing
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,

        // Configurações de Case Sensitivity
        PropertyNameCaseInsensitive = true,

        // Configurações de Numbers
        NumberHandling = JsonNumberHandling.AllowReadingFromString,

        // Performance: Evita reflection desnecessária
        IncludeFields = false,

        // Configurações de referência circular (se necessário)
        ReferenceHandler = null, // Mantém null para melhor performance

        // Configurações de buffer
        DefaultBufferSize = 16 * 1024, // 16KB buffer otimizado

    };

    /// <summary>
    /// Opções para desenvolvimento/debug com indentação.
    /// </summary>
    public static readonly JsonSerializerOptions Debug = new(Default)
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Default
    };

    /// <summary>
    /// Opções para serialização mínima (sem nulls, sem indentação).
    /// Ideal para logs e armazenamento.
    /// </summary>
    public static readonly JsonSerializerOptions Minimal = new(Default)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
        PropertyNamingPolicy = null // Mantém nomes originais
    };

    /// <summary>
    /// Opções para serialização com case sensitive.
    /// Para casos específicos que requerem exatidão de nomes.
    /// </summary>
    public static readonly JsonSerializerOptions CaseSensitive = new(Default)
    {
        PropertyNameCaseInsensitive = false,
        PropertyNamingPolicy = null
    };



    public class ByteToIntConverter : JsonConverter<byte>
    {
        public override byte Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number)
            {
                return (byte)reader.GetInt32();
            }
            return reader.GetByte();
        }

        public override void Write(Utf8JsonWriter writer, byte value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue((int)value);
        }
    }




}




