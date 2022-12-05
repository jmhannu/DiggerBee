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
            pManager.AddNumberParameter("Multiplicators", "Multiplicators", "Multiplicators", GH_ParamAccess.list);
            pManager.AddIntervalParameter("DepthInterval", "Depths", "Depth Interval", GH_ParamAccess.item);
            pManager.AddBooleanParameter("MultiplyDepths", "MultiplyDepths", "If true, multiplicators at 1.0 is translated to max depths. If false, this is inverted.", GH_ParamAccess.item, true);
            pManager.AddNumberParameter("EntryDepth", "Entry", "Entry depth, a number between 0.0 and 1.0", GH_ParamAccess.item);
            pManager.AddNumberParameter("ToolWidth", "ToolWidth", "Width of milling tool", GH_ParamAccess.item);
            pManager.AddNumberParameter("ToolLength", "ToolLength", "Length of milling tool", GH_ParamAccess.item);

            pManager[6].Optional = true;
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
            List<double> multiplicators = new List<double>();
            Interval depths = new Interval();
            bool multiplyDepths = true; 
            double entry = 0.0; 
            double toolWidth = 0.0;
            double toolLength = 0.0;

            DA.GetDataList(0, circles);
            DA.GetDataList(1, multiplicators);
            DA.GetData(2, ref depths);
            DA.GetData(3, ref multiplyDepths);
            DA.GetData(4, ref entry);
            DA.GetData(5, ref toolWidth);
            DA.GetData(6, ref toolLength);

            List<Brep> cavaties = new List<Brep>();
            List<Brep> cones = new List<Brep>();

            CavityInfo cInfo;

            if (toolLength > 0) cInfo = new CavityInfo(toolWidth, toolLength, depths.T0, depths.T1);
            else cInfo = new CavityInfo(toolWidth, depths.T0, depths.T1);

            for (int i = 0; i < circles.Count; i++)
            {
                Cavity c = new Cavity(circles[i], multiplicators[i], multiplyDepths, cInfo, entry);

                if (c.lofts.Count > 0) cavaties.Add(c.lofts[0]);
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