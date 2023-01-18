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
        Circle circle;
        public double multiplicator;


        public Cavity(Circle _circle, double _multiplicator, bool _multiplyDepth, CavityInfo _cInfo, double _entry)
        {
            lofts = new List<Brep>();
            circle = _circle;
            multiplicator = _multiplicator;
            CavitySetup(_cInfo.ToolWidth, _cInfo.ToolLength, _cInfo.Depths, multiplicator, _multiplyDepth, _entry);
        }

        void CavitySetup(double _toolWidth, double _toolLength, Interval _depths, double _multiplicator, bool _multiplyDepth, double _entry)
        {
            double bDepth;

            if (_multiplyDepth) bDepth = Utility.ReMap(_multiplicator, 0.1, 1.0, _depths.T0, _depths.T1);
            else bDepth = Utility.ReMap(_multiplicator, 0.1, 1.0, _depths.T1, _depths.T0);

            double entrySize = circle.Radius / 2;
            double entryDepth = bDepth * _entry;
            if (entryDepth > bDepth - 0.1) entryDepth = bDepth - 0.1; 

            List<Circle> circles = CreateCircles(bDepth, entrySize, entryDepth);

            List<Curve> circlesAsCrv = new List<Curve>();
            for (int i = 0; i < circles.Count; i++)
            {
                circlesAsCrv.Add(circles[i].ToNurbsCurve());
            }

            Brep[] tempBreps = Brep.CreateFromLoft(circlesAsCrv, Point3d.Unset, Point3d.Unset, LoftType.Straight, false);


            Circle bCircle = circles[circles.Count - 1];

            double coneRadius = bCircle.Radius - _toolWidth;

            double angle = Math.Atan((bDepth - entryDepth) / ((bCircle.Diameter - entrySize) / 3));
            double coneHeight = Math.Tan(angle) * coneRadius;

            Circle coneCircle = new Circle(bCircle.Plane, coneRadius);
            Curve coneCurve = coneCircle.ToNurbsCurve();

            List<Curve> bottomLoft = new List<Curve>();
            bottomLoft.Add(coneCurve);
            bottomLoft.Add(circlesAsCrv[circlesAsCrv.Count - 1]);
            Brep[] secondTemp = Brep.CreateFromLoft(bottomLoft, Point3d.Unset, Point3d.Unset, LoftType.Straight, false);

            for (int i = 0; i < tempBreps.Length; i++)
            {
                lofts.Add(tempBreps[i]);
            }

            for (int i = 0; i < secondTemp.Length; i++)
            {
                lofts.Add(secondTemp[i]);
            }


            Cone tempCone = CreateCone(coneRadius, coneHeight, bCircle);

            Point3d conePoint = tempCone.BasePoint;
            Point3d circlePoint = circles[circles.Count - 1].Center;

            Vector3d mVector = circlePoint - conePoint;
            Brep cone = Brep.CreateFromCone(tempCone, false);
            cone.Translate(mVector);

            lofts.Add(cone);

            Brep[] joinedBreps = Brep.JoinBreps(lofts, 0.1);

            lofts.Clear();

            for (int i = 0; i < joinedBreps.Length; i++)
            {
                lofts.Add(joinedBreps[i]);
            }
        }

        Cone CreateCone(double _coneRadius, double _coneHeight, Circle _bCircle)
        {
            Point3d tipPoint = _bCircle.Center;
            Vector3d movePoint = _bCircle.Normal;
            movePoint.Unitize();
            movePoint.Reverse();
            movePoint *= _coneHeight + 1.0;
            Transform move = Transform.Translation(movePoint);
            tipPoint.Transform(move);

            Plane smallTipPlane = new Plane(tipPoint, _bCircle.Normal);
            Cone smallShape = new Cone(smallTipPlane, _coneHeight, _coneRadius);

            return smallShape;
        }

        List<Circle> CreateCircles(double _bDepth, double _entrySize, double _entryDepth)
        {
            Circle upperCirle = circle;

            Circle entryCircle = new Circle(circle.Plane, _entrySize);
            Vector3d moveVector = circle.Normal;
            moveVector.Unitize();
            moveVector *= _entryDepth;
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

        double BottomDepth(double _angle, double _sLength, double _maxDepth, double _minDepth)
        {
            double angleA1 = _angle;
            double angleA1radians = (Math.PI / 180) * angleA1;

            double sideA = Math.Sin(angleA1radians) * _sLength;

            if ((sideA < _maxDepth) && (sideA > _minDepth))
            {
                return sideA;
            }

            else if (sideA < _minDepth)
            {
                return _minDepth;
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
