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
    public Interval uLocation;
    public Interval vLocation;

    public NurbsSurface panel;

    public double size;

    public Element(NurbsSurface _panel, Interval _uInterval, Interval _vInterval, double _size)
    {
      uLocation = _uInterval;
      vLocation = _vInterval;

      panel = _panel;

      size = _size;
    }
  }
}
