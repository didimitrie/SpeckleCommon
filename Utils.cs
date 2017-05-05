using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeckleCommon
{
    /// <summary>
    /// Defines a speckle layer.
    /// </summary>
    [Serializable]
    public class SpeckleLayer
    {
        public string name, guid, topology;
        /// <summary>
        /// How many objects does this layer hold.
        /// </summary>
        public int objectCount;

        /// <summary>
        /// The list index of the first object this layer contains. 
        /// </summary>
        public int startIndex;

        /// <summary>
        /// Keeps track of the position of this layer in the layer stack.
        /// </summary>
        public int orderIndex;

        /// <summary>
        /// Extra stuff one can set around.
        /// </summary>
        public dynamic properties;

        /// <summary>
        /// Creates a new Speckle Layer.
        /// <para>Note: A Speckle Stream has one (flat) list of objects and one (flat) list of layers. In order to understand which object belongs to which layer, every layer must contain (a) the index of the first object and (b) the number of objects it contains. </para>
        /// <para>For example: let objects = [A, B, C, D, E, F]. let myLayer = { startIndex: 2, objectCount: 3} => objects C, D, E belong to this layer. </para>
        /// </summary>
        /// <param name="_name">Layer name</param>
        /// <param name="_guid">Layer guid. Must be unique, otherwise dragons.</param>
        /// <param name="_topology">If coming from grasshopper, get the topology description here.</param>
        /// <param name="_objectCount">Number of objects this layer contains.</param>
        /// <param name="_startIndex">The index of the first object this layer contains from the global object list.</param>
        /// <param name="_properties">Extra properties, if you want to. Would be cool to have stuff like colours and materials in here.</param>
        public SpeckleLayer(string _name, string _guid, string _topology, int _objectCount, int _startIndex, int _orderIndex, dynamic _properties = null)
        {
            name = _name; guid = _guid; topology = _topology; objectCount = _objectCount; startIndex = _startIndex; orderIndex = _orderIndex;
            properties = _properties;
        }

        /// <summary>
        /// Diffs between two lists of layers.
        /// </summary>
        /// <param name="oldLayers"></param>
        /// <param name="newLayers"></param>
        /// <returns>A dynamic object containing the following lists: toRemove, toAdd and toUpdate. </returns>
        public static dynamic diffLayers(List<SpeckleLayer> oldLayers, List<SpeckleLayer> newLayers)
        {
            dynamic returnValue = new ExpandoObject();
            returnValue.toRemove = oldLayers.Except(newLayers, new SpeckleLayerComparer()).ToList();
            returnValue.toAdd = newLayers.Except(oldLayers, new SpeckleLayerComparer()).ToList();
            returnValue.toUpdate = newLayers.Intersect(oldLayers, new SpeckleLayerComparer()).ToList();

            return returnValue;
        }

        /// <summary>
        /// Converts a list of expando objects to speckle layers [tries to].
        /// </summary>
        /// <param name="o">List to convert.</param>
        /// <returns></returns>
        public static List<SpeckleLayer> fromExpandoList(IEnumerable<dynamic> o)
        {
            List<SpeckleLayer> list = new List<SpeckleLayer>();
            foreach (var oo in o) list.Add(SpeckleLayer.fromExpando(oo));
            return list;
        }

        /// <summary>
        /// Converts one expando object to a speckle layer [tries to].
        /// </summary>
        /// <param name="o">ExpandoObject to covnert.</param>
        /// <returns></returns>
        public static SpeckleLayer fromExpando(dynamic o)
        {
            return new SpeckleLayer((string)o.name, (string)o.guid, (string)o.topology, (int)o.objectCount, (int)o.startIndex, (int)o.orderIndex, (dynamic)o.properties);
        }

    }

    /// <summary>
    /// Used for diffing between layers w/h linq.
    /// </summary>
    internal class SpeckleLayerComparer : IEqualityComparer<SpeckleLayer>
    {
        public bool Equals(SpeckleLayer x, SpeckleLayer y)
        {
            return x.guid == y.guid;
        }

        public int GetHashCode(SpeckleLayer obj)
        {
            return obj.guid.GetHashCode();
        }
    }

    /// <summary>
    /// (string) EventInfo (random metadata); (dynamic) Data (the actual event data, if any).
    /// </summary>
    public class SpeckleEventArgs : EventArgs
    {
        public string EventInfo;
        public dynamic Data;
        /// <summary>
        /// Creates a new speckle event args.
        /// </summary>
        /// <param name="text">Event Info</param>
        /// <param name="_Data">ExpandoObject with event data.</param>
        public SpeckleEventArgs(string text, dynamic _Data = null)
        {
            EventInfo = text; Data = _Data;
        }
    }

    public delegate void SpeckleEvents(object source, SpeckleEventArgs e);


    /// <summary>
    /// Class that flexibily defines what a SpeckleObject is.
    /// </summary>
    //[Serializable]
    //public class SpeckleObject
    //{
    //    public string type;
    //    public string hash;
    //    public dynamic value; // expandoobject
    //    public dynamic properties; // expandoobject
    //    public string encodedValue;
    //}

    [Serializable]
    public class SpeckleObjectProperties
    {
        public int objectIndex;
        public object properties;

        public SpeckleObjectProperties(int _objectIndex, object _properties)
        {
            objectIndex = _objectIndex;
            properties = _properties;
        }
    }

    /// <summary>
    /// In progress. Not used yet.
    /// </summary>
    [Serializable]
    public class SpeckleClientDocument
    {
        public string documentGuid { get; set; }
        public string documentName { get; set; }
        /// <summary>
        /// RH, GH, DYNAMO, etc.
        /// </summary>
        public string documentType { get; set; }
    }
}
