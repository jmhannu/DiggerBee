using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace DiggerBee
{
    public class GhcDistortGrid : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public GhcDistortGrid()
          : base("DistortGrid", "DG",
            "Distort Grid",
            "DiggerBee", "Placing")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            // Use the pManager object to register your input parameters.
            // You can often supply default values when creating parameters.
            // All parameters must have the correct access type. If you want 
            // to import lists or trees of values, modify the ParamAccess flag.
            pManager.AddPointParameter("PointTree", "PT", "Point grid as tree", GH_ParamAccess.tree);
            pManager.AddPointParameter("repPoints", "RP", "Repelling points", GH_ParamAccess.list);
            pManager.AddNumberParameter("charge", "C", "Charge", GH_ParamAccess.item, 10.0);
            pManager.AddNumberParameter("decay", "D", "Decay", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("threshold", "T", "Threshold between 0.0 and 1.0", GH_ParamAccess.item, 1.0);
            pManager.AddIntegerParameter("crvDegree", "CD", "Nurbs curve degree", GH_ParamAccess.item, 3);

            // If you want to change properties of certain parameters, 
            // you can use the pManager instance to access them by index:
            //pManager[0].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            // Use the pManager object to register your output parameters.
            // Output parameters do not have default values, but they too must have the correct access type.
            pManager.AddPointParameter("points", "PT", "Points", GH_ParamAccess.tree);
            pManager.AddCurveParameter("vLines", "VL", "vLines", GH_ParamAccess.list);
            pManager.AddCurveParameter("uLines", "UL", "uLines", GH_ParamAccess.list);
            pManager.AddPointParameter("cellPoints", "CP", "Cell points", GH_ParamAccess.tree);
            // Sometimes you want to hide a specific parameter from the Rhino preview.
            // You can use the HideParameter() method as a quick way:
            //pManager.HideParameter(0);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // First, we need to retrieve all data from the input parameters.
            // We'll start by declaring variables and assigning them starting values.
            GH_Structure<GH_Point> pointTree = new GH_Structure<GH_Point>();
            List<Point3d> repPoints = new List<Point3d>();
            double charge = 0.0;
            double decay = 0.0;
            double threshold = 0.0;
            int crvDegree = 0;

            // Then we need to access the input parameters individually. 
            // When data cannot be extracted from a parameter, we should abort this method.
            if (!DA.GetDataTree(0, out pointTree)) return;
            if (!DA.GetDataList(1, repPoints)) return;
            if (!DA.GetData(2, ref charge)) return;
            if (!DA.GetData(3, ref decay)) return;
            if (!DA.GetData(4, ref threshold)) return;
            if (!DA.GetData(5, ref crvDegree)) return;

            // We should now validate the data and warn the user if invalid data is supplied.
            if (charge < 0.0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Charge must be bigger than or equal to zero");
                return;
            }
            if (decay < 0.0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Charge must be bigger than or equal to zero");
                return;
            }
            if (threshold < 0.0 || threshold > 1.0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Threshold should be a value between 0.0 and 1.0");
                return;
            }

            List<Vector3d> vectors = new List<Vector3d>();
            DataTree<Point3d> cells = new DataTree<Point3d>();

            GH_Field field = new GH_Field();

            for (int i = 0; i < repPoints.Count; i++)
            {
                GH_PointCharge pc = new GH_PointCharge();
                pc.Charge = charge;
                pc.Decay = decay;
                pc.Location = repPoints[i];
                field.Elements.Add(pc);
            }



            for (int i = 0; i < pointTree.BranchCount; i++)
            {
                List<Point3d> branchPoints = pointTree.Branch(i);

                for (int j = 0; j < branchPoints.Count; j++)
                {
                    Vector3d vectorAt = field.TensorAt(branchPoints[j]);

                    if (vectorAt.Length > threshold)
                    {
                        vectorAt.Unitize();
                        vectorAt *= threshold;
                    }

                    vectors.Add(vectorAt);

                    Point3d movedPoint = branchPoints[j];
                    Transform move = Transform.Translation(vectorAt);
                    movedPoint.Transform(move);

                    pointTree.Branch(i)[j] = movedPoint;
                }
            }

            List<NurbsCurve> vCrv = new List<NurbsCurve>();
            List<NurbsCurve> uCrv = new List<NurbsCurve>();

            int count = 0;

            for (int i = 0; i < pointTree.BranchCount - 1; i++)
            {
                List<Point3d> branchPoints1 = pointTree.Branch(i);
                List<Point3d> branchPoints2 = pointTree.Branch(i + 1);



                for (int j = 0; j < branchPoints1.Count - 1; j++)
                {
                    GH_Path path = new GH_Path(count);

                    cells.Add(branchPoints1[j], path);
                    cells.Add(branchPoints1[j + 1], path);
                    cells.Add(branchPoints2[j + 1], path);
                    cells.Add(branchPoints2[j], path);

                    count++;
                }
            }


            for (int i = 0; i < pointTree.BranchCount; i++)
            {
                List<Point3d> branchPoints = pointTree.Branch(i);

                for (int j = 0; j < branchPoints.Count; j++)
                {
                    NurbsCurve crv1 = NurbsCurve.Create(false, 3, branchPoints);
                    vCrv.Add(crv1);
                }
            }

            for (int j = 0; j < pointTree.Branch(0).Count; j++)
            {
                List<Point3d> ctrlPoints = new List<Point3d>();

                for (int i = 0; i < pointTree.Branches.Count; i++)
                {
                    ctrlPoints.Add(pointTree.Branch(i)[j]);
                }

                NurbsCurve crv2 = NurbsCurve.Create(false, crvDegree, ctrlPoints);
                uCrv.Add(crv2);
            }

            //Output parameter
            DA.SetDataTree(0, pointTree);
            DA.SetDataList(1, vCrv);
            DA.SetDataList(2, uCrv);
            DA.SetDataTree(0, cells);
        }

        Curve CreateSpiral(Plane plane, double r0, double r1, Int32 turns)
        {
            Line l0 = new Line(plane.Origin + r0 * plane.XAxis, plane.Origin + r1 * plane.XAxis);
            Line l1 = new Line(plane.Origin - r0 * plane.XAxis, plane.Origin - r1 * plane.XAxis);

            Point3d[] p0;
            Point3d[] p1;

            l0.ToNurbsCurve().DivideByCount(turns, true, out p0);
            l1.ToNurbsCurve().DivideByCount(turns, true, out p1);

            PolyCurve spiral = new PolyCurve();

            for (int i = 0; i < p0.Length - 1; i++)
            {
                Arc arc0 = new Arc(p0[i], plane.YAxis, p1[i + 1]);
                Arc arc1 = new Arc(p1[i + 1], -plane.YAxis, p0[i + 1]);

                spiral.Append(arc0);
                spiral.Append(arc1);
            }

            return spiral;
        }

        /// <summary>
        /// The Exposure property controls where in the panel a component icon 
        /// will appear. There are seven possible locations (primary to septenary), 
        /// each of which can be combined with the GH_Exposure.obscure flag, which 
        /// ensures the component will only be visible on panel dropdowns.
        /// </summary>
        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// You can add image files to your project resources and access them like this:
        /// return Resources.IconForThisComponent;
        /// </summary>
        protected override System.Drawing.Bitmap Icon => null;

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("B035CCC1-9761-4071-A18F-01C75A018EB3");
    }
}