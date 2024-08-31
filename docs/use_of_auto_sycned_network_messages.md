# How to use `IAutoSynchronizedMessageHandler`
1. Inject `IAutoSynchronizedMessageHandler` into your class.

    ``` C#
    public YourService(IAutoSynchronizedMessageHandler autoSynchronizedMessageHandler)
	{
		_autoSynchronizedMessageHandler = autoSynchronizedMessageHandler;
	}
    ```

2. Subscribe to `MockMessage`
    Image you have a `MockMessage` (like ..)

    ``` C#   
	_subscribeId = _autoSynchronizedMessageHandler.Subscribe<MockMessage>(OnMockMessageReceived);
    ```

    **_Note:_** You should save the _subscribeId to unsubcribe if you want.
    ``` C#   
	_autoSynchronizedMessageHandler.Unsubscribe(_subscribeId);
    ```

3. Handle message received event
    ``` C#
    protected INetworkMessage? OnRegistrationRequestReceived(INetworkMessage message)
	{
		if (message is MockMessage mockMessage)
		{
            // Do something with the new message
		}

		return null; // Means that nothing should send as an awnser to this message
	}
    ```

    **_Note:_** If you want to send a message, then you should create your message object and return it.
