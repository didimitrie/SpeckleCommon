using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeckleCommon
{
    [Serializable]
    public class SpeckleObject
    {
        public string type { get; set; }

        public void setType(string _type)
        {
            type = _type;
        }
    }

    [Serializable]
    public class SpeckleByteArray : SpeckleObject
    {
        public byte[] value;

        public SpeckleByteArray(byte[] arr)
        {
            type = "Bytearray";
            value = arr;
        }
    }

    [Serializable]
    public class SpeckleBoolean : SpeckleObject
    {
        public bool value;

        public SpeckleBoolean(bool val)
        {
            type = "Boolean";
            value = val;
        }
    }

    [Serializable]
    public class SpeckleNumber : SpeckleObject
    {
        public double value;

        public SpeckleNumber(double val)
        {
            type = "Number";
            value = val;
        }
    }

    [Serializable]
    public class SpeckleString : SpeckleObject
    {
        public string value;

        public SpeckleString(string val)
        {
            type = "String";
            value = val;
        }
    }

    [Serializable]
    public class SpeckleInterval : SpeckleObject
    {
        public double start, end;

        public SpeckleInterval(double start, double end)
        {
            type = "Interval";
            this.start = start; this.end = end;
        }
    }

    [Serializable]
    public class SpeckleInterval2d : SpeckleObject
    {
        public SpeckleInterval u, v;

        public SpeckleInterval2d(SpeckleInterval u, SpeckleInterval v)
        {
            type = "Interval2d";
            this.u = u; this.v = v;
        }
    }

    [Serializable]
    public class SpecklePoint : SpeckleObject
    {
        public double[] value = new double[3];
        public SpecklePoint(double x, double y, double z)
        {
            type = "Point";
            value[0] = x; value[1] = y; value[2] = z;
        }
    }

    [Serializable]
    public class SpeckleVector : SpeckleObject
    {
        public double[] value = new double[3];

        public SpeckleVector(double x, double y, double z)
        {
            type = "Vector";
            value[0] = x; value[1] = y; value[2] = z;
        }

    }

    [Serializable]
    public class SpecklePlane : SpeckleObject
    {
        public SpecklePoint origin;
        public SpeckleVector normal, xdir, ydir;

        public SpecklePlane(SpecklePoint origin, SpeckleVector normal, SpeckleVector xdir, SpeckleVector ydir)
        {
            type = "Plane";
            this.origin = origin;
            this.normal = normal;
            this.xdir = xdir;
            this.ydir = ydir;
        }
    }

    [Serializable]
    public class SpeckleLine : SpeckleObject
    {
        public SpecklePoint start, end;
        public SpeckleLine(SpecklePoint start, SpecklePoint end)
        {
            type = "Line";
            this.start = start;
            this.end = end;
        }
    }

    [Serializable]
    public class SpeckleRectangle : SpeckleObject
    {
        public SpecklePoint a, b, c, d;

        public SpeckleRectangle(SpecklePoint a, SpecklePoint b, SpecklePoint c, SpecklePoint d)
        {
            type = "Rectangle";
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
        }
    }

    [Serializable]
    public class SpecklePolyline : SpeckleObject
    {
        public double[] value;
        public string hash; 

        public SpecklePolyline(double[] coords)
        {
            type = "Polyline";
            value = coords;
            hash = type + "." + SpeckleConverter.getHash(coords);
        }
    }


    [Serializable]
    public class SpeckleCurve: SpeckleObject
    {
        public string hash;
        public SpecklePolyline displayValue;
        public string base64;
        public string provenance;

        public SpeckleCurve(SpecklePolyline polylineEquivalent, string base64, string provenance)
        {
            type = "Curve";
            this.displayValue = polylineEquivalent;
            this.base64 = base64;
            this.provenance = provenance;
            this.hash = type + "." + SpeckleConverter.getHash(polylineEquivalent.value);
        }
    }

    [Serializable]
    public class SpeckleMesh : SpeckleObject
    {
        public string hash { get; set;  }
        public double[] vertices { get; set; }
        public int[] faces { get; set; }
        public int[] colors { get; set; }

        public SpeckleMesh(double[] vertices, int[] faces, int[] colors)
        {
            type = "Mesh";
            this.vertices = vertices;
            this.faces = faces;
            this.colors = colors;
            hash = type + "." + SpeckleConverter.getHash(this);
        }
    }

    [Serializable]
    public class SpeckleBrep : SpeckleObject
    {
        public string hash { get; set; }
        public SpeckleMesh displayValue;
        public string base64;
        public string provenance;

        public SpeckleBrep(SpeckleMesh mesh, string base64, string provenance)
        {
            type = "Brep";
            this.displayValue = mesh;
            this.base64 = base64;
            this.provenance = provenance;
            this.hash = type + "." + SpeckleConverter.getHash(mesh);
        }
    }

}
