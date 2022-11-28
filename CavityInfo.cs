﻿using System;
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

    public Interval Sizes;
    public Interval Angles;
    public Interval Depths; 

    public CavityInfo()
    { }
    public CavityInfo(double _toolWidth, double _toolLength, double _minDepth, double _maxDepth, double _minSize, double _maxSize)
    {
      ToolWidth = _toolWidth;
      ToolLength = _toolLength;
  
      MaxDepth = _maxDepth;
      Depths = new Interval(_minDepth, _maxDepth);
      
      Angles = new Interval(35, 80);
        
      if(_minSize > _toolWidth) Sizes = new Interval(_minSize, _maxSize);
      else Sizes = new Interval(_toolWidth, _maxSize);
      }
  }
}
