using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DiggerBee
{
    public class GhcRecursive : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GhcRecursive class.
        /// </summary>
        public GhcRecursive()
          : base("GhcRecursive", "Recursive",
              "Recursive",
              "Cricket", "Placing")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
    {
      pManager.AddSurfaceParameter("Surface", "Surface", "Surface to place on", GH_ParamAccess.item);
      pManager.AddNumberParameter("MinSize", "MinSize", "Minimum size", GH_ParamAccess.item);
    }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
      pManager.AddSurfaceParameter("allPanels", "allPanels", "allPanels", GH_ParamAccess.list);
      pManager.AddNumberParameter("sizes", "sizes", "sizes", GH_ParamAccess.list);
      pManager.AddTextParameter("bugs", "bugs", "bugs", GH_ParamAccess.list);
    }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

        Surface surface = null;
        double MinSize = 0.0;

        DA.GetData(0, ref surface);
        DA.GetData(1, ref MinSize);


      Recursion r = new Recursion(MinSize);

      double uRange = surface.Domain(0).Max - surface.Domain(0).Min;
      double vRange = surface.Domain(1).Max - surface.Domain(1).Min;

      double size;
      if (uRange > vRange) size = vRange;
      else size = uRange;

      NurbsSurface nrbs = surface.ToNurbsSurface();

      Interval domain = new Interval(0, 1);
      nrbs.SetDomain(0, domain);
      nrbs.SetDomain(1, domain);

      Element firstElement = new Element(nrbs, domain, domain, size);


      r.Division(firstElement, 0.5, true);

      List<NurbsSurface> allPanels = new List<NurbsSurface>();
      List<double> sizes = new List<double>();
      List<string> bugs = new List<string>();

      for (int i = 0; i < r.ListOfElements.Count; i++)
      {
        allPanels.Add(r.ListOfElements[i].panel);
        sizes.Add(r.ListOfElements[i].size);
        bugs.Add(r.debug);
      }

      DA.SetDataList(0, allPanels);
      DA.SetDataList(1, sizes);
      DA.SetDataList(2, bugs);
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