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
            pManager.AddNumberParameter("Depths", "Depths", "Depths", GH_ParamAccess.list);
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
            List<Circle> ci = new List<Circle>();
            CavityInfo cInfo = new CavityInfo();
            List<double> multiplicator = new List<double>();
            DA.GetDataList(0, ci);
            DA.GetData(1, ref cInfo);
            DA.GetDataList(2, multiplicator);

            List<Brep> cavaties = new List<Brep>();
            List<Brep> cones = new List<Brep>();

            for (int i = 0; i < ci.Count; i++)
            {
                Cavity c = new Cavity(ci[i], multiplicator[i], cInfo);

                if(c.lofts.Count > 0) cavaties.Add(c.lofts[0]);
                if (c.cone != null) cones.Add(c.cone);
            }


            DA.SetDataList(0, cavaties);
            DA.SetDataList(1, cones);
            // DA.SetData(1, c.mDepth);
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