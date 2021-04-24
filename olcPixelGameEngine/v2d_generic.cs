using System;
using System.Collections.Generic;
using System.Text;

namespace olc
{

    public class vd2d
    {
        protected double _x;
        protected double _y;

        public vd2d() => (_x, _y) = (0, 0);
        public vd2d(vd2d v) => (_x, _y) = (v._x, v.y);
        public vd2d(double x, double y) => (_x, _y) = (x, y);

        public double x { get => _x; }
        public double y { get => _y; }

        public void assign(vd2d vect) => (_x,_y) = (vect.x, vect.y);
        public double cross (vd2d rhs) => this.x * rhs.x - this.y * rhs.y;
        public double dot (vd2d rhs) => this.x * rhs.x + this.y * rhs.y;
        public double mag() => (Math.Sqrt(x * x + y * y));
        public double mag2() => x * x + y * y;

        public vd2d floor() => new vd2d(Math.Floor(x), Math.Floor(y));
        public vd2d ceil() => new vd2d(Math.Ceiling(x), Math.Ceiling(y));
        public vd2d max(vd2d v) => new vd2d(Math.Max(x, v.x), Math.Max(y, v.y));
        public vd2d min(vd2d v) => new vd2d(Math.Min(x, v.x), Math.Min(y, v.y));
        public vd2d perp() => new vd2d(-y, x);
        public vd2d norm() { double r = 1 / mag(); return new vd2d(x * r, y * r); }


        public static vd2d operator +(vd2d lhs, vd2d rhs) => new vd2d(lhs.x + rhs.x, lhs.y + rhs.y);
        public static vd2d operator -(vd2d lhs, vd2d rhs) => new vd2d(lhs.x - rhs.x, lhs.y - rhs.y);
        public static vd2d operator *(vd2d lhs, double rhs) => new vd2d(lhs.x* rhs, lhs.y* rhs);
        public static vd2d operator *(double lhs, vd2d rhs) => new vd2d(rhs.x * lhs, rhs.y * lhs);
        public static vd2d operator *(vd2d lhs, vd2d rhs) => new vd2d(lhs.x * rhs.x, lhs.y * rhs.y);
        public static vd2d operator /(vd2d lhs, double rhs) => new vd2d(lhs.x / rhs, lhs.y / rhs);
        public static vd2d operator /(double lhs, vd2d rhs) => new vd2d(rhs.x / lhs, rhs.y / lhs);
    }

    public class vi2d : vd2d
    {
        public vi2d() => (_x, _y) = (0, 0);
        public vi2d(vi2d v) => (_x, _y) = (v.x, v.y);
        public vi2d(double x, double y) => (_x, _y) = (x, y);

        public int x { get { return (int)_x; } set { _x = value; } }
        public int y { get { return (int)_y; } set { _y = value; } }

        public static vi2d operator +(vi2d lhs, vi2d rhs) => new vi2d(lhs.x + rhs.x, lhs.y + rhs.y);
        public static vi2d operator -(vi2d lhs, vi2d rhs) => new vi2d(lhs.x - rhs.x, lhs.y - rhs.y);
        public static vi2d operator *(vi2d lhs, double rhs) => new vi2d(lhs.x * rhs, lhs.y * rhs);
        public static vi2d operator *(double lhs, vi2d rhs) => new vi2d(rhs.x * lhs, rhs.y * lhs);
        public static vi2d operator *(vi2d lhs, vi2d rhs) => new vi2d(lhs.x * rhs.x, lhs.y * rhs.y);
        public static vi2d operator /(vi2d lhs, double rhs) => new vi2d(lhs.x / rhs, lhs.y / rhs);
        public static vi2d operator /(vi2d lhs, vi2d rhs) => new vi2d(lhs.x / rhs.x, lhs.y / rhs.y);
        public static vi2d operator /(double lhs, vi2d rhs) => new vi2d(rhs.x / lhs, rhs.y / lhs);

        public static implicit operator vi2d(vu2d v) => new vi2d((int)v.x, (int)v.y);        
        public static implicit operator vi2d(vf2d v) => new vi2d((int)v.x, (int)v.y);
    }


    public class vu2d : vd2d
    {
        public vu2d() => (_x, _y) = (0, 0);
        public vu2d(vu2d v) => (_x, _y) = (v.x, v.y);
        public vu2d(uint x, uint y) => (_x, _y) = (x, y);

        public uint x { get { return (uint)_x; } set { _x = value; } }
        public uint y { get { return (uint)_y; } set { _y = value; } }

        public static vu2d operator +(vu2d lhs, vu2d rhs) => new vu2d(lhs.x + rhs.x, lhs.y + rhs.y);
        public static vu2d operator -(vu2d lhs, vu2d rhs) => new vu2d(lhs.x - rhs.x, lhs.y - rhs.y);
        public static vu2d operator *(vu2d lhs, double rhs) => new vu2d((uint)(lhs.x * rhs), (uint)(lhs.y * rhs));
        public static vu2d operator *(double lhs, vu2d rhs) => new vu2d((uint)(rhs.x * lhs), (uint)(rhs.y * lhs));
        public static vu2d operator *(vu2d lhs, vu2d rhs) => new vu2d(lhs.x * rhs.x, lhs.y * rhs.y);

        public static vu2d operator /(vu2d lhs, double rhs) => new vu2d((uint)(lhs.x / rhs), (uint)(lhs.y / rhs));
        public static vu2d operator /(double lhs, vu2d rhs) => new vu2d((uint)(rhs.x / lhs), (uint)(rhs.y / lhs));

        public static implicit operator vu2d(vi2d v) => new vu2d((uint)v.x, (uint)v.y);
        public static implicit operator vu2d(vf2d v) => new vu2d((uint)v.x, (uint)v.y);
    }

    public class vf2d : vd2d
    {
        public vf2d() => (_x, _y) = (0, 0);
        public vf2d(vf2d v) => (_x, _y) = (v.x, v.y);
        public vf2d(float x, float y) => (_x, _y) = (x, y);

        public float x { get { return (float)_x; } set { _x = value; } }
        public float y { get { return (float)_y; } set { _y = value; } }

        public static vf2d operator +(vf2d lhs, vf2d rhs) => new vf2d(lhs.x + rhs.x, lhs.y + rhs.y);
        public static vf2d operator -(vf2d lhs, vf2d rhs) => new vf2d(lhs.x - rhs.x, lhs.y - rhs.y);
        public static vf2d operator *(vf2d lhs, double rhs) => new vf2d((float)(lhs.x * rhs), (float)(lhs.y * rhs));
        public static vf2d operator *(double lhs, vf2d rhs) => new vf2d((float)(rhs.x * lhs), (float)(rhs.y * lhs));
        public static vf2d operator *(vf2d lhs, vf2d rhs) => new vf2d(lhs.x * rhs.x, lhs.y * rhs.y);
        public static vf2d operator /(vf2d lhs, double rhs) => new vf2d((float)(lhs.x / rhs), (float)(lhs.y / rhs));
        public static vf2d operator /(double lhs, vf2d rhs) => new vf2d((float)(rhs.x / lhs), (float)(rhs.y / lhs));

        public static implicit operator vf2d(vi2d v) => new vf2d((float)v.x, (float)v.y);
        public static implicit operator vf2d(vu2d v) => new vf2d((float)v.x, (float)v.y);
    }
}

