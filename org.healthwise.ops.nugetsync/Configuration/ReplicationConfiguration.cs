using System.Collections.Generic;
using Newtonsoft.Json;

namespace org.healthwise.ops.nugetsync.Configuration
{
    public class ReplicationConfiguration
    {
        public ReplicationConfiguration()
        {
            ReplicationPairs = new List<ReplicationPair>();
        }
        
        [JsonProperty("replicationPairs")]
        public List<ReplicationPair> ReplicationPairs { get; set; }

        [JsonProperty("messageSettings")]
        public MessageSetting MessageSetting { get; set; }
    }
}