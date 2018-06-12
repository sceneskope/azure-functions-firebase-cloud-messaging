# azure-functions-firebase-cloud-messaging
Firebase cloud messaging for Azure functions

This is a basic way of using Firebase Cloud Messaging as a trigger for Azure Functions.

I couldn't work out how to get the properties of the message exposed nicely as method parameters, so everything comes across in a `FirebaseCloudMessage`

The `FirebaseCloudMessage` contains:
    `From`: The sender of the message
    `Category`: The category of the message
    `MessageId`: The message id
    `Data`: The data as a `JObject`

The listener _should_ handle acks and draining correctly, but let's see what happens when it's used in anger.

The simple example in [TestFcm](TestFcm/) should help.


