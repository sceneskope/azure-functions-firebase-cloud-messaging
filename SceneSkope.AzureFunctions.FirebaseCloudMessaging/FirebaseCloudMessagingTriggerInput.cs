using Matrix.Xmpp.Client;

namespace SceneSkope.AzureFunctions.FirebaseCloudMessaging
{
    internal sealed class FirebaseCloudMessagingTriggerInput
    {
        public FirebaseCloudMessage Message { get; }

        public FirebaseCloudMessagingTriggerInput(FirebaseCloudMessage message)
        {
            Message = message;
        }

        public static FirebaseCloudMessagingTriggerInput New(FirebaseCloudMessage message) => new FirebaseCloudMessagingTriggerInput(message);
    }
}