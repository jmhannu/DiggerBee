using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//using Grasshopper;
//using Grasshopper.Kernel;
//using Grasshopper.Kernel.Data;
using Rhino.Geometry;

namespace DiggerBee
{
    class PixelPlace
    {
        Bitmap image;
        Surface surface;

        public List<Point3d> points;
        public List<Vector3d> normals;
        public List<Circle> circleList;
        public List<double> depthList;
        public List<double> multiplicators;
        public List<double> debug;

        public int xResolution;
        public int yResolution;

        public double xSize;
        public double ySize;
        public double gridSize;

        bool distort;
        double padding;
        double leaveWhite;

        Interval colInterval;
        Interval sizes;

        public PixelPlace(Surface _surface, Bitmap _image, Interval _sizes, double _padding, double _leaveWhite, bool _distort)
        {
            surface = _surface.Reverse(1);
            sizes = _sizes;
            padding = _padding;
            distort = _distort;
            leaveWhite = _leaveWhite;

            circleList = new List<Circle>();
            depthList = new List<double>();
            multiplicators = new List<double>();
            points = new List<Point3d>();
            normals = new List<Vector3d>();
            debug = new List<double>();

            XYsize();

            gridSize = sizes.Max;

            ResolutionXY(_image);

            image = new System.Drawing.Bitmap(_image, xResolution, yResolution);

            colInterval = AvgRGB();
        }

        void ReparametrizeSrf()
        {
            Interval reparam = new Interval(0, 1);

            if(distort)
            {
                surface.SetDomain(0, reparam);
                surface.SetDomain(1, reparam);
            }

            else
            {
                Interval other;

                if (xSize < ySize)
                {
                    surface.SetDomain(0, reparam);

                    other = new Interval(0, ySize / xSize);
                    surface.SetDomain(1, other);
                }

                else
                {
                    surface.SetDomain(1, reparam);
                    
                    other = new Interval(0, xSize / ySize);
                    surface.SetDomain(0, other);
                }
            }

        }

        void ResolutionXY(Bitmap _image)
        {
            if (distort)
            {
                xResolution = (int)(xSize / gridSize);
                yResolution = (int)(ySize / gridSize);
            }

            else
            {
                if (xSize < ySize)
                {
                    xResolution = (int)(xSize / gridSize);
                    double divFactor = _image.Width / _image.Height;
                    yResolution = (int)(xResolution / divFactor);
                }

                else
                {
                    yResolution = (int)(ySize / gridSize);
                    double divFactor = _image.Height / _image.Width;
                    xResolution = (int)(yResolution / divFactor);
                }
            }

            if (xResolution < 1) xResolution = 1;
            if (yResolution < 1) yResolution = 1;
        }

        void XYsize()
        {
            if (surface.IsSphere(0.1))
            {
                surface.TryGetSphere(out Sphere sphere);
                double radius = sphere.Radius;
                double circum = 2 * Math.PI * radius;
                xSize = circum;
                ySize = circum;
            }

            else if (surface.IsCone(0.1))
            {
                surface.TryGetCone(out Cone cone);
                double radius = cone.Radius;
                double circum = 2 * Math.PI * radius;
                xSize = circum;
                ySize = cone.Height;
            }

            else if (surface.IsCylinder(0.1))
            {
                surface.TryGetCylinder(out Cylinder cylinder);
                double radius = cylinder.Radius;
                double circum = 2 * Math.PI * radius;
                xSize = circum;
                ySize = cylinder.Height1;
            }

            else if (surface.IsTorus(0.1))
            {
                surface.TryGetTorus(out Torus torus);
                double radius1 = torus.MinorRadius;
                double circum1 = 2 * Math.PI * radius1;
                xSize = circum1;

                double radius2 = torus.MajorRadius;
                double circum2 = 2 * Math.PI * radius2;
                ySize = circum2;
            }

            else
            {
                xSize = surface.Domain(0).Max - surface.Domain(0).Min;
                ySize = surface.Domain(1).Max - surface.Domain(1).Min;
            }
        }

        Interval AvgRGB()
        {

            int cMin = int.MaxValue;
            int cMax = 0;

            for (int y = 0; y < yResolution; y++)
            {
                for (int x = 0; x < xResolution; x++)
                {
                    Color c = image.GetPixel(x, y);

                    int r = c.R;
                    int g = c.G;
                    int b = c.B;
                    int avg = (r + g + b) / 3;
                    image.SetPixel(x, y, Color.FromArgb(avg, avg, avg));

                    if (avg <= 255 - leaveWhite)

                    {
                        if (cMin > avg) cMin = avg;
                        if (cMax < avg) cMax = avg;
                    }
                }
            }

            return new Interval(cMin, cMax);
        }

        public void StraightGrid(double _xMove, double _yMove)
        {
            ReparametrizeSrf(); 

            for (int y = 0; y < yResolution; y++)
            {
                for (int x = 0; x < xResolution; x++)
                {
                    Color c = image.GetPixel(x, y);

                    if (c.R <= 255 - leaveWhite)
                    {

                        Point3d point = surface.PointAt(((x + 0.5) / xResolution) + _xMove, ((y + 0.5) / yResolution) + _yMove);
                        points.Add(point);

                        Vector3d normal = surface.NormalAt(((x + 0.5) / xResolution) + _xMove, ((y + 0.5) / yResolution) + _yMove);
                        normals.Add(normal);

                        Plane plane = new Plane(point, normal);

                        double cSize = Utility.ReMap(c.R, colInterval.Min, colInterval.Max, sizes.Max, sizes.Min);
                        double dSize = Utility.ReMap(c.R, colInterval.Min, colInterval.Max, 0.1, 1.0);
                        double multiplicate = Utility.ReMap(c.R, colInterval.Min, colInterval.Max, 1.0, 0.1);

                        if (cSize > gridSize - padding) cSize = gridSize - padding;

                        circleList.Add(new Circle(plane, point, cSize / 2));
                        depthList.Add(dSize);
                        multiplicators.Add(multiplicate);
                    }
                }
            }
        }
    }
}
