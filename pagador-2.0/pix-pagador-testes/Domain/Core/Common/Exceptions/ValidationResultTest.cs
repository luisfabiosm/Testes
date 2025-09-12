using Domain.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pix_pagador_testes.Domain.Core.Common.Exceptions
{
    public class ValidationResultTest
    {
        [Fact]
        public void CanCreateValidResult()
        {
            // Act
            var result = ValidationResult.Valid();

            // Assert
            Assert.True(result.IsValid);
            Assert.NotNull(result.Errors);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void CanCreateInvalidResultWithErrorList()
        {
            // Arrange
            var errors = new List<ErrorDetails>
            {
                new ErrorDetails("field1", "Error message 1"),
                new ErrorDetails("field2", "Error message 2")
            };

            // Act
            var result = ValidationResult.Invalid(errors);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal(2, result.Errors.Count);
            Assert.Equal("field1", result.Errors[0].campo);
            Assert.Equal("Error message 1", result.Errors[0].mensagens);
            Assert.Equal("field2", result.Errors[1].campo);
            Assert.Equal("Error message 2", result.Errors[1].mensagens);
        }

        [Fact]
        public void CanCreateInvalidResultWithSingleError()
        {
            // Arrange
            var message = "Single error message";
            var field = "testField";

            // Act
            var result = ValidationResult.Invalid(message, field);

            // Assert
            Assert.False(result.IsValid);
            Assert.Single(result.Errors);
            Assert.Equal(field, result.Errors[0].campo);
            Assert.Equal(message, result.Errors[0].mensagens);
        }

        [Fact]
        public void CanCreateInvalidResultWithSingleErrorAndDefaultField()
        {
            // Arrange
            var message = "Single error message";

            // Act
            var result = ValidationResult.Invalid(message);

            // Assert
            Assert.False(result.IsValid);
            Assert.Single(result.Errors);
            Assert.Equal("Unknown", result.Errors[0].campo);
            Assert.Equal(message, result.Errors[0].mensagens);
        }

        [Fact]
        public void CanCombineValidResults()
        {
            // Arrange
            var result1 = ValidationResult.Valid();
            var result2 = ValidationResult.Valid();
            var result3 = ValidationResult.Valid();

            // Act
            var combined = ValidationResult.Combine(result1, result2, result3);

            // Assert
            Assert.True(combined.IsValid);
            Assert.Empty(combined.Errors);
        }

        [Fact]
        public void CanCombineInvalidResults()
        {
            // Arrange
            var errors1 = new List<ErrorDetails> { new ErrorDetails("field1", "Error 1") };
            var errors2 = new List<ErrorDetails> { new ErrorDetails("field2", "Error 2") };
            var result1 = ValidationResult.Invalid(errors1);
            var result2 = ValidationResult.Invalid(errors2);

            // Act
            var combined = ValidationResult.Combine(result1, result2);

            // Assert
            Assert.False(combined.IsValid);
            Assert.Equal(2, combined.Errors.Count);
            Assert.Contains(combined.Errors, e => e.campo == "field1" && e.mensagens == "Error 1");
            Assert.Contains(combined.Errors, e => e.campo == "field2" && e.mensagens == "Error 2");
        }

        [Fact]
        public void CanCombineMixedResults()
        {
            // Arrange
            var validResult = ValidationResult.Valid();
            var invalidResult = ValidationResult.Invalid("Error message", "errorField");

            // Act
            var combined = ValidationResult.Combine(validResult, invalidResult);

            // Assert
            Assert.False(combined.IsValid);
            Assert.Single(combined.Errors);
            Assert.Equal("errorField", combined.Errors[0].campo);
            Assert.Equal("Error message", combined.Errors[0].mensagens);
        }

        [Fact]
        public void CanConvertToResultPatternForValidResult()
        {
            // Arrange
            var validationResult = ValidationResult.Valid();
            var testValue = "test-value";

            // Act
            var result = validationResult.ToResult(testValue);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(testValue, result.Value);
        }

        [Fact]
        public void CanConvertToResultPatternForInvalidResult()
        {
            // Arrange
            var errors = new List<ErrorDetails>
            {
                new ErrorDetails("field1", "Error 1"),
                new ErrorDetails("field2", "Error 2")
            };
            var validationResult = ValidationResult.Invalid(errors);
            var testValue = "test-value";

            // Act
            var result = validationResult.ToResult(testValue);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("Error 1", result.Error);
            Assert.Contains("Error 2", result.Error);
            Assert.Equal(default, result.Value);
        }

        [Fact]
        public void CombineHandlesEmptyArray()
        {
            // Act
            var combined = ValidationResult.Combine();

            // Assert
            Assert.True(combined.IsValid);
            Assert.Empty(combined.Errors);
        }

        [Fact]
        public void CombineHandlesSingleResult()
        {
            // Arrange
            var singleResult = ValidationResult.Invalid("Single error", "singleField");

            // Act
            var combined = ValidationResult.Combine(singleResult);

            // Assert
            Assert.False(combined.IsValid);
            Assert.Single(combined.Errors);
            Assert.Equal("singleField", combined.Errors[0].campo);
            Assert.Equal("Single error", combined.Errors[0].mensagens);
        }

        [Fact]
        public void CombinePreservesAllErrors()
        {
            // Arrange
            var result1 = ValidationResult.Invalid(new List<ErrorDetails>
            {
                new ErrorDetails("field1", "Error 1"),
                new ErrorDetails("field2", "Error 2")
            });
            var result2 = ValidationResult.Invalid(new List<ErrorDetails>
            {
                new ErrorDetails("field3", "Error 3")
            });
            var result3 = ValidationResult.Valid();

            // Act
            var combined = ValidationResult.Combine(result1, result2, result3);

            // Assert
            Assert.False(combined.IsValid);
            Assert.Equal(3, combined.Errors.Count);
            Assert.Contains(combined.Errors, e => e.campo == "field1");
            Assert.Contains(combined.Errors, e => e.campo == "field2");
            Assert.Contains(combined.Errors, e => e.campo == "field3");
        }

        [Fact]
        public void ToResultJoinsErrorsWithSemicolon()
        {
            // Arrange
            var errors = new List<ErrorDetails>
            {
                new ErrorDetails("field1", "First error"),
                new ErrorDetails("field2", "Second error"),
                new ErrorDetails("field3", "Third error")
            };
            var validationResult = ValidationResult.Invalid(errors);

            // Act
            var result = validationResult.ToResult("test");

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("First error", result.Error);
            Assert.Contains("Second error", result.Error);
            Assert.Contains("Third error", result.Error);
            Assert.Contains(";", result.Error);
        }
    }

}
