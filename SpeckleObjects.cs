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
        public string type;
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

    public class SpeckleString: SpeckleObject
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
        public double[] value = new double[2];
        public SpeckleInterval(double start, double end)
        {
            type = "Interval";
            value[0] = start; value[1] = end;
        }
    }

    public class SpeckleInterval2d : SpeckleObject
    {
        public double[] value = new double[4];
        public SpeckleInterval2d(double s1, double e1, double s2, double e2)
        {
            type = "Interval2d";
            value[0] = s1; value[1] = e1;
            value[2] = s2; value[3] = e2;
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
        dynamic value;

        public SpecklePlane( SpecklePoint origin, SpeckleVector normal, SpeckleVector xdir, SpeckleVector ydir )
        {
            type = "Plane";
            value = new ExpandoObject();
            value.origin = origin.value;
            value.normal = normal.value;
            value.xdir = xdir.value;
            value.ydir = ydir.value;
        }
    }
}
