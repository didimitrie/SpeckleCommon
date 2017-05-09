﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeckleCommon
{
    /// <summary>
    /// Standardises object conversion. The SpeckleApiProxy requires a converter object which should derive from this interface.
    /// </summary>
    [Serializable]
    public abstract class SpeckleConverter
    {

        public string[] hashedTypes = new string[] { "Polyline", "Curve", "Mesh", "Brep" };
        public string[] encodedTypes = new string[] { "Curve", "Brep" };

        public SpeckleConverter()
        {
        }

        #region global convert functions

        /// <summary>
        /// Converts a list of objects. 
        /// </summary>
        /// <param name="objects">Objects to convert.</param>
        /// <param name="getEncoded">If true, should return a base64 encoded version of the object as well.</param>
        /// <param name="getAbstract">If set to false will not return speckle-parsable values for objects.</param>
        /// <returns>A list of dynamic objects.</returns>
        abstract public List<SpeckleObject> convert(IEnumerable<object> objects);

        /// <summary>
        /// Async conversion of a list of objects. Returns the result in a callback.
        /// </summary>
        /// <param name="objects">Objects to convert.</param>
        /// <param name="callback">Action to perform with the converted objects.</param>
        abstract public void convertAsync(IEnumerable<object> objects, Action<List<SpeckleObject>> callback);


        abstract public List<SpeckleObjectProperties> getObjectProperties(IEnumerable<object> objects);

        #endregion

        /// <summary>
        /// Encodes an object back to its native type.
        /// </summary>
        /// <param name="myObj">object to encode to native.</param>
        /// <returns></returns>
        abstract public object encodeObject(dynamic myObj, dynamic objectProperties = null);

        #region standard types: boolean, numbers, strings

        public static SpeckleObject fromBoolean(bool b)
        {
            return new SpeckleBoolean(b);
        }

        public static bool toBoolean(dynamic b)
        {
            return b.value;
        }

        public static SpeckleObject fromNumber(double num)
        {
            return new SpeckleNumber(num);
        }

        public static double toNumber(dynamic num)
        {
            return (double)num.value;
        }

        public static SpeckleObject fromString(string str)
        {
            return new SpeckleString(str);
        }

        public static string toString(dynamic str)
        {
            return (string)str.value;
        }

        #endregion

        #region utils

        /// <summary>
        /// Encodes a serialisable object in base64 string.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string getBase64(object obj)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Serialize(ms, obj);
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        /// <summary>
        /// Encodes a serialisable object in a byte array.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] getBytes(object obj)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public static string getHash(object obj)
        {
            byte[] b = getBytes(obj);
            return getHash(b);
        }

        public static string getHash(byte[] b)
        {
            byte[] hash;
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                hash = md5.ComputeHash(b);
                StringBuilder sb = new StringBuilder();
                foreach (byte bbb in hash)
                    sb.Append(bbb.ToString("X2"));

                return sb.ToString().ToLower();
            }
        }

        /// <summary>
        /// Hashes a given string using md5. Used to get object hashes. 
        /// </summary>
        /// <param name="str">What to hash.</param>
        /// <returns>a lowercase string of the md5 hash.</returns>
        public static string getHash(string str)
        {
            byte[] hash;
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                hash = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hash)
                    sb.Append(b.ToString("X2"));

                return sb.ToString().ToLower();
            }
        }

        public static object getObjFromString(string base64String)
        {
            if (base64String == null) return null;
            byte[] bytes = Convert.FromBase64String(base64String);
            return getObjFromBytes(bytes);
        }

        public static object getObjFromBytes( byte[] bytes )
        {
            using (MemoryStream ms = new MemoryStream(bytes, 0, bytes.Length))
            {
                ms.Write(bytes, 0, bytes.Length);
                ms.Position = 0;
                return new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Deserialize(ms);
            }
        }

        #endregion

        /// <summary>
        /// Spits out a string descriptor, ie "grasshopper-converter", or "rhino-converter". Used for reiniatisation of the API Proxy.
        /// </summary>
        /// <returns></returns>
        abstract public dynamic description();
    }
}
