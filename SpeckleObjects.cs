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
        public string hash { get; set; }

        public SpeckleObject() { }
        public SpeckleObject(string type, string hash)
        {
            this.type = type; this.hash = hash;
        }
    }

    [Serializable]
    public class SpeckleBoolean : SpeckleObject
    {
        public bool value { get; set; }

        public SpeckleBoolean() { }

        public SpeckleBoolean(bool val)
        {
            type = "Boolean";
            value = val;
        }
    }

    [Serializable]
    public class SpeckleNumber : SpeckleObject
    {
        public double value { get; set; }

        public SpeckleNumber() { }

        public SpeckleNumber(double val)
        {
            type = "Number";
            value = val;
        }
    }

    [Serializable]
    public class SpeckleString : SpeckleObject
    {
        public string value { get; set; }

        public SpeckleString() { }

        public SpeckleString(string val)
        {
            type = "String";
            value = val;
        }
    }

    [Serializable]
    public class SpeckleInterval : SpeckleObject
    {
        public double start { get; set; }
        public double end { get; set; }

        public SpeckleInterval() { }

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

        public SpeckleInterval2d() { }

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

        public SpecklePoint() { }

        public SpecklePoint(double x, double y, double z)
        {
            type = "Point";
            value[0] = x; value[1] = y; value[2] = z;
            hash = "Point." + SpeckleConverter.getHash(x + "" + y + "" + z);
        }
    }

    [Serializable]
    public class SpeckleVector : SpeckleObject
    {
        public double[] value = new double[3];

        public SpeckleVector() { }

        public SpeckleVector(double x, double y, double z)
        {
            type = "Vector";
            value[0] = x; value[1] = y; value[2] = z;
            hash = "Vector."  + SpeckleConverter.getHash( x + "" + y + "" + z);
        }

    }

    [Serializable]
    public class SpecklePlane : SpeckleObject
    {
        public SpecklePoint origin;
        public SpeckleVector normal, xdir, ydir;

        public SpecklePlane() { }

        public SpecklePlane(SpecklePoint origin, SpeckleVector normal, SpeckleVector xdir, SpeckleVector ydir)
        {
            type = "Plane";
            this.origin = origin;
            this.normal = normal;
            this.xdir = xdir;
            this.ydir = ydir;
            hash = "Plane." + SpeckleConverter.getHash(origin.hash + normal.hash + xdir.hash + ydir.hash);
        }
    }

    [Serializable]
    public class SpeckleLine : SpeckleObject
    {
        public SpecklePoint start { get; set; }
        public SpecklePoint end { get; set; }

        public SpeckleLine() {  }

        public SpeckleLine(SpecklePoint start, SpecklePoint end)
        {
            type = "Line";
            this.start = start;
            this.end = end;
            hash = "Line." + SpeckleConverter.getHash(start.hash + end.hash);
        }
    }

    [Serializable]
    public class SpeckleRectangle : SpeckleObject
    {
        public SpecklePoint a { get; set; }
        public SpecklePoint b { get; set; }
        public SpecklePoint c { get; set; }
        public SpecklePoint d { get; set; }

        public SpeckleRectangle() { }

        public SpeckleRectangle(SpecklePoint a, SpecklePoint b, SpecklePoint c, SpecklePoint d)
        {
            type = "Rectangle";
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
            hash = "Rectangle." + SpeckleConverter.getHash(a.hash + b.hash + c.hash + d.hash);
        }
    }

    [Serializable]
    public class SpeckleCircle : SpeckleObject
    {
        public double radius { get; set; }
        public SpecklePoint center { get; set; }
        public SpeckleVector normal { get; set; }

        public SpeckleCircle() { }
        public SpeckleCircle(SpecklePoint center, SpeckleVector normal, double radius)
        {
            type = "Circle";
            this.center = center;
            this.normal = normal;
            this.radius = radius;
            hash = "Circle." + SpeckleConverter.getHash(radius + center.hash + normal.hash);
        }
    }

    [Serializable]
    public class SpeckleBox : SpeckleObject
    {
        public SpecklePlane basePlane;
        public SpeckleInterval xSize, ySize, zSize;

        public SpeckleBox() { }
        public SpeckleBox(SpecklePlane basePlane, SpeckleInterval xSize, SpeckleInterval ySize, SpeckleInterval zSize)
        {
            type = "Box";
            this.basePlane = basePlane;
            this.xSize = xSize;
            this.ySize = ySize;
            this.zSize = zSize;
            hash = "Box." + SpeckleConverter.getHash(basePlane.hash + xSize.start + xSize.end + ySize.start + ySize.end);
        }
    }

    [Serializable]
    public class SpecklePolyline : SpeckleObject
    {
        public double[] value { get; set; }

        public SpecklePolyline() { }

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
        public SpecklePolyline displayValue { get; set; }
        public string base64 { get; set; }
        public string provenance { get; set; }

        public SpeckleCurve() { }

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
        public double[] vertices { get; set; }
        public int[] faces { get; set; }
        public int[] colors { get; set; }

        public SpeckleMesh() { }

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
        public SpeckleMesh displayValue { get; set; }
        public string base64 { get; set; }
        public string provenance { get; set; }

        public SpeckleBrep() { }

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
