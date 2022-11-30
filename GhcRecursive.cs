using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Drawing;

namespace DiggerBee
{
    public class GhcRecursive : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GhcRecursive class.
        /// </summary>
        public GhcRecursive()
          : base("Recursive", "Recursive",
              "Recursive subdivision",
              "DiggerBee", "Placing")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddRectangleParameter("Rectangle", "Rectangle", "Rectangle to place on", GH_ParamAccess.item);
            pManager.AddTextParameter("ImagePath", "ImagePath", "Image path", GH_ParamAccess.item);
            pManager.AddIntervalParameter("SizeInterval", "SizeInterval", "Size interval", GH_ParamAccess.item);
            pManager.AddNumberParameter("Padding", "Padding", "Padding", GH_ParamAccess.item, 0.0);
            pManager.AddIntegerParameter("Threshold", "Threshold", "Threshold", GH_ParamAccess.item);
            pManager.AddBooleanParameter("DivBlack", "DivBlack", "DivBlack", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("LeaveBlank", "LeaveBlank", "LeaveBlank", GH_ParamAccess.item, true);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddRectangleParameter("Rectangles", "Rectangles", "Rectangles", GH_ParamAccess.list);
            pManager.AddPointParameter("Points", "Points", "Points", GH_ParamAccess.list);
            pManager.AddCircleParameter("Circles", "Circles", "Circles", GH_ParamAccess.list);
             pManager.AddNumberParameter("Multiplicator", "Multiplicator", "Multiplicator", GH_ParamAccess.list);
            //pManager.AddNumberParameter("Sizes", "Sizes", "Sizes", GH_ParamAccess.list);
            pManager.AddTextParameter("Debug", "Debug", "Debug", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            Rectangle3d inputRect = new Rectangle3d();
            string imagePath = null;
            Interval sizes = new Interval();
            double padding = 0.0;
            int threshold = 0;
            bool divBlack = true;
            bool leaveBlank = true; 

            DA.GetData(0, ref inputRect);
            DA.GetData(1, ref imagePath);
            DA.GetData(2, ref sizes);
            DA.GetData(3, ref padding);
            DA.GetData(4, ref threshold);
            DA.GetData(5, ref divBlack);
            DA.GetData(6, ref leaveBlank);

            Bitmap image = new Bitmap(imagePath);

            Recursion r = new Recursion(sizes.T0, sizes.T1, image, inputRect, threshold, padding, divBlack, leaveBlank);

            double firstSize;
            if (inputRect.Width <= inputRect.Height) firstSize = inputRect.Width;
            else firstSize = inputRect.Height;

            Element firstElement = new Element(inputRect, 1.0, firstSize);
            r.Division(firstElement);

            List<Point3d> points = new List<Point3d>();
            List<Circle> circles = new List<Circle>();
            //List<double> sizeList = new List<double>();
            List<double> multiplicators = new List<double>();

            for (int i = 0; i < r.ListOfElements.Count; i++)
            {
                points.Add(r.ListOfElements[i].center);
                //sizeList.Add(r.ListOfElements[i].size);
                circles.Add(r.ListOfElements[i].circle);
                double multiplicator = Utility.ReMap(r.ListOfElements[i].size, r.minMaxSize.T0, r.minMaxSize.T1, 1.0, 0.1);
                multiplicators.Add(multiplicator);
            }

            DA.SetDataList(0, r.ListOfRectangles);
            DA.SetDataList(1, points);
            DA.SetDataList(2, circles);
            DA.SetDataList(3, multiplicators);
            DA.SetDataList(4, r.debug);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("E7A88C9D-995C-49D3-BE1E-7C1BC264DDB7"); }
        }
    }
}