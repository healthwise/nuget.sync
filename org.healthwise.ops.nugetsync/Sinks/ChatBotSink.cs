using System;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json.Linq;
using Serilog.Core;
using Serilog.Events;

namespace org.healthwise.ops.nugetsync
{
    public class ChatBotSink : ILogEventSink
    {
        private readonly string _FunctionKey;
        private readonly string _ServiceURL;
        private readonly string _ChannelData;
        private readonly string _Environment;
        private readonly IFormatProvider _formatProvider;

        public ChatBotSink(IFormatProvider formatProvider, string FunctionKey, string ServiceURL, string ChannelData, string Environment)
        {
            _formatProvider = formatProvider;
            _FunctionKey = FunctionKey;
            _ServiceURL = ServiceURL;
            _ChannelData = ChannelData;
            _Environment = Environment;
        }

        // Construct a sink that passes messages to the specificed chat account.
        public void Emit(LogEvent logEvent)
        {
            // Ensure only Error and Fatal level messages are sent to the sink
            if (logEvent.Level != LogEventLevel.Error && logEvent.Level != LogEventLevel.Fatal)
                return;

            JObject json = JObject.FromObject(new
            {
                type = "message",
                serviceURL = _ServiceURL,
                channelData = new {teamsChannelId = _ChannelData},
                message = logEvent.RenderMessage(_formatProvider)
            });

            HttpContent content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                Uri baseUrl = new Uri($"https://hw-{_Environment}-botservice-unclehab.azurewebsites.net");
                client.BaseAddress = baseUrl; //Base part of URL (like https://api.healthwise.org)

                string restOfUrl = $"/api/MessageProxy?code={_FunctionKey}";
                var result = client.PostAsync(restOfUrl, content).Result; //Rest of url - API endpoint - /my/endpoint

            }
        }
    }
}
