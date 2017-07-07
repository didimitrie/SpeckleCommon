using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using WebSocketSharp;

namespace SpeckleCommon
{
    public class SpeckleReceiver
    {
        // connectivity:
        SpeckleServer Server;
        WebSocket Ws;

        // converer:
        SpeckleConverter Converter;

        // cache:
        SessionCache Cache;

        //timers:
        int ReconnDebounceInterval = 1000;

        System.Timers.Timer isReadyCheck;
        System.Timers.Timer wsReconnecter;

        //first call stream
        dynamic StreamLiveInstance;
        bool StreamFound = false;

        /// <summary>
        /// Event emitted when everything is ready.
        /// </summary>
        public event SpeckleEvents OnReady;
        /// <summary>
        /// Event emitted when a volatile message was received.
        /// </summary>
        public event SpeckleEvents OnMessage;
        public event SpeckleEvents OnBroadcast; // do we need the separation? maybe yeah

        public event SpeckleEvents OnDataMessage;

        public event SpeckleEvents OnData;
        public event SpeckleEvents OnMetadata;
        public event SpeckleEvents OnHistory;
        public event SpeckleEvents OnError;

        #region constructors

        public SpeckleReceiver(string apiUrl, string token, string streamId, SpeckleConverter _converter, string documentId = null, string documentName = null)
        {
            Converter = _converter;

            Cache = new SessionCache();

            Server = new SpeckleServer(apiUrl, token, streamId);
            Server.OnError += (sender, e) =>
            {
                OnError?.Invoke(this, new SpeckleEventArgs(e.EventInfo));
            };

            Server.OnReady += (sender, e) =>
            {
                Setup();
            };
        }

        public SpeckleReceiver(string serialisedObject, SpeckleConverter _converter)
        {
            dynamic description = JsonConvert.DeserializeObject(serialisedObject);
            Server = new SpeckleServer((string)description.restEndpoint, (string)description.token, (string)description.streamId);

            Converter = _converter;

            Cache = new SessionCache();

            Server.OnError += (sender, e) =>
            {
                OnError?.Invoke(this, new SpeckleEventArgs(e.EventInfo));
            };

            Server.OnReady += (sender, e) =>
            {
                Setup();
            };
        }

        #endregion

        #region main setup calls

        private void Setup()
        {
            Server.GetStream((success, response) =>
            {
                if (!success)
                {
                    OnError?.Invoke(this, new SpeckleEventArgs("Failed to retrieve stream."));
                    return;
                }
                StreamFound = true;
                StreamLiveInstance = response; // this will fail!!!
                SetupWebsocket();
            });

            // ready is defined as: streamId exists && wsSessionId && streamWas found.
            isReadyCheck = new System.Timers.Timer(150);
            isReadyCheck.AutoReset = false; isReadyCheck.Enabled = true;
            isReadyCheck.Elapsed += (sender, e) =>
            {
                if (Server.StreamId == null) { isReadyCheck.Start(); return; }
                if (Server.WsSessionId == null) { isReadyCheck.Start(); return; }
                if (!StreamFound) { isReadyCheck.Start(); return; }

                GetObjects(StreamLiveInstance as ExpandoObject, (castObjects) =>
                {
                    dynamic eventData = new ExpandoObject();
                    eventData.objects = castObjects;
                    eventData.layers = SpeckleLayer.FromExpandoList(StreamLiveInstance.layers);
                    eventData.layerMaterials = SpeckleLayer.FromExpandoList(StreamLiveInstance.layerMaterials);
                    eventData.name = StreamLiveInstance.name;

                    OnReady?.Invoke(this, new SpeckleEventArgs("receiver ready", eventData));
                });
            };

        }

