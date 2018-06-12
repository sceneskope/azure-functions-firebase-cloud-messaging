using Newtonsoft.Json.Linq;

namespace SceneSkope.AzureFunctions.FirebaseCloudMessaging
{
    public class FirebaseCloudMessage
    {
        public string From { get; }
        public string Category { get; }
        public string MessageId { get; }
        public JObject Data { get; }

        public FirebaseCloudMessage(string from, string category, string messageId, JObject data)
        {
            From = from;
            Category = category;
            MessageId = messageId;
            Data = data;
        }
    }
}
