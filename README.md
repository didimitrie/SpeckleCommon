# SpeckleCommon
Base .NET Speckle Clients + Speckle Converters

## SpeckleConverter
This is the base half-virtual class from which you should inherit/override in order to create your own data & geometry converters to speckle objects.

## Speckle Client
This is the base library that provides two classes that you should really care about: 

### Speckle Receiver
Exposes a series of events that get triggered when the sender emits data. 
```csharp
myReceiver = new SpeckleReceiver(API_URL, API_TOKEN, STREAM_TO_LISTEN_TO, GEOMETRY_CONVERTER);

// Events:

// triggered when errors are pooped in the pipes
myReceiver.OnError += OnError;
// triggered when component init is ready
myReceiver.OnReady += OnReady;

// you've got metadata
myReceiver.OnMetadata += OnMetadata;
// you've got both metadata and data
myReceiver.OnData += OnData;
// stream history was updated
myReceiver.OnHistory += OnHistory;
// direct message from another client.
myReceiver.OnMessage += OnVolatileMessage;
// broadcasted message from another client.
myReceiver.OnBroadcast += OnBroadcast;
```

### Speckle Sender
Exposes a series of methods that allow you to send data (metadata + geometry) as well as just metadata (layer names, etc). 

```csharp
mySender = new SpeckleSender(API_URL, API_TOKEN, GEOMETRY_CONVERTER);

// Events: 
mySender.OnError += OnError;
mySender.OnReady += OnReady;
mySender.OnDataSent += OnDataSent;
// direct message from another client.
myReceiver.OnMessage += OnVolatileMessage;
// broadcasted message from another client.
myReceiver.OnBroadcast += OnBroadcast;

// Methods:
// when sending geometry:
mySender.sendDataUpdate(DATA, LAYERS, NAME);
// when sending cosmetic changes:
mySender.sendMetadataUpdate(LAYERS, NAME);
```

Also exposed is a virtual class `Speckle Converter`. You need to implement this class if you want to be able to send data back and forth. It translates geometry from application_x format to a speckle intermediary format.  
