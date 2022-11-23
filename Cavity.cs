using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Geometry;

namespace DiggerBee
{
    class Cavity
    {
        public List<Brep> lofts;
        public Brep cone;

        Circle circle; 

        public double angle;

        public Cavity(Circle _circle, double _multiplicator, CavityInfo _cInfo)
        {
            lofts = new List<Brep>();
            circle = _circle;
            angle = Utility.ReMap(_multiplicator, 0.1, 1.0, _cInfo.Angles.Max, _cInfo.Angles.Min);
            double entrySize = Utility.ReMap(_multiplicator, 0.1, 1.0, _cInfo.ToolWidth/2, circle.Radius);
            CavitySetup(entrySize, _cInfo.ToolWidth, _cInfo.ToolLength, _cInfo.MaxDepth);
        }

        void CavitySetup(double _entrySize, double _toolWidth, double _toolLength, double _maxDepth)
        {
            double sLength = SideLength(angle, _toolLength);
            double bDepth = BottomDepth(angle, sLength, _maxDepth);

            List<Circle> circles = CreateCircles(bDepth, _entrySize);

            List<Curve> circlesAsCrv = new List<Curve>();
            for (int i = 0; i < circles.Count; i++)
            {
                circlesAsCrv.Add(circles[i].ToNurbsCurve());
            }

            List<Brep> tempLofts = CreateLoft(circlesAsCrv);

            Circle bCircle = circles[circles.Count - 1];
            Cone smallShape = CreateCone(bCircle.Radius - _toolWidth, (bDepth/2) - _toolWidth, bCircle);
            
            cone = Brep.CreateFromCone(smallShape, true);

            if ((cone != null) && (tempLofts.Count > 0))
            {
                Brep[] difference = Brep.CreateBooleanDifference(tempLofts[0], cone, 0.1);

                for (int i = 0; i < difference.Length; i++)
                {
                   lofts.Add(difference[i]);
                }
             }

            else
            {
                lofts = tempLofts;
            }

        }

        Cone CreateCone(double _coneRadius, double _coneHeight, Circle _bCircle)
        {            
            Point3d tipPoint = _bCircle.Center;
            Vector3d movePoint = _bCircle.Normal;
            movePoint.Unitize();
            movePoint.Reverse();
            movePoint *= _coneHeight;
            Transform move3 = Transform.Translation(movePoint);
            tipPoint.Transform(move3);

            Plane smallTipPlane = new Plane(tipPoint, _bCircle.Normal);
            Cone smallShape = new Cone(smallTipPlane, _coneHeight, _coneRadius);
            return smallShape;
        }

        List<Circle> CreateCircles(double _bDepth, double _entrySize)
        {
            Circle upperCirle = circle;

            Circle entryCircle = new Circle(circle.Plane, _entrySize);
            Vector3d moveVector = circle.Normal;
            moveVector.Unitize();
            moveVector *= (_bDepth / 2);
            Transform move = Transform.Translation(moveVector);
            entryCircle.Transform(move);

            Circle bottomCircle = new Circle(circle.Plane, circle.Radius);
            Vector3d moveVector2 = circle.Normal;
            moveVector2.Unitize();
            moveVector2 *= _bDepth;
            Transform move2 = Transform.Translation(moveVector2);
            bottomCircle.Transform(move2);

            List<Circle> circles = new List<Circle> { upperCirle, entryCircle, bottomCircle };
   
            return circles; 
        }


        List<Brep> CreateLoft(List<Curve> _circles)
        {
            List<Brep> breps = new List<Brep>();
            List<Brep> brepsCaped = new List<Brep>();

            var tempBreps = Brep.CreateFromLoft(_circles, Point3d.Unset, Point3d.Unset, LoftType.Straight, false);

            foreach (var brep in tempBreps)
            {
                breps.Add(brep);
            }

            for (int i = 0; i < breps.Count; i++)
            {
                breps[i].Faces.SplitKinkyFaces(Rhino.RhinoMath.DefaultAngleTolerance, true);
                brepsCaped.Add(breps[i].CapPlanarHoles(0.001));
            }

            if (brepsCaped.Count != 0) return brepsCaped;
            else return breps;
        }


        double SideLength(double _angle, double _toolLength)
        {

            double millHead = 50.0;
            double sideC2head = (millHead / 2) + 0.5;

            double angleA1 = _angle;
            double angleB1 = 90 - _angle;

            double angleB2 = 90 - angleB1;
            double angleA2 = 90 - angleB2;

            double angleA2radians = (Math.PI / 180) * angleA2;
            double sideA = Math.Sin(angleA2radians) * sideC2head;

            double angleA1radians = (Math.PI / 180) * angleA1;
            double sideC = sideA / Math.Sin(angleA1radians);

            double sLength = _toolLength - sideC;

            return sLength;
        }

        double BottomDepth(double _angle, double _sLength, double _maxDepth)
        {
            double angleA1 = _angle;
            double angleA1radians = (Math.PI / 180) * angleA1;

            double sideA = Math.Sin(angleA1radians) * _sLength;

            if (sideA < _maxDepth)
            {
                return sideA;
            }

            else
            {
                return _maxDepth;
            }
        }

        double BottomDim(double _angle, double _cDepth, double _eDim)
        {

            double _angleRadians = (Math.PI / 180) * _angle;
            double addDim = _cDepth / Math.Tan(_angleRadians);


            return addDim + addDim + _eDim;
        }

        public double GetWidth()
        {
            return circle.Diameter;
        }
    }
}
