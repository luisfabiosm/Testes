using api_crud_template_tests.Fixtures;
using Domain.Core.Enums;
using Domain.Core.Models.Entities;
using Domain.Core.Models.Response;
using System.Diagnostics;
using T = System.String;


namespace api_crud_template_tests.Unit.Domain.Core.Models
{
    public class ApiResponseTests
    {

        private ApiResponse<T> _testClass;

        private T _data;
        private bool _Success;
        private string _Message;
        private DateTime _timestamp;
        private string _requestId;

        public ApiResponseTests()
        {
            _testClass = new ApiResponse<T>();
            _data = "TestValue123456";
            _Success = true;
            _Message = "TestErrorMessage";
            _timestamp = DateTime.UtcNow;
            _requestId = Activity.Current?.Id ?? Guid.NewGuid().ToString();
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new ApiResponse<T>();

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public void CanInitialize()
        {
            // Act
            var instance = new ApiResponse<T>
            {
                Success = _Success,
                Data = _data,
                Message = _Message,
                Timestamp = _timestamp,
                RequestId = _requestId
            };

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public void CanCallOnSuccess()
        {
            // Arrange
            var data = TestUserFixtures.CreateValidUser;

            // Act
            var result = ApiResponse<User>.OnSuccess(data);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(data, result.Data);
            Assert.True(result.Success);
        }

        [Fact]
        public void CanCallOnError()
        {
            // Arrange
            var data = TestErrorFixtures.ErrorMessage;

            // Act
            var result = ApiResponse<string>.OnError(data, data);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(data, result.Data);
            Assert.False(result.Success);
        }
    }
}
