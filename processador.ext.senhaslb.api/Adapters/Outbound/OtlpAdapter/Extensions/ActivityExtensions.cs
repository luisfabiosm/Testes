using System.Diagnostics;

namespace Adapters.Outbound.OtlpAdapter.Extensions
{
    public static class ActivityExtensions
    {
        public static void AddTags(this Activity activity, IDictionary<string, object> tags)
        {
            if (activity == null || tags == null) return;

            foreach (var tag in tags)
            {
                activity.SetTag(tag.Key, tag.Value);
            }
        }
    }
}
