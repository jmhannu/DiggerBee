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

        public Interval Angles;
        public Interval Depths;

        public CavityInfo()
        { }

        public CavityInfo(double _toolWidth, double _minDepth, double _maxDepth)
        {
            ToolWidth = _toolWidth;
            Depths = new Interval(_minDepth, _maxDepth);

            Angles = new Interval(35, 80);

            double maxRadians = (Math.PI / 180) * Angles.Max;
            ToolLength = _maxDepth / Math.Sin(maxRadians);
        }

        public CavityInfo(double _toolWidth, double _toolLength, double _minDepth, double _maxDepth)
        {
            ToolWidth = _toolWidth;
            ToolLength = _toolLength;

            Depths = new Interval(_minDepth, _maxDepth);

            Angles = new Interval(35, 80);
        }
    }
}
