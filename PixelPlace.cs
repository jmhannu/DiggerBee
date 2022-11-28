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
    Surface reparamSurface; 
    public Interval surfaceDomain1;
    public Interval surfaceDomain2;
    public List<Point3d> points;
    public List<Vector3d> normals; 

    public int xResolution;
    public int yResolution;

    public double xSize;
    public double ySize;
    public double gridSize;

    bool useX;
        double padding; 

    Interval colInterval;
    Interval sizes;

    public List<Circle> circleList;
    public List<double> depthList;
    public List<double> multiplicators;
    public List<double> debug;

    public PixelPlace(Surface _surface, Bitmap _image, Interval _sizes, double _padding, double _leaveWhite)
    {
      surface = _surface.Reverse(1);
      //surface = _surface;
      sizes = _sizes;
      surfaceDomain1 = surface.Domain(0);
      surfaceDomain2 = surface.Domain(1);
            padding = _padding; 

      points = new List<Point3d>();
      normals = new List<Vector3d>();
            debug = new List<double>();

      if(surface.IsSphere(0.1))
      {
        Sphere sphere;
        Cone cone;
        Cylinder cylinder;
        Torus torus; 
        
        if (surface.TryGetSphere(out sphere))
        {
          double radius = sphere.Radius;
          double circum = 2 * Math.PI * radius;
          xSize = circum;
          ySize = circum;
        }

        else if(surface.TryGetCone(out cone))
        {
          double radius = cone.Radius;
          double circum = 2 * Math.PI * radius;
          xSize = circum;
          ySize = cone.Height;
        }

        else if(surface.TryGetCylinder(out cylinder))
        {
          double radius = cylinder.Radius;
          double circum = 2 * Math.PI * radius;
          xSize = circum;
          ySize = cone.Height;
        }

        else if(surface.TryGetTorus(out torus))
        {
          double radius1 = torus.MinorRadius;
          double circum1 = 2 * Math.PI * radius1;
          xSize = circum1;

          double radius2 = torus.MajorRadius;
          double circum2 = 2 * Math.PI * radius2;
          ySize = circum2;
        }
      }

      else
      {
        xSize = surface.Domain(0).Max - surface.Domain(0).Min;
        ySize = surface.Domain(1).Max - surface.Domain(1).Min;
      }

      useX = CheckXDim(xSize, ySize);

      gridSize = sizes.Max;

      xResolution = (int)(xSize / gridSize);
      yResolution = (int)(ySize / gridSize);

            debug.Add((double)xResolution);
            debug.Add((double)yResolution);

            if (xResolution == 0) xResolution = 1;
      if (yResolution == 0) yResolution = 1;

      image = new System.Drawing.Bitmap(_image, xResolution, yResolution);

      colInterval = AvgRGB(_leaveWhite);
    }

      bool CheckXDim(double _xSize, double _ySize)
      {
        bool x; 

          if (_xSize < _ySize)
          {
          x = true;

          }
          else
          {
            x = false;
          }

        return x; 
      }

      Interval AvgRGB(double _leaveWhite)
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

            if (avg <= 255 - _leaveWhite)

            {
              if (cMin > avg) cMin = avg;
              if (cMax < avg) cMax = avg;
            }
          }
        }

        return new Interval(cMin, cMax);
      }

    public void StraightGrid(bool _distort, double _leaveWhite, double _xMove, double _yMove)
    {
      circleList = new List<Circle>();
      depthList = new List<double>();
      multiplicators = new List<double>();
      debug = new List<double>();

      reparamSurface = surface;

      Interval reparam = new Interval(0, 1);
      reparamSurface.SetDomain(0, reparam);
      reparamSurface.SetDomain(1, reparam);
      
      Interval xDomain = surface.Domain(0);
      Interval yDomain = surface.Domain(1);

      for (int y = 0; y < yResolution; y++)
      {
        for (int x = 0; x < xResolution; x++)
        {
          Color c = image.GetPixel(x, y);

          if (c.R <= 255 - _leaveWhite)
          {
            /*
             double px, py;

            if (_distort)
            {
              px = Utility.ReMap(x, 0, xResolution, xDomain.Min, xDomain.Max);
              py = Utility.ReMap(y, 0, yResolution, yDomain.Min, yDomain.Max);
            }

            else
            {
              if (useX)
              {
                px = Utility.ReMap(x, 0, xResolution, xDomain.Min, xDomain.Max);
                py = Utility.ReMap(y, 0, yResolution, yDomain.Min, yDomain.Min + xSize);
              }

              else
              {
                px = Utility.ReMap(x, 0, xResolution, xDomain.Min, xDomain.Min + ySize);
                py = Utility.ReMap(y, 0, yResolution, yDomain.Min, yDomain.Max);
              }
            }

            Point3d point = surface.PointAt(px + gridSize / 2 + _xMove, py + gridSize / 2 + _yMove);*/

            Point3d point = surface.PointAt(((x+0.5)/xResolution) + _xMove, ((y+0.5) / yResolution) + _yMove);
            points.Add(point);

            Vector3d normal = surface.NormalAt(((x + 0.5) / xResolution) + _xMove, ((y + 0.5) / yResolution) + _yMove);
            normals.Add(normal);

            Plane plane = new Plane(point, normal);

            double cSize = Utility.ReMap(c.R, colInterval.Min, colInterval.Max - _leaveWhite, sizes.Max, sizes.Min);
            double dSize = Utility.ReMap(c.R, colInterval.Min, colInterval.Max - _leaveWhite, 0.1, 1.0);
            double multiplicate = Utility.ReMap(c.R, colInterval.Min, colInterval.Max, 1.0, 0.1);

            //circleList.Add(new Circle(point, cSize/2));

            if (cSize > gridSize - padding) cSize = gridSize - padding;

            circleList.Add(new Circle(plane, point, cSize/2));
            depthList.Add(dSize);
            multiplicators.Add(multiplicate);
          }
        }
      }
    }
    }
}
