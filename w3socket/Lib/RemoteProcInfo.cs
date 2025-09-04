

using System.Text.Json;

namespace W3Socket.Lib
{
    public class RemoteProcInfo
    {
        public string Name { get; set; }
        public object[] Params { get; set; }

        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }

        public static RemoteProcInfo FromJson(string json)
        {
            return JsonSerializer.Deserialize<RemoteProcInfo>(json);
        }
    }
}
