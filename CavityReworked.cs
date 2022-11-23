using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Geometry;

namespace DiggerBee
{
    class CavityReworked
    {
        public List<Brep> lofts;
        public Brep cone;

        Point3d point;
        Circle circle; 

        public double Width;
        public double eDim;
        public List<Circle> circles;
        public double angle;



        public CavityReworked(Circle _circle, double _multiplicator, CavityInfo _cInfo)
        {
            lofts = new List<Brep>();
            circle = _circle;
            point = _circle.Center;
            double angle = Utility.ReMap(_multiplicator, 0.1, 1.0, _cInfo.Angles.Max, _cInfo.Angles.Min);
            double insertion = Utility.GetRandomNumber(_cInfo.Insertion.Min, _cInfo.Insertion.Max);
            double entrySize = Utility.ReMap(_multiplicator, 0.1, 1.0, _cInfo.ToolWidth/2, circle.Radius);
            CavitySetup(entrySize, _cInfo.ToolWidth, _cInfo.ToolLength, _cInfo.MaxDepth, insertion);
        }

        void CavitySetup(double _entrySize, double _toolWidth, double _toolLength, double _maxDepth, double _eDepth)
        {
            Circle upperCirle = circle; 

            double sLength = SideLength(angle, _toolLength);
            double bDepth = BottomDepth(angle, sLength, _maxDepth);

            Circle entryCircle = new Circle(circle.Plane, _entrySize);
            Vector3d moveVector = circle.Normal;
            moveVector.Unitize();
            moveVector *= (bDepth/2);
            Transform move = Transform.Translation(moveVector);
            entryCircle.Transform(move);

            Circle bottomCircle = new Circle(circle.Plane, circle.Radius);
            Vector3d moveVector2 = circle.Normal;
            moveVector2.Unitize();
            moveVector2 *= bDepth;
            Transform move2 = Transform.Translation(moveVector2);
            bottomCircle.Transform(move2);

            List<Circle> circles = new List<Circle>();
            circles.Add(upperCirle);
            circles.Add(entryCircle);
            circles.Add(bottomCircle);

            List<Curve> circlesAsCrv = new List<Curve>();
            for (int i = 0; i < circles.Count; i++)
            {
                circlesAsCrv.Add(circles[i].ToNurbsCurve());
            }

            List<Brep> tempLofts = CreateLoft(circlesAsCrv);


            //Cone smallShape = CreateCone(circles[circles.Count - 1], angle, _toolWidth);

            Point3d planePoint = entryCircle.Center;
            Vector3d moveVectorPlane = circle.Normal;
            moveVectorPlane.Unitize();
            moveVectorPlane *= _toolWidth;
            Transform movePlanePoint = Transform.Translation(moveVectorPlane);
            planePoint.Transform(movePlanePoint);


            Cone smallShape = new Cone(new Plane(planePoint, entryCircle.Normal), (bDepth / 2) + _toolWidth, bottomCircle.Radius - _toolWidth);
            cone = Brep.CreateFromCone(smallShape, true);

            //if ((cone != null) && (tempLofts.Count > 0))
            //{
                //Brep[] difference = Brep.CreateBooleanDifference(tempLofts[0], cone, 0.1);

               // for (int i = 0; i < difference.Length; i++)
                //{
                 //   lofts.Add(difference[i]);
                //}
            // }

            //else
            //{
                lofts = tempLofts;
            //}

        }

        Cone CreateCone(Circle _bCircle, double _angle, double _toolWidth)
        {
            double coneRadius = _bCircle.Radius - _toolWidth;
            double angleRadians = (Math.PI / 180) * _angle;

            double tipHeight = coneRadius * Math.Tan(angleRadians);
            
            Point3d tipPoint = _bCircle.Center;
            Vector3d movePoint = _bCircle.Normal;
            movePoint.Unitize();
            movePoint *= tipHeight;
            Transform move3 = Transform.Translation(movePoint);
            tipPoint.Transform(move3);

            Plane smallTipPlane = new Plane(tipPoint, _bCircle.Normal);
            Cone smallShape = new Cone(smallTipPlane, tipHeight, coneRadius);
            return smallShape;
        }

        List<Circle> CreateCircles(double _eDepth, double _bDepth, double _eDim, double _bDim, double _angle)
        {
            List<Circle> c = new List<Circle>();

            Point3d ePoint = new Point3d(point.X, point.Y, point.Z - _eDepth);
            Circle eCircle = new Circle(ePoint, _eDim / 2);

            Point3d bPoint = new Point3d(point.X, point.Y, point.Z - _bDepth);
            Circle bCircle = new Circle(bPoint, _bDim / 2);

            if (_eDepth > 0)
            {
                double tDepth = 0.0 - ePoint.Z;
                double tDim = BottomDim(_angle, tDepth, _eDim);
                Point3d tPoint = new Point3d(point.X, point.Y, point.Z);
                Circle tCircle = new Circle(tPoint, tDim / 2);
                c.Add(tCircle);
            }

            c.Add(eCircle);
            c.Add(bCircle);

            return c;
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
            return Width;
        }
    }
}
