using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeckleCommon
{
    public class SpeckleObject
    {
        public string type;

        public void setType(string _type)
        {
            type = _type;
        }
    }

    public class SpeckleByteArray : SpeckleObject
    {
        public byte[] value;

        public SpeckleByteArray(byte[] arr)
        {
            type = "Bytearray";
            value = arr;
        }
    }

    public class SpeckleBoolean : SpeckleObject
    {
        public bool value;

        public SpeckleBoolean(bool val)
        {
            type = "Boolean";
            value = val;
        }
    }

    public class SpeckleNumber : SpeckleObject
    {
        public double value;

        public SpeckleNumber(double val)
        {
            type = "Number";
            value = val;
        }
    }

    public class SpeckleString : SpeckleObject
    {
        public string value;

        public SpeckleString(string val)
        {
            type = "String";
            value = val;
        }
    }

    public class SpeckleInterval : SpeckleObject
    {
        public double start, end;

        public SpeckleInterval(double start, double end)
        {
            type = "Interval";
            this.start = start; this.end = end;
        }
    }

    public class SpeckleInterval2d : SpeckleObject
    {
        public SpeckleInterval u, v;

        public SpeckleInterval2d(SpeckleInterval u, SpeckleInterval v)
        {
            type = "Interval2d";
            this.u = u; this.v = v;
        }
    }

    public class SpecklePoint : SpeckleObject
    {
        public double[] value = new double[3];
        public SpecklePoint(double x, double y, double z)
        {
            type = "Point";
            value[0] = x; value[1] = y; value[2] = z;
        }
    }


    public class SpeckleVector : SpeckleObject
    {
        public double[] value = new double[3];

        public SpeckleVector(double x, double y, double z)
        {
            type = "Vector";
            value[0] = x; value[1] = y; value[2] = z;
        }

    }

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

    public class SpecklePolyline : SpeckleObject
    {
        public double[] value;
        public string hash; 

        public SpecklePolyline(double[] coords)
        {
            type = "Polyline";
            value = coords;
        }
    }

    public class SpeckleMesh : SpeckleObject
    {
        public dynamic value;
        public string hash;

        public SpeckleMesh(double[] vertices, double[] faces, int[] colors)
        {
            type = "Mesh";
            value = new ExpandoObject();
            value.vertices = vertices;
            value.faces = faces;
            value.colors = colors;
        }
    }

    public class SpeckleBrep : SpeckleObject
    {

    }

}
