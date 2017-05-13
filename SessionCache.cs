using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeckleCommon
{
    // Something that keeps track of sent/received objects during 
    // the current session.

    //Behvaiour for receivers: 
    // if object is in cache, just get it from there, do not request 
    // from the server

    // Behaviour for senders: 
    // if object is in cache, do not send the full one, just send 
    // its metadata (type and hash)
    public class SessionCache
    {
        HashSet<string> cacheHashes;
        HashSet<string> stageHashes;
        Dictionary<string, object> cacheObjects;
        Dictionary<string, object> stagedObjects;

        public SessionCache()
        {
            cacheHashes = new HashSet<string>();
            stageHashes = new HashSet<string>();
            cacheObjects = new Dictionary<string, object>();
            stagedObjects = new Dictionary<string, object>();
        }

        public bool isInCache(string hash)
        {
            return cacheHashes.Contains(hash);
        }

        public bool getFromCache(string hash, ref object obj)
        {
            if (cacheHashes.Contains(hash))
            {
                obj = cacheObjects[hash];
                return true;
            }
            return false;
        }

        public void addToStage(string hash, object o)
        {
            if (stageHashes.Add(hash))
                stagedObjects.Add(hash, o);
        }

        public void addToCache(string hash, object o)
        {
            if (cacheHashes.Add(hash))
                cacheObjects.Add(hash, o);
        }

        public void commitStage()
        {
            cacheHashes.UnionWith(stageHashes);
            stageHashes = new HashSet<string>();

            cacheObjects.Union(stagedObjects);
            stagedObjects = new Dictionary<string, object>();
        }
    }
}
