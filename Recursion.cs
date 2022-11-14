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
  class Recursion
  {
    public List<Element> ListOfElements;
    public string debug;
    double minSize;

    public Recursion(double _min)
    {
      ListOfElements = new List<Element>();
      minSize = _min;
    }

    public void Division(Element _inputElement, double _toAdd, bool _uDivide)
    {
      ListOfElements.Add(_inputElement);

      Interval uDomain = _inputElement.uLocation;
      Interval vDomain = _inputElement.vLocation;
      NurbsSurface surface = _inputElement.panel;

      Point3d ppt1 = surface.PointAt(uDomain.Min, vDomain.Min);
      Point3d ppt2 = surface.PointAt(uDomain.Max, vDomain.Min);
      Point3d ppt3 = surface.PointAt(uDomain.Max, vDomain.Max);
      Point3d ppt4 = surface.PointAt(uDomain.Min, vDomain.Max);

      Point3d pptdiv1, pptdiv2;

      for (int i = 0; i < 2; i++)
      {
        NurbsSurface panel;
        Interval newV;
        Interval newU;

        if (_uDivide)
        {
          pptdiv1 = surface.PointAt(uDomain.Min + _toAdd, vDomain.Min);
          pptdiv2 = surface.PointAt(uDomain.Min + _toAdd, vDomain.Max);

          newV = new Interval(vDomain.Min, vDomain.Max);

          if (i % 2 == 0)
          {
            panel = NurbsSurface.CreateFromCorners(ppt1, pptdiv1, pptdiv2, ppt4);
            newU = new Interval(uDomain.Min, _toAdd);
          }

          else
          {
            panel = NurbsSurface.CreateFromCorners(pptdiv1, ppt2, ppt3, pptdiv2);
            newU = new Interval(_toAdd, uDomain.Max);
          }
        }

        else
        {
          pptdiv1 = surface.PointAt(uDomain.Min, vDomain.Min + _toAdd);
          pptdiv2 = surface.PointAt(uDomain.Max, vDomain.Min + _toAdd);

          newU = new Interval(uDomain.Min, uDomain.Max);

          if (i % 2 == 0)
          {
            panel = NurbsSurface.CreateFromCorners(ppt1, ppt2, pptdiv2, pptdiv1);
            newV = new Interval(vDomain.Min, _toAdd);
          }

          else
          {
            panel = NurbsSurface.CreateFromCorners(pptdiv1, pptdiv2, ppt3, ppt4);
            newV = new Interval(_toAdd, vDomain.Max);
          }
        }

        double size = ppt1.DistanceTo(pptdiv1);

        Element newElement = new Element(panel, newU, newV, size);

        if (newElement.size >= minSize)
        {
          debug = "reached";
          Division(newElement, _toAdd / 2, !_uDivide);
        }
      }
    }
  }
}
