using Common;
using System.Text;

namespace StorageService
{
    public class VisitLogger : IMessageProcessor<VisitMessage>
    {
        private readonly string filePath;

        public VisitLogger(string filePath)
        {
            this.filePath = filePath;
        }

        public void Process(VisitMessage message)
        {
            if (!string.IsNullOrEmpty(message.Ip))
            {
                const string delimiter = "|";
                var logMessageBuilder = new StringBuilder();
                logMessageBuilder.Append(!string.IsNullOrEmpty(message.DateTime) ? message.DateTime : "null");
                logMessageBuilder.Append(delimiter);
                logMessageBuilder.Append(!string.IsNullOrEmpty(message.Referrer) ? message.Referrer : "null");
                logMessageBuilder.Append(delimiter);
                logMessageBuilder.Append(!string.IsNullOrEmpty(message.UserAgent) ? message.UserAgent : "null");
                logMessageBuilder.Append(delimiter);
                logMessageBuilder.Append(message.Ip);
                logMessageBuilder.Append(Environment.NewLine);

                var logMessage = logMessageBuilder.ToString();

                Console.WriteLine(logMessage);
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                File.AppendAllText(filePath, logMessage);
            }
        }
    }
}
