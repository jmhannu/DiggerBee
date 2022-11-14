﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiggerBee
{
  public class Utility
  {
    Utility()
    {
    }

    static public double GetRandomNumber(double minimum, double maximum)
    {
      Random random = new Random();
      return random.NextDouble() * (maximum - minimum) + minimum;
    }

    static public double ReMap(double value, double from1, double to1, double from2, double to2)
    {
      return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
  }
}
