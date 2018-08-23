using Newtonsoft.Json;

namespace org.healthwise.ops.nugetsync.Configuration
{
    public class ReplicationPair
    {
        [JsonProperty("description")]
        public string Description { get; set; }
        
        [JsonProperty("type")]
        public string Type { get; set; }
        
        [JsonProperty("source")]
        public ReplicationServer Source { get; set; }
        
        [JsonProperty("destination")]
        public ReplicationServer Destination { get; set; }

    }
}