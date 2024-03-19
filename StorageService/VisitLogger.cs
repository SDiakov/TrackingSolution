using Common;
using System.Text;

namespace StorageService
{
    public class VisitLogger : IMessageProcessor<VisitMessage>
    {
        private static object logLocker = new object();
        
        private readonly string filePath;

        public VisitLogger(string filePath)
        {
            this.filePath = filePath;
        }

        public void Process(VisitMessage message)
        {
            if (!string.IsNullOrEmpty(message.Ip))
            {
                lock(logLocker)
                {
                    var logMessage = BuildLogMessage(message);
                    Console.WriteLine(logMessage);

                    var fileDirectory = Path.GetDirectoryName(filePath);
                    if (!string.IsNullOrEmpty(fileDirectory))
                    {
                        Directory.CreateDirectory(fileDirectory);
                    }
                    File.AppendAllText(filePath, logMessage);
                }
            }
        }

        private string BuildLogMessage(VisitMessage message)
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

            return logMessageBuilder.ToString();
        }
    }
}
