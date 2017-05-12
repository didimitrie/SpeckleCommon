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
        HashSet<string> cache_hashes;
        Dictionary<string, object> cache_objects;

        public SessionCache()
        {

        }
    }
}
