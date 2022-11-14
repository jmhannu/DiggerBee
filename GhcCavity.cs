using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DiggerBee
{
    public class GhcCavity : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GhcCavity class.
        /// </summary>
        public GhcCavity()
          : base("Cavity", "Cavity",
              "Lofts cavity",
              "Cricket", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
         pManager.AddCircleParameter("Circle", "Circle", "Circle to mark cavity", GH_ParamAccess.item);
         pManager.AddGenericParameter("Cinfo", "Cinfo", "Cinfo", GH_ParamAccess.item);
         pManager.AddNumberParameter("Depth", "Depth", "Depth", GH_ParamAccess.item);
    }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
         pManager.AddBrepParameter("Cavity", "Cavity", "Cavity as Brep", GH_ParamAccess.list);
         pManager.AddBrepParameter("Cone", "Cone", "Cone", GH_ParamAccess.item);
    }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
          Circle ci = new Circle();
          CavityInfo cInfo = new CavityInfo();
          double multiplicator = 0.0;
          DA.GetData(0, ref ci);
          DA.GetData(1, ref cInfo);
          DA.GetData(2, ref multiplicator);

          Cavity c = new Cavity(ci, multiplicator, cInfo);

          DA.SetDataList(0, c.lofts);
         DA.SetData(1, c.cone);
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
                return Cricket.Properties.Resources.IconCavity;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("602172E6-C730-4D49-ACC9-D87E56DE6DB2"); }
        }
  }
}