        private void SetupWebsocket()
        {
            wsReconnecter = new System.Timers.Timer(ReconnDebounceInterval);
            wsReconnecter.AutoReset = false; wsReconnecter.Enabled = false;
            wsReconnecter.Elapsed += (sender, e) =>
            {
                Ws.Connect();
            };

            Ws = new WebSocket(Server.WsEndpoint + "?access_token=" + Server.Token);

            Ws.OnOpen += (sender, e) =>
            {
                wsReconnecter.Stop();
                Ws.Send(JsonConvert.SerializeObject(new { eventName = "join-stream", args = new { streamid = Server.StreamId, role = "receiver" } }));
            };

            Ws.OnClose += (sender, e) =>
            {
                wsReconnecter.Start();
                Server.WsSessionId = null;
            };

            Ws.OnMessage += (sender, e) =>
            {
                if (e.Data == "ping")
                {
                    Ws.Send("alive");
                    return;
                }

                dynamic message = JsonConvert.DeserializeObject<ExpandoObject>(e.Data);

                if (message.eventName == "ws-session-id")
                {
                    Server.WsSessionId = message.sessionId;
                    return;
                }

                if (message.eventName == "volatile-broadcast")
                {
                    OnBroadcast?.Invoke(this, new SpeckleEventArgs("volatile-broadcast", message));
                    return;
                }

                if (message.eventName == "volatile-message")
                {
                    OnMessage?.Invoke(this, new SpeckleEventArgs("volatile-message", message));
                    return;
                }

                if (message.eventName == "live-update")
                {
                    OnDataMessage?.Invoke(this, new SpeckleEventArgs("Received update notification."));
                    GetObjects(message.args as ExpandoObject, (castObjects) =>
                    {
                        dynamic eventData = new ExpandoObject();
                        eventData.objects = castObjects;
                        eventData.layers = SpeckleLayer.FromExpandoList(message.args.layers);
                        eventData.name = message.args.name;

                        OnData?.Invoke(this, new SpeckleEventArgs("live-update", eventData));
                    });
                    return;
                }

                if (message.eventName == "metadata-update")
                {
                    dynamic eventData = new ExpandoObject();
                    eventData.name = message.args.name;
                    eventData.layers = SpeckleLayer.FromExpandoList(message.args.layers);
                    OnMetadata?.Invoke(this, new SpeckleEventArgs("metadata-update", eventData));
                    return;
                }

                if (message.eventName == "history-update")
                {
                    // TODO
                    OnHistory?.Invoke(this, new SpeckleEventArgs("history-update"));
                    return;
                }
            };

            Ws.Connect();
        }

        #endregion

        #region object getters

        public void GetObjects(dynamic liveUpdate, Action<List<object>> callback)
        {
            // if stream contains no objects:
            if (liveUpdate.objects.Count == 0)
            {
                callback(new List<object>());
                return;
            }

            object[] castObjects = new object[(int)liveUpdate.objects.Count];


            int k = 0; // current list head
            int insertionCount = 0; // cast objects head
            foreach (dynamic obj in liveUpdate.objects)
            {
                // check if we have a user prop
                dynamic prop = null;
                foreach (var myprop in liveUpdate.objectProperties)
                {
                    if (myprop.objectIndex == k)
                        prop = myprop;
                }

                // TODO: Async doesn't guarantee object order.
                // need to switch toa insertAt(k, obj) list, and pass that through politely to the guy below;
                GetObject(obj as ExpandoObject, prop as ExpandoObject, k, (encodedObject, index) =>
                {
                    castObjects[index] = encodedObject;
                    if (++insertionCount == (int)liveUpdate.objects.Count)
                        callback(castObjects.ToList());
                });

                k++;
            }
        }

        public void GetObject(dynamic obj, dynamic objectProperties, int index, Action<object, int> callback)
        {
            if (!SpeckleConverter.HeavyTypes.Contains((string)obj.type))
            {
                callback(Converter.EncodeObject(obj, objectProperties), index);
                return;
            }


            if (Cache.IsInCache((string)obj.hash))
            {
                object cachedObj = null;
                Cache.GetFromCache((string)obj.hash, ref cachedObj);
                callback(cachedObj, index);
                return;
            }

            Server.GetGeometry((string)obj.hash, SpeckleConverter.EncodedTypes.Contains((string)obj.type) ? "native":"", (success, response) =>
            {
                if(!success)
                {
                    callback("Failed to retrieve object: " + (string) obj.hash, index);
                    return;
                }

                var castObject = Converter.EncodeObject(response.data, objectProperties);

                Cache.AddToCache((string)obj.hash, castObject);

                callback(castObject, index);
            });
        }

        public string GetStreamId()
        {
            return Server.StreamId;
        }

        public string GetServer()
        {
            return Server.RestEndpoint;
        }

        public string GetToken()
        {
            return Server.Token;
        }

        #endregion

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
            // look ma, json! XD c#
            dynamic description = new
            {
                restEndpoint = Server.RestEndpoint,
                wsEndpoint = Server.WsEndpoint,
                streamId = Server.StreamId,
                token = Server.Token
            };

            return JsonConvert.SerializeObject(description);
        }

        public void Dispose(bool delete = false)
        {
            if (delete)
            {
                //server.apiCall(@"/api/stream", Method.DELETE, etc, etc 
            }

            if (Ws != null) Ws.Close();
            if (wsReconnecter != null) wsReconnecter.Dispose();
            if (isReadyCheck != null) isReadyCheck.Dispose();
        }
    }
}
