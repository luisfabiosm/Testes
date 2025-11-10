using Domain.Core.SharedKernel.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_crud_template_tests.Fixtures
{
    public static class TestValidatorModelFixtures
    {
        public class TestModel
        {
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
        }

        public class SimpleTestValidator : BaseValidator<TestModel>
        {
            public bool ValidateInternalCalled { get; private set; }
            public bool ValidateInternalAsyncCalled { get; private set; }
            public TestModel? LastValidatedInstance { get; private set; }

            protected override void ValidateInternal(TestModel instance)
            {
                ValidateInternalCalled = true;
                LastValidatedInstance = instance;
                // Implementação vazia para testes básicos
            }

            protected override async Task ValidateInternalAsync(TestModel instance, CancellationToken cancellationToken)
            {
                ValidateInternalAsyncCalled = true;
                LastValidatedInstance = instance;
                await Task.Delay(1, cancellationToken); // Simular operação assíncrona
            }

            // Método para expor a lista de erros para teste
            public List<ValidationError> GetErrors() => _errors;

            // Método para adicionar erro manualmente para teste
            public void AddTestError(string propertyName, string message)
            {
                _errors.Add(new ValidationError(propertyName, message, null));
            }
        }

        public class EmptyTestValidator : BaseValidator<TestModel>
        {
            protected override void ValidateInternal(TestModel instance)
            {
                // Implementação completamente vazia
            }
        }
    }
}
