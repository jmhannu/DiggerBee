using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Drawing;

namespace DiggerBee
{
  class Recursion
  {
        public List<Rectangle3d> ListOfRectangles;
        public List<Element> ListOfElements;
        double padding; 
        double MinWidth;
        double threshold;
        double details;
        Bitmap image;

        Rectangle3d startRectangle;

        public List<string> debug;
        public Interval minMaxSize; 


        public Recursion(double _min, Bitmap _image, Rectangle3d _startRectangle, double _threshold, double _details, double _padding)
        {
            ListOfRectangles = new List<Rectangle3d>();
            ListOfElements = new List<Element>();
            MinWidth = _min;
            image = _image;
            threshold = _threshold;
            padding = _padding;

            startRectangle = _startRectangle;
            debug = new List<string>();

            //details = Remap(_details, 0.0, 1.0, 1.0, 0.0);
            details = _details;
            minMaxSize = new Interval(double.MaxValue, 0.0);
        }


        public void Division(Element _inputElement)
        {
            Rectangle3d inputRectangle = _inputElement.rectangle;

            Plane rectanglePlane = inputRectangle.Plane;

            for (int i = 0; i < 2; i++)
            {
                Rectangle3d newRectangle;
                if (inputRectangle.Width >= inputRectangle.Height)
                {
                    Point3d otherPoint = new Point3d(inputRectangle.Center.X, inputRectangle.Corner(3).Y, inputRectangle.Center.Z);

                    if (i == 0) newRectangle = new Rectangle3d(rectanglePlane, inputRectangle.Corner(0), otherPoint);
                    else newRectangle = new Rectangle3d(rectanglePlane, inputRectangle.Corner(1), otherPoint);
                }

                else
                {

                    Point3d otherPoint = new Point3d(inputRectangle.Corner(1).X, inputRectangle.Center.Y, inputRectangle.Center.Z);

                    if (i == 0) newRectangle = new Rectangle3d(rectanglePlane, inputRectangle.Corner(0), otherPoint);
                    else newRectangle = new Rectangle3d(rectanglePlane, inputRectangle.Corner(3), otherPoint);
                }

                int pixelU = (int)Math.Ceiling(Utility.ReMap(newRectangle.Center.X, 0.0, startRectangle.Width, 0.0, image.Width));
                int pixelV = (int)Math.Ceiling(Utility.ReMap(newRectangle.Center.Y, 0.0, startRectangle.Height, image.Height, 0.0));

                double avg = CalculateAverage(pixelU, pixelV, newRectangle);
                //double avg = CalculateMedian(pixelU, pixelV, newRectangle);

                double size;

                if (newRectangle.Width <= newRectangle.Height) size = newRectangle.Width;
                else size = newRectangle.Height;

                // debug.Add(avg.ToString());

                Element newElement = new Element(newRectangle, avg, size, padding);

                if (size < minMaxSize.T0) minMaxSize.T0 = size;
                if (size > minMaxSize.T1) minMaxSize.T1 = size;


                if (((size > MinWidth) && (avg < (threshold + details))) || ((size >= MinWidth) && (avg < threshold)))
                {
                    Division(newElement);
                }

                else
                {
                    ListOfRectangles.Add(newRectangle);
                    ListOfElements.Add(newElement);
                }

            }
        }

        double CalculateAverage(int _pixelU, int _pixelV, Rectangle3d _newRectangle)
        {
            double average = 0.0;

            int pixelCountU = (int)Math.Floor(image.Width * (_newRectangle.Width / startRectangle.Width));
            int pixelCountV = (int)Math.Floor(image.Height * (_newRectangle.Height / startRectangle.Height));

            //debug.Add(pixelU.ToString());
            //debug.Add(pixelCountU.ToString());

            int lowerU = (int)Math.Floor(-pixelCountU / 2.0);
            int upperU = (int)Math.Floor(pixelCountU / 2.0);
            // debug.Add(lowerU.ToString());
            // debug.Add(upperU.ToString());
            // debug.Add(image.Width.ToString());

            int lowerV = (int)Math.Floor(-pixelCountV / 2.0);
            int upperV = (int)Math.Floor(pixelCountV / 2.0);

            for (int x = lowerU; x < upperU; x++)
            {
                for (int y = lowerV; y < upperV; y++)
                {
                    Color pixelC = image.GetPixel(_pixelU + x, _pixelV + y);
                    //average = average + (((pixelC.R + pixelC.G + pixelC.B) / 3.0) / 255);
                    average = average + (((pixelC.R + pixelC.G + pixelC.B) / 3.0));
                    //debug.Add(pixelC.R.ToString());
                    //debug.Add(pixelC.G.ToString());
                    //debug.Add(pixelC.B.ToString());
                }
            }

            double divide = 1.0;

            if ((pixelCountU != 0) && (pixelCountV != 0)) divide = pixelCountU * pixelCountV;

            average = average / divide;

            return average;
        }

        double CalculateMedian(int _pixelU, int _pixelV, Rectangle3d _newRectangle)
        {
            double median = 0.0;

            List<double> values = new List<double>();

            int pixelCountU = (int)Math.Floor(image.Width * (_newRectangle.Width / startRectangle.Width));
            int pixelCountV = (int)Math.Floor(image.Height * (_newRectangle.Height / startRectangle.Height));

            int lowerU = (int)Math.Floor(-pixelCountU / 2.0);
            int upperU = (int)Math.Floor(pixelCountU / 2.0);

            int lowerV = (int)Math.Floor(-pixelCountV / 2.0);
            int upperV = (int)Math.Floor(pixelCountV / 2.0);

            for (int x = lowerU; x < upperU; x++)
            {
                for (int y = lowerV; y < upperV; y++)
                {
                    Color pixelC = image.GetPixel(_pixelU + x, _pixelV + y);
                    double average = ((pixelC.R + pixelC.G + pixelC.B) / 3.0) / 255.0;

                    values.Add(average);
                }
            }

            values.Sort();

            debug.Add(values.Count.ToString());

            if (values.Count == 0) median = 0.0;
            else if (values.Count == 1) median = values[0];
            else if (values.Count % 2 == 0) median = values[(values.Count / 2) - 1];
            else median = (values[(int)Math.Floor(values.Count / 2.0)] + values[(int)Math.Ceiling(values.Count / 2.0)]) / 2;


            return median;
        }
    }
}
