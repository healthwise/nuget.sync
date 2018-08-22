using Newtonsoft.Json;

namespace org.healthwise.ops.nugetsync.Configuration
{
    public class ReplicationServer
    {
        [JsonProperty("url")]
        public string Url { get; set; }
        
        [JsonProperty("token")]
        public string Token { get; set; }
        
        [JsonProperty("username")]
        public string Username { get; set; }
        
        [JsonProperty("password")]
        public string Password { get; set; }
    }
}