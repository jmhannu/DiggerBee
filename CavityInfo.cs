using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Geometry;

namespace DiggerBee
{
  class CavityInfo
  {
    public double ToolWidth; 
    public double ToolLength;
    public double MaxDepth;

    public Interval Insertion; 
    public Interval Sizes;
    public Interval Entries; 
    public Interval Angles;

    public CavityInfo()
    {
    }

    public CavityInfo(double _toolWidth, double _toolLength, double _maxDepth, Interval _insertion)
    {
      ToolWidth = _toolWidth;
      ToolLength = _toolLength;
      Insertion = _insertion;
      MaxDepth = _maxDepth;

      Angles = new Interval(35, 80);

      Tuple<double, double> minSizes = CalculateWidth(Angles.Max, Insertion.Max);

      Tuple<double, double> maxSizes = CalculateWidth(Angles.Min, Insertion.Min);
      double minWidth = minSizes.Item1;
      double maxWidth = maxSizes.Item1;

      Sizes = new Interval(minWidth, maxWidth);

      double maxE = minSizes.Item2;
      Entries = new Interval(maxSizes.Item2, maxE);
    }

    Tuple<double, double> CalculateWidth(double _angle, double _insertion)
    {
      Cavity c = new Cavity(_angle, ToolLength, ToolWidth, MaxDepth, _insertion);
      

      //return new Tuple<double, double>(c.Width, c.eDim);
      return new Tuple<double, double>(c.Width/2, c.eDim);
    }
  }
}
