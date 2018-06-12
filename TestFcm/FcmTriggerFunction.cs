using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SceneSkope.AzureFunctions.FirebaseCloudMessaging;
using System.Xml.Linq;

namespace TestFcm
{
    public static class FcmTriggerFunction
    {
        private static readonly XName GcmName = XName.Get("gcm", "google:mobile:data");

        [FunctionName("FcmTriggerFunction")]
        public static void Run([FirebaseCloudMessagingTrigger(Connection = "FcmConnectionString")]FirebaseCloudMessage message, ILogger logger)
        {
            logger.LogInformation("Got message {Id}: {Data}", message.MessageId, message.Data);
        }

    }
}
