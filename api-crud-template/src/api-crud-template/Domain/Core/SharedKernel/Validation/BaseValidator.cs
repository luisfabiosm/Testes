using System.Text.RegularExpressions;

namespace Domain.Core.SharedKernel.Validation
{
    public abstract class BaseValidator<T>
    {
        protected readonly List<ValidationError> _errors = new();

        public ValidationResult Validate(T instance)
        {
            _errors.Clear();
            ValidateInternal(instance);

            return _errors.Any()
                ? ValidationResult.Failure(_errors.ToList())
                : ValidationResult.Success();
        }

        public async Task<ValidationResult> ValidateAsync(T instance, CancellationToken cancellationToken = default)
        {
            _errors.Clear();
            await ValidateInternalAsync(instance, cancellationToken);

            return _errors.Any()
                ? ValidationResult.Failure(_errors.ToList())
                : ValidationResult.Success();
        }

        protected abstract void ValidateInternal(T instance);

        protected virtual Task ValidateInternalAsync(T instance, CancellationToken cancellationToken)
        {
            ValidateInternal(instance);
            return Task.CompletedTask;
        }

        // ===== HELPER METHODS PARA VALIDAÇÃO =====

        protected void ValidateRequired(string? value, string propertyName, string? customMessage = null)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                var message = customMessage ?? $"{propertyName} é obrigatório";
                _errors.Add(new ValidationError(propertyName, message, value));
            }
        }

        protected void ValidateMinLength(string? value, int minLength, string propertyName, string? customMessage = null)
        {
            if (!string.IsNullOrEmpty(value) && value.Length < minLength)
            {
                var message = customMessage ?? $"{propertyName} deve ter pelo menos {minLength} caracteres";
                _errors.Add(new ValidationError(propertyName, message, value));
            }
        }

        protected void ValidateMaxLength(string? value, int maxLength, string propertyName, string? customMessage = null)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > maxLength)
            {
                var message = customMessage ?? $"{propertyName} deve ter no máximo {maxLength} caracteres";
                _errors.Add(new ValidationError(propertyName, message, value));
            }
        }

        protected void ValidateEmail(string? value, string propertyName, string? customMessage = null)
        {
            if (string.IsNullOrWhiteSpace(value))
                return; // Não valida email se valor estiver vazio (deixe para ValidateRequired)

            var emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            if (!Regex.IsMatch(value, emailPattern))
            {
                var message = customMessage ?? $"{propertyName} deve ter formato válido";
                _errors.Add(new ValidationError(propertyName, message, value));
            }
        }

        protected void ValidatePattern(string? value, string pattern, string propertyName, string? customMessage = null)
        {
            if (string.IsNullOrWhiteSpace(value))
                return; // Não valida padrão se valor estiver vazio

            if (!Regex.IsMatch(value, pattern))
            {
                var message = customMessage ?? $"{propertyName} não atende ao padrão exigido";
                _errors.Add(new ValidationError(propertyName, message, value));
            }
        }

        protected void ValidateRange<TValue>(TValue? value, TValue min, TValue max, string propertyName, string? customMessage = null)
            where TValue : IComparable<TValue>
        {
            if (value == null) return;

            if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
            {
                var message = customMessage ?? $"{propertyName} deve estar entre {min} e {max}";
                _errors.Add(new ValidationError(propertyName, message, value));
            }
        }

        protected void ValidateGreaterThan<TValue>(TValue? value, TValue threshold, string propertyName, string? customMessage = null)
            where TValue : IComparable<TValue>
        {
            if (value == null) return;

            if (value.CompareTo(threshold) <= 0)
            {
                var message = customMessage ?? $"{propertyName} deve ser maior que {threshold}";
                _errors.Add(new ValidationError(propertyName, message, value));
            }
        }

        protected void ValidateNotNull<TValue>(TValue? value, string propertyName, string? customMessage = null)
        {
            if (value == null)
            {
                var message = customMessage ?? $"{propertyName} não pode ser nulo";
                _errors.Add(new ValidationError(propertyName, message, value));
            }
        }

        protected void ValidateGuid(Guid? value, string propertyName, string? customMessage = null)
        {
            if (value == null || value == Guid.Empty)
            {
                var message = customMessage ?? $"{propertyName} deve ser um GUID válido";
                _errors.Add(new ValidationError(propertyName, message, value));
            }
        }

        protected void ValidateCustom(bool condition, string propertyName, string message, object? attemptedValue = null)
        {
            if (!condition)
            {
                _errors.Add(new ValidationError(propertyName, message, attemptedValue));
            }
        }
    }

}
