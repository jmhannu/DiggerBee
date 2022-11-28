using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DiggerBee
{
    public class GhcCavity : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GhcCavityReworked class.
        /// </summary>
        public GhcCavity()
          : base("Cavity", "Cavity",
              "Cavity",
              "DiggerBee", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCircleParameter("Circle", "Circle", "Circle to mark cavity", GH_ParamAccess.list);
            pManager.AddNumberParameter("Multiplicator", "Multiplicator", "Multiplicator", GH_ParamAccess.list);
            pManager.AddNumberParameter("MinDepth", "MinDepth", "MinDepth", GH_ParamAccess.item);
            pManager.AddNumberParameter("MaxDepth", "MaxDepth", "MaxDepth", GH_ParamAccess.item);
            pManager.AddNumberParameter("ToolWidth", "ToolWidth", "Width of milling tool", GH_ParamAccess.item);
            pManager.AddNumberParameter("ToolLength", "ToolLength", "Length of milling tool", GH_ParamAccess.item);

            pManager[5].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Cavity", "Cavity", "Cavity as Brep", GH_ParamAccess.list);
            pManager.AddBrepParameter("Cone", "Cone", "Cone", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Circle> circles = new List<Circle>();
            List<double> multiplicator = new List<double>();
            double minDepth = 0.0;
            double maxDepth = 0.0;
            double toolWidth = 0.0;
            double toolLength = 0.0;

            DA.GetDataList(0, circles);
            DA.GetDataList(1, multiplicator);
            DA.GetData(2, ref minDepth);
            DA.GetData(3, ref maxDepth);
            DA.GetData(4, ref toolWidth);
            DA.GetData(5, ref toolLength);

            List<Brep> cavaties = new List<Brep>();
            List<Brep> cones = new List<Brep>();

            CavityInfo cInfo;

            if (toolLength > 0) cInfo = new CavityInfo(toolWidth, toolLength,  minDepth, maxDepth);
            else cInfo = new CavityInfo(toolWidth, minDepth, maxDepth);

            for (int i = 0; i < circles.Count; i++)
            {
                Cavity c = new Cavity(circles[i], multiplicator[i], cInfo);

                if(c.lofts.Count > 0) cavaties.Add(c.lofts[0]);
                if (c.cone != null) cones.Add(c.cone);
            }

            DA.SetDataList(0, cavaties);
            DA.SetDataList(1, cones);
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
                return DiggerBee.Properties.Resources.IconCavity;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("E009DE70-C1A9-4D32-B1DE-789287C3C6D3"); }
        }
    }
}