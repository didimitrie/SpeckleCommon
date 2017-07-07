using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using WebSocketSharp;

namespace SpeckleCommon
{
    public class SpeckleSender
    {
        // connectivity:
        SpeckleServer Server;
        WebSocket Ws;

        // buckets:
        List<SpeckleLayer> Layers;
        List<object> Objects;
        string Name;

        // converer:
        SpeckleConverter Converter;

        // timers:
        int DataDebounceInterval = 1000;
        int ReconnDebounceInterval = 1000;
        int MetaDebounceInterval = 750;

        System.Timers.Timer IsReadyCheck;
        System.Timers.Timer WsReconnecter;
        System.Timers.Timer DataSender;
        System.Timers.Timer MetadataSender;

        /// <summary>
        /// Event emitted when everything is ready.
        /// </summary>
        public event SpeckleEvents OnReady;
        /// <summary>
        /// Event emitted when a volatile message was received.
        /// </summary>
        public event SpeckleEvents OnMessage;
        public event SpeckleEvents OnBroadcast;
        /// <summary>
        /// Event emitted when an api call returns.
        /// </summary>
        public event SpeckleEvents OnDataSent;
        public event SpeckleEvents OnError;


        SessionCache Cache;

        #region constructors

        /// <summary>
        /// Creates a new sender.
        /// </summary>
        /// <param name="_server">Speckle Server to connect to.</param>
        public SpeckleSender(string apiUrl, string token, SpeckleConverter _converter, string documentId = null, string documentName = null)
        {
            Converter = _converter;

            Server = new SpeckleServer(apiUrl, token);

            Cache = new SessionCache();

            Server.OnError += (sender, e) =>
            {
                OnError?.Invoke(this, new SpeckleEventArgs(e.EventInfo));
            };

            Server.OnReady += (e, data) =>
            {
                Setup();
            };
        }

        /// <summary>
        /// Creates a new sender from a previously serialised one.
        /// </summary>
        /// <param name="serialisedObject"></param>
        public SpeckleSender(string serialisedObject, SpeckleConverter _converter)
        {
            dynamic description = JsonConvert.DeserializeObject(serialisedObject);

            Server = new SpeckleServer((string)description.restEndpoint, (string)description.token, (string)description.streamId);

            Cache = new SessionCache();

            Server.OnError += (sender, e) =>
            {
                OnError?.Invoke(this, new SpeckleEventArgs(e.EventInfo));
            };

            Server.OnReady += (sender, e) =>
            {
                Setup();
            };

            Converter = _converter;
        }

        #endregion

        #region main setup calls

        private void Setup()
        {
            if (Server.StreamId == null)
                Server.CreateNewStream((success, data) =>
                {
                    if (!success)
                    {
                        OnError?.Invoke(this, new SpeckleEventArgs("Failed to create stream."));
                        return;
                    }
                    Server.StreamId = data.streamId;
                    SetupWebsocket();
                });
            else
                Server.GetStream((success, data) =>
                {
                    if (!success)
                    {
                        OnError?.Invoke(this, new SpeckleEventArgs("Failed to retrieve stream."));
                        return;
                    }
                    SetupWebsocket();
                });

            // start the is ready checker timer.
            // "ready" is defined as:
            // a) we have a valid stream id (means we've contacted the server sucessfully)
            // && b) we have a live wsSessionId (means sockets were correctly initialised)

            IsReadyCheck = new System.Timers.Timer(150)
            {
                AutoReset = false,
                Enabled = true
            };
            IsReadyCheck.Elapsed += (sender, e) =>
            {
                if (Server.StreamId == null) { IsReadyCheck.Start(); return; }
                if (Server.WsSessionId == null) { IsReadyCheck.Start(); return; }

                dynamic data = new ExpandoObject();
                data.streamId = Server.StreamId;
                data.wsSessionId = Server.WsSessionId;

                OnReady?.Invoke(this, new SpeckleEventArgs("sender ready", data));
            };

            // data sender debouncer
            DataSender = new System.Timers.Timer(DataDebounceInterval)
            {
                AutoReset = false,
                Enabled = false
            };
            DataSender.Elapsed +=
            (sender, e) =>
            {
                Debug.WriteLine("SPKSENDER: Sending data payload.");

                dynamic x = new ExpandoObject();
                List<SpeckleObject> convertedObjs = Converter.Convert(Objects);
                List<SpeckleObject> payload = new List<SpeckleObject>();
                convertedObjs.All(o =>
                {
                    if (o == null)
                    {
                        payload.Add(new SpeckleObject("invalid_object", ""));
                        return true;
                    }
                    if (o.Hash == null)
                        payload.Add(o);
                    else
                    if (Cache.IsInCache(o.Hash))
                    {
                        payload.Add(new SpeckleObject(o.Type, o.Hash));
                    }
                    else
                    {
                        payload.Add(o);
                        if (SpeckleConverter.HeavyTypes.Contains(o.Type))
                            Cache.AddToStage(o.Hash, o);
                    }

                    return true;
                });


                x.objects = payload;
                x.objectProperties = Converter.GetObjectProperties(Objects);
                x.layers = Layers;
                x.streamName = Name;

                Server.UpdateStream(x as ExpandoObject, (success, data) =>
                {
                    if (!success)
                    {
                        OnError?.Invoke(this, new SpeckleEventArgs("Failed to update stream."));
                        return;
                    }

                    // only commit to the cache if we had a positive res from the api
                    Cache.CommitStage();

                    OnDataSent?.Invoke(this, new SpeckleEventArgs("Stream was updated."));
                });
                DataSender.Stop();
            };

            // metadata sender debouncer
            MetadataSender = new System.Timers.Timer(MetaDebounceInterval)
            {
                AutoReset = false,
                Enabled = false
            };
            MetadataSender.Elapsed += (sender, e) =>
            {
                Debug.WriteLine("SPKSENDER: Sending meta payload.");

                dynamic payload = new ExpandoObject();
                payload.layers = Layers;
                payload.streamName = Name;

                Server.UpdateStreamMetadata(payload as ExpandoObject, (success, response) =>
                {
                    if (!success)
                    {
                        OnError?.Invoke(this, new SpeckleEventArgs("Failed to update stream metadata."));
                        return;
                    }
                    OnDataSent?.Invoke(this, new SpeckleEventArgs("Stream metadata was updated."));
                });
            };

        }

