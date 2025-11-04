using gg.parse.rules;
using gg.parse.script;

namespace gg.parse.argparser
{
    public static class ParserBuilderExtension
    {
        public static string GetReport(this ParserBuilder builder, Exception? e = null, LogLevel level = LogLevel.Info )
        {
            var logs = builder.LogHandler?.ReceivedLogs?.Where(l => (l.level & level) > 0);

            if (logs != null && logs.Any())
            {
                return string.Join("\n", logs);
            }

            return  e == null ? "" : e.Message;
        }
    }
}
