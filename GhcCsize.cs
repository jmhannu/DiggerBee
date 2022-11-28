using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace DiggerBee
{
    public class GhcCsize : GH_Component
    {

        public GhcCsize()
          : base("Cavaty sizes", "Csize",
            "Gets the possible range size for milled cavaties based on the milling tool size",
            "DiggerBee", "Cavaties")
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
        pManager.AddNumberParameter("ToolWidth", "ToolWidth", "Width of milling tool", GH_ParamAccess.item);
        pManager.AddNumberParameter("ToolLength", "ToolLength", "Length of milling tool", GH_ParamAccess.item);
        pManager.AddNumberParameter("MinSize", "MinSize", "MinSize", GH_ParamAccess.item);
        pManager.AddNumberParameter("MaxSize", "MaxSize", "MaxSize", GH_ParamAccess.item);
        pManager.AddNumberParameter("MinDepth", "MinDepth", "MinDepth", GH_ParamAccess.item);
        pManager.AddNumberParameter("MaxDepth", "MaxDepth", "MaxDepth", GH_ParamAccess.item);

            // If you want to change properties of certain parameters, 
            // you can use the pManager instance to access them by index:
            //pManager[0].Optional = true;
        }

    protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
     {
      pManager.AddIntervalParameter("SizeInterval", "SizeInterval", "Interval with the of possible range of sizes", GH_ParamAccess.item);
      pManager.AddGenericParameter("CavatyInfo", "CInfo", "Production info for cavaties", GH_ParamAccess.item);

      // Sometimes you want to hide a specific parameter from the Rhino preview.
      // You can use the HideParameter() method as a quick way:
      //pManager.HideParameter(0);
    }

        
        protected override void SolveInstance(IGH_DataAccess DA)
        {
          double toolWidth = 0.0;
          double toolLength = 0.0;
          double minSize = 0.0;
          double maxSize = 0.0;
          double minDepth = 0.0;
          double maxDepth = 0.0;

          DA.GetData(0, ref toolWidth);
          DA.GetData(1, ref toolLength);
          DA.GetData(2, ref minSize);
          DA.GetData(3, ref maxSize);
          DA.GetData(4, ref minDepth);
          DA.GetData(5, ref maxDepth);

          CavityInfo cInfo = new CavityInfo(toolWidth, toolLength, minDepth, maxDepth, minSize, maxSize);

          DA.SetData(0, cInfo.Sizes);
          DA.SetData(1, cInfo);
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override System.Drawing.Bitmap Icon
    {
      get
      {
        return DiggerBee.Properties.Resources.IconCsize;
      }
    }

        public override Guid ComponentGuid => new Guid("9A1039D4-C6A6-46B9-BEB9-A48B038023ED");
    }
}