using Serilog;
using Serilog.Configuration;
using System;

namespace org.healthwise.ops.nugetsync.Sinks
{
    public static class ChatBotSinkExtensions
    {
        public static LoggerConfiguration ChatBotSink(
                  this LoggerSinkConfiguration loggerConfiguration,
                  IFormatProvider formatProvider = null,
                  string functionKey = null,
                  string serviceURL = null,
                  string channelData = null,
                  string environment = null)
        {
            return loggerConfiguration.Sink(new ChatBotSink(formatProvider, functionKey, serviceURL, channelData, environment));
        }
    }
}
