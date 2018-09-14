using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace org.healthwise.ops.nugetsync.Configuration
{
    public class MessageSetting
    {
        [JsonProperty("FunctionKey")]
        public string FunctionKey { get; set; }

        [JsonProperty("ServiceURL")]
        public string ServiceURL { get; set; }

        [JsonProperty("ChannelData")]
        public string ChannelData { get; set; }

        [JsonProperty("Environment")]
        public string Environment { get; set; }
    }
}
