﻿using System;
using System.Drawing;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;


namespace DiggerBee
{
    public class GhcPixelPlace : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GhcPixelPlace class.
        /// </summary>
        public GhcPixelPlace()
          : base("Pixel placing", "PixelPlace",
              "Places holes/cavaites by the colours of an image",
              "DiggerBee", "Placing")
        {
        }


    /// <summary>
    /// Registers all the input parameters for this component.
    /// </summary>
    protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
          pManager.AddTextParameter("ImagePath", "ImagePath", "Path to where the image is located", GH_ParamAccess.item);
          pManager.AddSurfaceParameter("Surface", "Surface", "Surface", GH_ParamAccess.item);
          pManager.AddIntervalParameter("SizeInterval", "SizeInterval", "SizeInterval", GH_ParamAccess.item);
          pManager.AddNumberParameter("Threshold", "Threshold", "Threshold to leave white areas as solid", GH_ParamAccess.item, 0.0);
          pManager.AddNumberParameter("Padding", "Padding", "Padding", GH_ParamAccess.item, 0.0);
          pManager.AddBooleanParameter("Distort", "Distort", "Distort", GH_ParamAccess.item, true);
          pManager.AddNumberParameter("MoveU", "MoveU", "Move placement in the U-direction on the surface. To keep placed items on the surface, a value between 0 and 1 is recommended.", GH_ParamAccess.item, 0.0);
          pManager.AddNumberParameter("MoveV", "MoveV", "Move placement in the V-direction on the surface. To keep placed items on the surface, a value between 0 and 1 is recommended.", GH_ParamAccess.item, 0.0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
          pManager.AddPointParameter("Points", "Points", "Points", GH_ParamAccess.list);
          pManager.AddCircleParameter("Circles", "Circles", "Circles", GH_ParamAccess.list);
          pManager.AddNumberParameter("Multiplicators", "Multiplicators", "Multiplicators", GH_ParamAccess.list);
          pManager.AddIntegerParameter("xResolution", "xResolution", "xResolution", GH_ParamAccess.item);
          pManager.AddIntegerParameter("yResolution", "yResolution", "yResolution", GH_ParamAccess.item);
          pManager.AddNumberParameter("SizeX", "SizeX", "SizeX", GH_ParamAccess.item);
          pManager.AddNumberParameter("SizeY", "SizeY", "SizeY", GH_ParamAccess.item);
          pManager.AddNumberParameter("GridSize", "GridSize", "GridSize", GH_ParamAccess.item);
          pManager.AddNumberParameter("Debug", "Debug", "Debug", GH_ParamAccess.list);
        }

    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
        {
         string filePath = null;
         Interval sizes = new Interval();
         double leaveWhite = 0.0;
         Surface surface = null;
         double padding = 0.0;
         bool distort = true;
         double xMove = 0.0;
         double yMove = 0.0;

         

         DA.GetData(0, ref filePath);
         DA.GetData(1, ref surface);
         DA.GetData(2, ref sizes);
         DA.GetData(3, ref leaveWhite);
         DA.GetData(4, ref padding);
         DA.GetData(5, ref distort);
         DA.GetData(6, ref xMove);
         DA.GetData(7, ref yMove);


        System.Drawing.Bitmap image = new System.Drawing.Bitmap(filePath);

        PixelPlace place = new PixelPlace(surface, image, sizes, padding, leaveWhite, distort);
        place.StraightGrid(xMove, yMove);

        DA.SetDataList(0, place.points);
        DA.SetDataList(1, place.circleList);
        DA.SetDataList(2, place.multiplicators);
        DA.SetData(3, place.xResolution);
        DA.SetData(4, place.yResolution);
        DA.SetData(5, place.xSize);
        DA.SetData(6, place.ySize);
        DA.SetData(7, place.gridSize);
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
                return DiggerBee.Properties.Resources.IconPixelPlace;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("F8B28398-AF73-4208-9688-3A5A5D1994C0"); }
        }
    }
}