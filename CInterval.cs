using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Geometry;

namespace DiggerBee
{
  class CInterval
  {
    public Interval ThisInterval;
    public int Type;
    public int Range; 

    CInterval(Interval _frequencies, double _toolSize, int _type)
    {
      double fMin = _frequencies.Min;
      double fMax = _frequencies.Max;

      double tool = _toolSize + 2.0;

      double cMin = (((1 - tool) / 10) * fMin) + tool;
      double cMax = (((1 - tool) / 10) * fMax) + tool;

      ThisInterval = new Interval(cMin, cMax);

      Type = _type;

      if (fMin < 500) Range = 0;
      else if (fMin > 500 && fMin < 2000) Range = 1;
      else Range = 3; 
    }

    public double RandomSize()
    {
      Random random = new Random();
      return random.NextDouble() * (ThisInterval.Min - ThisInterval.Max) + ThisInterval.Min;
    }

  }
}
