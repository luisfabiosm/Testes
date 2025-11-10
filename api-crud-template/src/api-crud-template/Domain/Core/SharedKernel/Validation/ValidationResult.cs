namespace Domain.Core.SharedKernel.Validation
{
    public class ValidationResult
    {
  
        public bool IsValid { get; private set; }
        public List<ValidationError> Errors { get; private set; }

        private ValidationResult(bool isValid, List<ValidationError> errors)
        {
            IsValid = isValid;
            Errors = errors;
        }

        public static ValidationResult Success() => new(true, new List<ValidationError>());

        public static ValidationResult Failure(List<ValidationError> errors) => new(false, errors);

        public static ValidationResult Failure(ValidationError error) => new(false, new List<ValidationError> { error });

        public string ErrorsAsString
        {
            get
            {
                return string.Join("; ", Errors.Select(e => e.Message));
            }
        }
    }

}
