using Domain.Core.Models.Transactions;
using System.Buffers;
using System.Text;
using System.Text.Json;

namespace Domain.Core.Common.Serialization;

/// <summary>
/// Métodos de extensão otimizados para serialização JSON.
/// Reduz alocações de memória usando ArrayPool e Span<T>.
/// </summary>
public static class JsonExtensions
{
    private static readonly ArrayPool<byte> ByteArrayPool = ArrayPool<byte>.Shared;

    /// <summary>
    /// Deserializa JSON string para objeto usando Source Generators otimizados.
    /// </summary>
    /// <typeparam name="T">Tipo do objeto esperado</typeparam>
    /// <param name="json">JSON string para deserializar</param>
    /// <param name="options">Opções de deserialização (opcional)</param>
    /// <returns>Objeto deserializado</returns>
    public static T? FromJsonOptimized<T>(this string json, JsonSerializerOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(json)) return default;

        options ??= JsonOptions.Default;

        return JsonSerializer.Deserialize<T>(json, options);
    }

    /// <summary>
    /// Versão otimizada para deserialização usando Span<T> quando possível.
    /// </summary>
    /// <typeparam name="T">Tipo do objeto esperado</typeparam>
    /// <param name="json">JSON string para deserializar</param>
    /// <param name="options">Opções de deserialização (opcional)</param>
    /// <returns>Objeto deserializado</returns>
    public static T? FromJsonOptimizedSpan<T>(this string json, JsonSerializerOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(json)) return default;

        options ??= JsonOptions.Default;

        // Para strings pequenas, usa stack allocation
        if (json.Length <= 256)
        {
            Span<byte> utf8Bytes = stackalloc byte[Encoding.UTF8.GetByteCount(json)];
            Encoding.UTF8.GetBytes(json, utf8Bytes);
            return JsonSerializer.Deserialize<T>(utf8Bytes, options);
        }

        // Para strings maiores, usa ArrayPool
        var maxByteCount = Encoding.UTF8.GetMaxByteCount(json.Length);
        var rentedBuffer = ByteArrayPool.Rent(maxByteCount);

        try
        {
            var actualByteCount = Encoding.UTF8.GetBytes(json, rentedBuffer);
            return JsonSerializer.Deserialize<T>(rentedBuffer.AsSpan(0, actualByteCount), options);
        }
        finally
        {
            ByteArrayPool.Return(rentedBuffer);
        }
    }


    /// <summary>
    /// Verifica se uma string é um JSON válido sem alocar objetos.
    /// </summary>
    /// <param name="json">String JSON a ser validada</param>
    /// <returns>true se JSON válido, false caso contrário</returns>
    public static bool IsValidJson(this string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return false;

        try
        {
            // ✅ CORREÇÃO: Usar scopes separados para evitar CS8352/CS8347
            if (json.Length <= 512) // Para strings pequenas, usa stack allocation
            {
                Span<byte> buffer = stackalloc byte[Encoding.UTF8.GetByteCount(json)];
                Encoding.UTF8.GetBytes(json, buffer);
                var reader = new Utf8JsonReader(buffer);
                while (reader.Read()) { } // Consume todo o JSON
                return true;
            }
            else // Para strings maiores, aloca no heap
            {
                var utf8Json = Encoding.UTF8.GetBytes(json);
                var reader = new Utf8JsonReader(utf8Json);
                while (reader.Read()) { } // Consume todo o JSON
                return true;
            }
        }
        catch (JsonException)
        {
            return false;
        }
        catch (Exception) // Captura erros de encoding também
        {
            return false;
        }
    }

    /// <summary>
    /// Método de validação mais otimizado usando TryParse pattern.
    /// </summary>
    /// <param name="json">String JSON a ser validada</param>
    /// <param name="errorMessage">Mensagem de erro se inválido</param>
    /// <returns>true se JSON válido, false caso contrário</returns>
    public static bool TryValidateJson(this string json, out string? errorMessage)
    {
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            errorMessage = "JSON string is null or empty";
            return false;
        }

        try
        {
            var utf8Json = Encoding.UTF8.GetBytes(json);
            var reader = new Utf8JsonReader(utf8Json);

            while (reader.Read()) { }

            return true;
        }
        catch (JsonException ex)
        {
            errorMessage = $"Invalid JSON: {ex.Message}";
            return false;
        }
        catch (Exception ex)
        {
            errorMessage = $"Error processing JSON: {ex.Message}";
            return false;
        }
    }



    /// <summary>
    /// Método auxiliar para serialização com buffer customizado.
    /// Útil quando você sabe o tamanho aproximado do JSON.
    /// </summary>
    /// <typeparam name="T">Tipo do objeto</typeparam>
    /// <param name="value">Objeto a ser serializado</param>
    /// <param name="estimatedSize">Tamanho estimado em bytes</param>
    /// <param name="options">Opções de serialização</param>
    /// <returns>JSON string serializada</returns>
    public static string ToJsonOptimizedWithSize<T>(this T value, int estimatedSize, JsonSerializerOptions? options = null)
    {
        if (value == null) return "null";

        options ??= JsonOptions.Default;

        // Usa o tamanho fornecido + margem de segurança
        var bufferSize = Math.Max(estimatedSize, 1024) + 512;
        var rentedBuffer = ByteArrayPool.Rent(bufferSize);

        try
        {
            using var memoryStream = new MemoryStream(rentedBuffer);
            using var writer = new Utf8JsonWriter(memoryStream);
            JsonSerializer.Serialize(writer, value, options);
            writer.Flush();

            var bytesWritten = (int)memoryStream.Position;
            return Encoding.UTF8.GetString(rentedBuffer.AsSpan(0, bytesWritten));
        }
        finally
        {
            ByteArrayPool.Return(rentedBuffer);
        }
    }

    public static string ToJsonOptimized<T>(this T value, JsonSerializerOptions? options = null)
    {
        if (value == null) return "null";

        options ??= JsonOptions.Default;


        var bufferSize = EstimateBufferSize<T>();
        var rentedBuffer = ByteArrayPool.Rent(bufferSize);
        try
        {
            using var memoryStream = new MemoryStream(rentedBuffer);
            using var writer = new Utf8JsonWriter(memoryStream);
            JsonSerializer.Serialize(writer, value, options);
            writer.Flush();
            var bytesWritten = (int)memoryStream.Position;
            return Encoding.UTF8.GetString(rentedBuffer.AsSpan(0, bytesWritten));
        }
        finally
        {
            ByteArrayPool.Return(rentedBuffer);
        }
    }

    private static int EstimateBufferSize<T>()
    {
        var type = typeof(T);

        // Estimativas baseadas no tamanho típico dos objetos da aplicação
        return type.Name switch
        {

            nameof(TransactionConsultaAplicacaoDia) => 4 * 1024,            // 4KB - objeto médio
            nameof(TransactionConsultaCarteiraAplicacao) => 4 * 1024,       // 4KB - objeto médio
            nameof(TransactionConsultaListaOperacoes) => 4 * 1024,          // 4KB - objeto médio
            nameof(TransactionConsultaPapeisCarteira) => 4 * 1024,          // 4KB - objeto médio
            nameof(TransactionConsultaPapelDispAplic) => 4 * 1024,          // 4KB - objeto médio
            nameof(TransactionConsultarAplicacaoPorTipoPapel) => 4 * 1024,  // 4KB - objeto médio
            nameof(TransactionConsultaSaldoTotalPapel) => 4 * 1024,         // 4KB - objeto médio

            _ when type.Name.Contains("Response") => 4 * 1024,              // 4KB - responses
            _ when type.Name.Contains("Request") => 6 * 1024,               // 6KB - requests
            _ when type.Name.Contains("BaseReturn") => 4 * 1024,            // 4KB - base returns
            _ => 16 * 1024 // 16KB - padrão seguro para tipos desconhecidos
        };
    }
}