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
            pManager.AddCircleParameter("Circles", "Circles", "Circles where cavities should be placed.", GH_ParamAccess.list);
            pManager.AddIntervalParameter("DepthInterval", "DepthInterval", "Depth Interval", GH_ParamAccess.item);
            pManager.AddNumberParameter("Multiplicators", "Multiplicators", "List of multiplication numbers. One number for each cavity.", GH_ParamAccess.list);
            pManager.AddBooleanParameter("LowToHigh", "LowToHigh", "If true, multiplicators at 1.0 is translated to max depths. If false, this is inverted.", GH_ParamAccess.item, true);
            pManager.AddNumberParameter("EntryDepths", "Entry", "List of numbers between 0.0 and 1.0 where 0.0 means the entry is not inserted. One number for each cavity", GH_ParamAccess.list);
            pManager.AddNumberParameter("ToolWidth", "ToolWidth", "Width of milling tool", GH_ParamAccess.item);
            pManager.AddNumberParameter("ToolLength", "ToolLength", "Length of milling tool", GH_ParamAccess.item);

            pManager[6].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Debug", "Debug", "Debug", GH_ParamAccess.list);
            pManager.AddBrepParameter("Cavity", "Cavity", "Cavity as Brep", GH_ParamAccess.list);
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
            List<double> entries = new List<double>();
            double toolWidth = 0.0;
            double toolLength = 0.0;

            DA.GetDataList(0, circles);
            DA.GetData(1, ref depths);
            DA.GetDataList(2, multiplicators);
            DA.GetData(3, ref multiplyDepths);
            DA.GetDataList(4, entries);
            DA.GetData(5, ref toolWidth);
            DA.GetData(6, ref toolLength);

            List<Brep> cavaties = new List<Brep>();
            List<string> debug = new List<string>();

            CavityInfo cInfo;

            if (toolLength > 0) cInfo = new CavityInfo(toolWidth, toolLength, depths.T0, depths.T1);
            else cInfo = new CavityInfo(toolWidth, depths.T0, depths.T1);

            if (multiplicators.Count < circles.Count)
            {
                debug.Add("The number of Multiplicators needs to be the same as the number of Circles.");
            }

            if (entries.Count < circles.Count)
            {
                debug.Add("The number of EntryDepths needs to be the same as the number of Circles.");
            }

            else
            {
                for (int i = 0; i < circles.Count; i++)
                {
                    Cavity c = new Cavity(circles[i], multiplicators[i], multiplyDepths, cInfo, entries[i]);

                    for (int j = 0; j < c.lofts.Count; j++)
                    {
                        cavaties.Add(c.lofts[j]);
                    }
                }
            }

            DA.SetDataList(0, debug);
            DA.SetDataList(1, cavaties);
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