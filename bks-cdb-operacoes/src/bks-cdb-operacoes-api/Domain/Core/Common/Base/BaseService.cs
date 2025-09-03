using Domain.Core.Exceptions;
using Domain.Core.Ports.Outbound;

namespace Domain.Core.Common.Base
{
    public class BaseService
    {

        protected readonly ILoggingAdapter _loggingAdapter;

        public BaseService(IServiceProvider serviceProvider)
        {
            _loggingAdapter = serviceProvider.GetRequiredService<ILoggingAdapter>();
        }

    }
}
