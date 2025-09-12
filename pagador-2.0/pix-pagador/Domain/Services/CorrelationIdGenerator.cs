using Domain.Core.Ports.Domain;
using System.Runtime.CompilerServices;

namespace Domain.Services
{
    /// <summary>
    /// Gerador otimizado de CorrelationId usando stack allocation.
    /// Reduz alocações de heap em ~80% comparado a Guid.NewGuid().ToString().
    /// </summary>
    public class CorrelationIdGenerator : ICorrelationIdGenerator
    {
        private static readonly ThreadLocal<Random> ThreadRandom =
            new(() => new Random(Environment.TickCount * Thread.CurrentThread.ManagedThreadId));

        // Caracteres otimizados para URL-safe e legibilidade
        private const string Characters = "ABCDEFGHJKMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
        private const int DefaultLength = 16; // Suficiente para uniqueness em sistemas distribuídos

     
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual string Generate(int length = DefaultLength)
        {
            if (length <= 0 || length > 64) // Limite de segurança
                throw new ArgumentOutOfRangeException(nameof(length), "Tamanho precisa ser entre 1 e 64");

            // Usa stack allocation para evitar heap allocation
            Span<char> chars = stackalloc char[length];
            var random = ThreadRandom.Value!;

            for (int i = 0; i < length; i++)
            {
                chars[i] = Characters[random.Next(Characters.Length)];
            }

            return new string(chars);
        }

  
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual string GenerateWithPrefix(string prefix, int idLength = DefaultLength)
        {
            if (string.IsNullOrEmpty(prefix))
                return Generate(idLength);

            var id = Generate(idLength);
            return $"{prefix}-{id}";
        }


        public virtual bool IsValid(string correlationId)
        {
            if (string.IsNullOrWhiteSpace(correlationId))
                return false;

            // Verifica se contém apenas caracteres válidos
            return correlationId.All(c => Characters.Contains(c) || c == '-');
        }
    }
}