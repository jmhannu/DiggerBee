using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DiggerBee
{
  class Element
  {
        public Rectangle3d rectangle;
        public Circle circle;
        public bool last;
        public double grey;
        public Point3d center;
        public double size;

        public Element(Rectangle3d _rectangle, double _avg, double _size, double _padding)
        {
            rectangle = _rectangle;
            last = false;
            grey = _avg;
            center = _rectangle.Center;
            size = _size;

            circle = new Circle(center, Math.Abs(_size / 2) - _padding);
        }
    }
}