        private void SetupWebsocket()
        {
            WsReconnecter = new System.Timers.Timer(ReconnDebounceInterval)
            {
                AutoReset = false,
                Enabled = false
            };
            WsReconnecter.Elapsed += (sender, e) =>
            {
                Ws.Connect();
            };

            Ws = new WebSocket(Server.WsEndpoint + "?access_token=" + Server.Token);

            Ws.OnOpen += (sender, e) =>
            {
                WsReconnecter.Stop();
                Ws.Send(JsonConvert.SerializeObject(new { eventName = "join-stream", args = new { streamid = Server.StreamId, role = "sender" } }));
            };

            Ws.OnClose += (sender, e) =>
            {
                WsReconnecter.Start();
                Server.WsSessionId = null;
                OnError?.Invoke(this, new SpeckleEventArgs("Disconnected from server."));
            };

            Ws.OnMessage += (sender, e) =>
            {
                if (e.Data == "ping")
                {
                    Ws.Send("alive");
                    return;
                }
                dynamic message = JsonConvert.DeserializeObject(e.Data);

                if (message.eventName == "ws-session-id")
                {
                    Server.WsSessionId = message.sessionId;
                    return;
                }

                if (message.eventName == "volatile-message")
                {
                    OnMessage?.Invoke(this, new SpeckleEventArgs("volatile-message", message));
                    return;
                }

                if (message.eventName == "volatile-broadcast")
                {
                    OnBroadcast?.Invoke(this, new SpeckleEventArgs("volatile-broadcast", message));
                }
            };

            Ws.Connect();
        }

        #endregion

        #region public methods

        /// <summary>
        /// Sends a data update. 
        /// </summary>
        /// <param name="_objects">List of objects to convert and send.</param>
        /// <param name="_layers">List of layers.</param>
        /// <param name="_name">The name of the stream.</param>
        public void SendDataUpdate(List<dynamic> _objects, List<SpeckleLayer> _layers, string _name)
        {
            Objects = _objects;
            Layers = _layers;
            Name = _name;

            // in case there is a metadata update in progress
            MetadataSender.Stop();

            // it's time to... hit the button!
            DataSender.Start();
        }

        /// <summary>
        /// Sends a metadata update (just layers and stream name).
        /// </summary>
        /// <param name="layers">List of layers.</param>
        /// <param name="_name">The name of the stream.</param>
        public void SendMetadataUpdate(List<SpeckleLayer> layers, string _name)
        {
            Layers = layers;
            Name = _name;

            MetadataSender.Start();
        }

        /// <summary>
        /// Saves instance to the stream history.
        /// </summary>
        /// <param name="name">A specific name to save it by.</param>
        public void SaveToHistory(string name = "History")
        {
            Server.CreateNewStreamHistory((success, data) =>
            {
                if (!success)
                {
                    OnError?.Invoke(this, new SpeckleEventArgs("Failed to create a new history instance."));
                    return;
                }

            });
        }

        /// <summary>
        /// Returns the id to which this sender sends data.
        /// </summary>
        /// <returns></returns>
        public string GetStreamId()
        {
            return Server.StreamId;
        }

        public string GetServer()
        {
            return Server.RestEndpoint;
        }

        #endregion;

        #region volatile messaging
        /// <summary>
        /// Sends a volatile message that will be broadcast to this stream's clients.
        /// </summary>
        /// <param name="message">Message to broadcast.</param>
        public void BroadcastVolatileMessage(string message)
        {
            Ws.Send(JsonConvert.SerializeObject(new { eventName = "volatile-broadcast", args = message }));
        }

        public void SendVolatileMessage(string message, string socketId)
        {
            Ws.Send(JsonConvert.SerializeObject(new { eventName = "volatile-message", args = message }));
        }
        #endregion

        public override string ToString()
        {
            dynamic description = new
            {
                restEndpoint = Server.RestEndpoint,
                wsEndpoint = Server.WsEndpoint,
                streamId = Server.StreamId,
                token = Server.Token
            };

            return JsonConvert.SerializeObject(description);
        }

        /// <summary>
        /// Call this method whenever you are done with this component (document is closed, etc.)
        /// </summary>
        public void Dispose(bool delete = false)
        {
            if (delete)
            {
                //server.apiCall(@"/api/stream", Method.DELETE, etc, etc) 
            }

            if (Ws != null) Ws.Close();
            if (DataSender != null) DataSender.Dispose();
            if (MetadataSender != null) MetadataSender.Dispose();
            if (WsReconnecter != null) WsReconnecter.Dispose();
            if (IsReadyCheck != null) IsReadyCheck.Dispose();
        }
    }
}
