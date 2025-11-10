namespace Domain.Core.SharedKernel.Validation
{
    public class ValidationError
    {
        public string PropertyName { get; }
        public string Message { get; }
        public object? AttemptedValue { get; }

        public ValidationError(string propertyName, string message, object? attemptedValue = null)
        {
            PropertyName = propertyName;
            Message = message;
            AttemptedValue = attemptedValue;
        }


    }

}
