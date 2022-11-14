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
          : base("Subdivsion", "Subdivision",
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
            pManager.AddNumberParameter("MinR", "MinR", "Minimum size", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Threshold", "Threshold", "Threshold", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Detailing", "Detailing", "Detailing", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddRectangleParameter("A", "A", "A", GH_ParamAccess.list);
            pManager.AddPointParameter("B", "B", "B", GH_ParamAccess.list);
            pManager.AddNumberParameter("C", "C", "C", GH_ParamAccess.list);
            pManager.AddTextParameter("D", "D", "D", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

        Rectangle3d Rectangle = new Rectangle3d();
        string ImagePath = null;
        double MinR = 0.0;
        int Threshold = 0;
        int Detailing = 0;

        DA.GetData(0, ref Rectangle);
        DA.GetData(1, ref ImagePath);
        DA.GetData(2, ref MinR);
        DA.GetData(3, ref Threshold);
        DA.GetData(4, ref Detailing);


            Bitmap image = new Bitmap(ImagePath);

            Recursion r = new Recursion(MinR, image, Rectangle, Threshold, Detailing);

            double firstSize;
            if (Rectangle.Width <= Rectangle.Height) firstSize = Rectangle.Width;
            else firstSize = Rectangle.Height;

            Element firstElement = new Element(Rectangle, 1.0, firstSize);
            r.Division(firstElement);

            List<Point3d> points = new List<Point3d>();
            List<double> sizes = new List<double>();

            for (int i = 0; i < r.ListOfElements.Count; i++)
            {
                points.Add(r.ListOfElements[i].center);
                sizes.Add(r.ListOfElements[i].size);
            }

            DA.SetDataList(0, r.ListOfRectangles);
            DA.SetDataList(1, points);
            DA.SetDataList(2, sizes);
            DA.SetDataList(3, r.debug);
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