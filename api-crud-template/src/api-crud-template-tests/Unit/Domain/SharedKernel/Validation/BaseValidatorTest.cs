using Domain.Core.Models.Response;
using Domain.Core.SharedKernel.Transactions;
using Domain.Core.SharedKernel.Validation;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
using System.Diagnostics;
using static api_crud_template_tests.Fixtures.TestValidatorModelFixtures;
using T = string;

namespace api_crud_template_tests.Unit.Domain.SharedKernel.Validation
{
    public class BaseValidatorTest
    {


        [Fact]
        public void CanConstructor()
        {
            // Act
            var validator = new SimpleTestValidator();

            // Assert
            validator.Should().NotBeNull();
            validator.GetErrors().Should().NotBeNull();
            validator.GetErrors().Should().BeEmpty();
        }

        [Fact]
        public void CanInitialize()
        {
            // Act
            var validator = new SimpleTestValidator();

            // Assert
            validator.GetErrors().Should().BeOfType<List<ValidationError>>();
            validator.GetErrors().Count.Should().Be(0);
        }

        [Fact]
        public void MultipleInstances_ShouldHaveIndependentErrorsLists()
        {
            // Arrange & Act
            var validator1 = new SimpleTestValidator();
            var validator2 = new SimpleTestValidator();

            validator1.AddTestError("Property1", "Error1");
            validator2.AddTestError("Property2", "Error2");

            // Assert
            validator1.GetErrors().Should().HaveCount(1);
            validator2.GetErrors().Should().HaveCount(1);
            validator1.GetErrors()[0].PropertyName.Should().Be("Property1");
            validator2.GetErrors()[0].PropertyName.Should().Be("Property2");
        }

        [Fact]
        public void Validate_ShouldCallValidateInternal()
        {
            // Arrange
            var validator = new SimpleTestValidator();
            var model = new TestModel { Name = "Test" };

            // Act
            validator.Validate(model);

            // Assert
            validator.ValidateInternalCalled.Should().BeTrue();
            validator.LastValidatedInstance.Should().Be(model);
        }

        [Fact]
        public void Validate_WithNoErrors_ShouldReturnSuccess()
        {
            // Arrange
            var validator = new EmptyTestValidator();
            var model = new TestModel();

            // Act
            var result = validator.Validate(model);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }


        [Fact]
        public void Validate_WithErrors_ShouldReturnFailure()
        {
            // Arrange
            var validator = new SimpleTestValidator();
            var model = new TestModel();

            validator.AddTestError("TestProperty", "Test error message");
           
            // Act
            var result = validator.Validate(model);

            // Assert
            result.Should().NotBeNull();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be("TestProperty");
            result.Errors[0].Message.Should().Be("Test error message");
        }
    }
}
