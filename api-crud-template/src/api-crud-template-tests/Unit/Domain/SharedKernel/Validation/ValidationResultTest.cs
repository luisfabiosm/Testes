
using Domain.Core.SharedKernel.Validation;
using FluentAssertions;


namespace api_crud_template_tests.Unit.Domain.SharedKernel.Validation
{
    public class ValidationResultTest
    {
        private ValidationResult _testClass;
        private bool _IsValid;
        private List<ValidationError> _Errors;
      

        public ValidationResultTest()
        {
            _Errors = new List<ValidationError>
            {
                new ValidationError("Property1", "Error message 1"),
                new ValidationError("Property2", "Error message 2", "InvalidValue")
            };
            _IsValid = true;
            _testClass = ValidationResult.Success();
        }


        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = ValidationResult.Success();

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public void CanConstructSuccess()
        {
            // Act
            var instance = ValidationResult.Success();

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public void CanConstructFailure()
        {
            // Act
            var instance = ValidationResult.Failure(_Errors);

            // Assert
            Assert.NotNull(instance);
            Assert.Equal(instance.Errors, _Errors);
            instance.Errors.Should().NotBeNull();

        }

        [Fact]
        public void CanGetErrorsAsString_ShouldReturnErrorString()
        {
            // Arrange
            var errors = new List<ValidationError>
            {
                new ValidationError("Property1", "Error message 1"),
                new ValidationError("Property2", "Error message 2", "InvalidValue")
            };
            var instance = ValidationResult.Failure(errors);

            // Act
            var result = instance.ErrorsAsString;

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
            result.Should().Be(instance.ErrorsAsString);

        }

        [Fact]
        public void CanGetErrorsAsString_ShouldReturnEmptyString()
        {
            // Arrange
            var errors = new List<ValidationError>();
            var instance = ValidationResult.Failure(errors);

            // Act
            var result = instance.ErrorsAsString;

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            result.Should().Be(string.Empty);

        }

    }
}
