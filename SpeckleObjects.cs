using System;

namespace SpeckleCommon
{
    [Serializable]
    public class SpeckleObject
    {
        public string Type { get; set; }
        public string Hash { get; set; }

        public SpeckleObject() { }
        public SpeckleObject(string type, string hash)
        {
            Type = type; Hash = hash;
        }
    }

    [Serializable]
    public class SpeckleBoolean : SpeckleObject
    {
        public bool Value { get; set; }

        public SpeckleBoolean() { }

        public SpeckleBoolean(bool val)
        {
            Type = "Boolean";
            Value = val;
        }
    }

    [Serializable]
    public class SpeckleNumber : SpeckleObject
    {
        public double Value { get; set; }

        public SpeckleNumber() { }

        public SpeckleNumber(double val)
        {
            Type = "Number";
            Value = val;
        }
    }

    [Serializable]
    public class SpeckleString : SpeckleObject
    {
        public string Value { get; set; }

        public SpeckleString() { }

        public SpeckleString(string val)
        {
            Type = "String";
            Value = val;
        }
    }

    [Serializable]
    public class SpeckleInterval : SpeckleObject
    {
        public double Start { get; set; }
        public double End { get; set; }

        public SpeckleInterval() { }

        public SpeckleInterval(double start, double end)
        {
            Type = "Interval";
            Start = start; End = end;
        }
    }

    [Serializable]
    public class SpeckleInterval2d : SpeckleObject
    {
        public SpeckleInterval U, V;

        public SpeckleInterval2d() { }

        public SpeckleInterval2d(SpeckleInterval u, SpeckleInterval v)
        {
            Type = "Interval2d";
            U = u; V = v;
        }
    }

    [Serializable]
    public class SpecklePoint : SpeckleObject
    {
        public double[] Value = new double[3];

        public SpecklePoint() { }

        public SpecklePoint(double x, double y, double z)
        {
            Type = "Point";
            Value[0] = x; Value[1] = y; Value[2] = z;
            Hash = "Point." + SpeckleConverter.GetHash(x + "" + y + "" + z);
        }
    }

    [Serializable]
    public class SpeckleVector : SpeckleObject
    {
        public double[] Value = new double[3];

        public SpeckleVector() { }

        public SpeckleVector(double x, double y, double z)
        {
            Type = "Vector";
            Value[0] = x; Value[1] = y; Value[2] = z;
            Hash = "Vector."  + SpeckleConverter.GetHash( x + "" + y + "" + z);
        }

    }

    [Serializable]
    public class SpecklePlane : SpeckleObject
    {
        public SpecklePoint Origin;
        public SpeckleVector Normal, Xdir, Ydir;

        public SpecklePlane() { }

        public SpecklePlane(SpecklePoint origin, SpeckleVector normal, SpeckleVector xdir, SpeckleVector ydir)
        {
            Type = "Plane";
            Origin = origin;
            Normal = normal;
            Xdir = xdir;
            Ydir = ydir;
            Hash = "Plane." + SpeckleConverter.GetHash(origin.Hash + normal.Hash + xdir.Hash + ydir.Hash);
        }
    }

    [Serializable]
    public class SpeckleLine : SpeckleObject
    {
        public SpecklePoint Start { get; set; }
        public SpecklePoint End { get; set; }

        public SpeckleLine() {  }

        public SpeckleLine(SpecklePoint start, SpecklePoint end)
        {
            Type = "Line";
            Start = start;
            End = end;
            Hash = "Line." + SpeckleConverter.GetHash(start.Hash + end.Hash);
        }
    }

    [Serializable]
    public class SpeckleRectangle : SpeckleObject
    {
        public SpecklePoint A { get; set; }
        public SpecklePoint B { get; set; }
        public SpecklePoint C { get; set; }
        public SpecklePoint D { get; set; }

        public SpeckleRectangle() { }

        public SpeckleRectangle(SpecklePoint a, SpecklePoint b, SpecklePoint c, SpecklePoint d)
        {
            Type = "Rectangle";
            A = a;
            B = b;
            C = c;
            D = d;
            Hash = "Rectangle." + SpeckleConverter.GetHash(a.Hash + b.Hash + c.Hash + d.Hash);
        }
    }

    [Serializable]
    public class SpeckleCircle : SpeckleObject
    {
        public double Radius { get; set; }
        public SpecklePoint Center { get; set; }
        public SpeckleVector Normal { get; set; }

        public SpeckleCircle() { }
        public SpeckleCircle(SpecklePoint center, SpeckleVector normal, double radius)
        {
            Type = "Circle";
            Center = center;
            Normal = normal;
            Radius = radius;
            Hash = "Circle." + SpeckleConverter.GetHash(radius + center.Hash + normal.Hash);
        }
    }

    [Serializable]
    public class SpeckleBox : SpeckleObject
    {
        public SpecklePlane BasePlane;
        public SpeckleInterval XSize, YSize, ZSize;

        public SpeckleBox() { }
        public SpeckleBox(SpecklePlane basePlane, SpeckleInterval xSize, SpeckleInterval ySize, SpeckleInterval zSize)
        {
            Type = "Box";
            BasePlane = basePlane;
            XSize = xSize;
            YSize = ySize;
            ZSize = zSize;
            Hash = "Box." + SpeckleConverter.GetHash(basePlane.Hash + xSize.Start + xSize.End + ySize.Start + ySize.End);
        }
    }

    [Serializable]
    public class SpecklePolyline : SpeckleObject
    {
        public double[] Value { get; set; }

        public SpecklePolyline() { }

        public SpecklePolyline(double[] coords)
        {
            Type = "Polyline";
            Value = coords;
            Hash = Type + "." + SpeckleConverter.GetHash(coords);
        }
    }


    [Serializable]
    public class SpeckleCurve: SpeckleObject
    {
        public SpecklePolyline DisplayValue { get; set; }
        public string Base64 { get; set; }
        public string Provenance { get; set; }

        public SpeckleCurve() { }

        public SpeckleCurve(SpecklePolyline polylineEquivalent, string base64, string provenance)
        {
            Type = "Curve";
            DisplayValue = polylineEquivalent;
            Base64 = base64;
            Provenance = provenance;
            Hash = Type + "." + SpeckleConverter.GetHash(polylineEquivalent.Value);
        }
    }

    [Serializable]
    public class SpeckleMesh : SpeckleObject
    {
        public double[] Vertices { get; set; }
        public int[] Faces { get; set; }
        public int[] Colors { get; set; }

        public SpeckleMesh() { }

        public SpeckleMesh(double[] vertices, int[] faces, int[] colors)
        {
            Type = "Mesh";
            Vertices = vertices;
            Faces = faces;
            Colors = colors;
            Hash = Type + "." + SpeckleConverter.GetHash(this);
        }
    }

    [Serializable]
    public class SpeckleBrep : SpeckleObject
    {
        public SpeckleMesh DisplayValue { get; set; }
        public string Base64 { get; set; }
        public string Provenance { get; set; }

        public SpeckleBrep() { }

        public SpeckleBrep(SpeckleMesh mesh, string base64, string provenance)
        {
            Type = "Brep";
            DisplayValue = mesh;
            Base64 = base64;
            Provenance = provenance;
            Hash = Type + "." + SpeckleConverter.GetHash(mesh);
        }
    }

}
