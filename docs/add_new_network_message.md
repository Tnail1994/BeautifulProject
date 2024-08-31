# Add new network message

1. Create a class with specific value. Here `MessageMockValue` is not shown.

    ``` C#
    public class MessageMock : NetworkMessage<MessageMockValue>
    {
        public static MessageMock Create()
        {
            var messageMockValue = new MessageMockValue();
            return new RegistrationReply(messageMockValue);
        }
    }
    ```

    **_Hint:_** Do not create a constructor, which constructor arguments are not registered the container.

 2. Add the new message object to the container.

    ``` C#
    services.AddTransient<INetworkMessage, MessageMock>();
    ```

       **_Note:_** The `ITransformerService` needs the `INetwork` registration to automatically transform messages to its specific value.

