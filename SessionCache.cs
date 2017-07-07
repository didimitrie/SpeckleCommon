using System.Collections.Generic;
using System.Linq;

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
        HashSet<string> CacheHashes;
        HashSet<string> StageHashes;
        Dictionary<string, object> CacheObjects;
        Dictionary<string, object> StagedObjects;

        public SessionCache()
        {
            CacheHashes = new HashSet<string>();
            StageHashes = new HashSet<string>();
            CacheObjects = new Dictionary<string, object>();
            StagedObjects = new Dictionary<string, object>();
        }

        public bool IsInCache(string hash)
        {
            return CacheHashes.Contains(hash);
        }

        public bool GetFromCache(string hash, ref object obj)
        {
            if (CacheHashes.Contains(hash))
            {
                obj = CacheObjects[hash];
                return true;
            }
            return false;
        }

        public void AddToStage(string hash, object o)
        {
            if (StageHashes.Add(hash))
                StagedObjects.Add(hash, o);
        }

        public void AddToCache(string hash, object o)
        {
            if (CacheHashes.Add(hash))
                CacheObjects.Add(hash, o);
        }

        public void CommitStage()
        {
            CacheHashes.UnionWith(StageHashes);
            StageHashes = new HashSet<string>();

            CacheObjects.Union(StagedObjects);
            StagedObjects = new Dictionary<string, object>();
        }
    }
}
