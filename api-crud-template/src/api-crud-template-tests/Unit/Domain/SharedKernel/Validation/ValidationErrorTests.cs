using Domain.Core.Models.Response;
using Domain.Core.SharedKernel.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_crud_template_tests.Unit.Domain.SharedKernel.Validation
{
    public class ValidationErrorTests
    {
        private ValidationError _testClass;
        private string _PropertyName;
        private string _Message;
        private object? _AttemptedValue;

        public ValidationErrorTests()
        {

            _PropertyName = "Property1";
            _Message = "Error message 1";
            _AttemptedValue = "Invalid";

            _testClass = new ValidationError(_PropertyName, _Message, _AttemptedValue);
        }

        [Fact]
        public void CanInitialize()
        {
            // Act
            var instance = new ValidationError(_PropertyName, _Message, _AttemptedValue);

            // Assert
            Assert.NotNull(instance);
        }


    }
}
