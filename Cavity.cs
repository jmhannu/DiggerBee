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
        private Circle circle;

        public Cavity(Circle _circle, double _multiplicator, bool _multiplyDepth, CavityInfo _cInfo, double _entry)
        {
            lofts = new List<Brep>();
            _circle.Reverse();
            circle = _circle; 

            Interval depths = _cInfo.Depths;
            double bDepth;

            if (_multiplyDepth) bDepth = Utility.ReMap(_multiplicator, 0.1, 1.0, depths.T0, depths.T1);
            else bDepth = Utility.ReMap(_multiplicator, 0.1, 1.0, depths.T1, depths.T0);

            CavitySetup(_cInfo.ToolWidth, _cInfo.ToolLength, bDepth, _entry);
        }

        void CavitySetup(double _toolWidth, double _toolLength, double _bDepth, double _entry)
        {
            double entrySize = circle.Radius / 2;
            double entryDepth = _bDepth * _entry;
            if (entryDepth > _bDepth - 0.1) entryDepth = _bDepth - 0.1; 

            List<Circle> circles = CreateCircles(_bDepth, entrySize, entryDepth);

            List<Curve> circlesAsCrv = CirclesToCurve(circles);

            Brep[] loftedCircles = Brep.CreateFromLoft(circlesAsCrv, Point3d.Unset, Point3d.Unset, LoftType.Straight, false);
            AddToLofts(loftedCircles);

            Circle bCircle = circles[circles.Count - 1];

            double coneRadius = bCircle.Radius - _toolWidth;
            double coneAngle = Math.Atan((_bDepth - entryDepth) / ((bCircle.Diameter - entrySize) / 3));
            double coneHeight = Math.Tan(coneAngle) * coneRadius;

            Brep[] loftedBottom = BottomLoft(bCircle, coneRadius, circlesAsCrv[circlesAsCrv.Count - 1]);
            AddToLofts(loftedBottom);

            Cone cone = CreateCone(coneRadius, coneHeight, bCircle);
            Brep coneBrep = BrepCone(cone, circles[circles.Count - 1].Center);

            if(coneHeight > _bDepth)
            {
                CutCone(bCircle.Diameter, coneBrep);
            }

            else
            {
            lofts.Add(coneBrep);
            }

            Brep[] joinedBreps = Brep.JoinBreps(lofts, 0.1);

            lofts.Clear();

            AddToLofts(joinedBreps);
        }

        List<Circle> CreateCircles(double _bDepth, double _entrySize, double _entryDepth)
        {

            List<Circle> circles = new List<Circle>();
            circles.Add(circle);

            if (_entryDepth > 0.05)
            {
                Circle entryCircle = new Circle(circle.Plane, _entrySize);
                Vector3d moveVector = circle.Normal;
                moveVector.Unitize();
                moveVector *= _entryDepth;
                Transform move = Transform.Translation(moveVector);
                entryCircle.Transform(move);

                circles.Add(entryCircle);
            }

            Circle bottomCircle = new Circle(circle.Plane, circle.Radius);
            Vector3d moveVector2 = circle.Normal;
            moveVector2.Unitize();
            moveVector2 *= _bDepth;
            Transform move2 = Transform.Translation(moveVector2);
            bottomCircle.Transform(move2);
            circles.Add(bottomCircle);

            return circles;
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

        Brep BrepCone(Cone _cone, Point3d _circlePoint)
        {
            Point3d conePoint = _cone.BasePoint;
            Vector3d mVector = _circlePoint - conePoint;
            Brep breped = Brep.CreateFromCone(_cone, false);
            breped.Translate(mVector);

            return breped;
        }

        List<Curve> CirclesToCurve(List<Circle> _circles)
        {
            List<Curve> curves = new List<Curve>();
            for (int i = 0; i < _circles.Count; i++)
            {
                curves.Add(_circles[i].ToNurbsCurve());
            }

            return curves; 
        }

        Brep[] BottomLoft(Circle _bCircle, double _coneRadius, Curve _bottomCurve)
        {
            Circle coneCircle = new Circle(_bCircle.Plane, _coneRadius);
            Curve coneCurve = coneCircle.ToNurbsCurve();

            List<Curve> bottomLoft = new List<Curve>();
            bottomLoft.Add(coneCurve);
            bottomLoft.Add(_bottomCurve);
            return Brep.CreateFromLoft(bottomLoft, Point3d.Unset, Point3d.Unset, LoftType.Straight, false);
        }

        void AddToLofts(Brep[] _brepArray)
        {
            for (int i = 0; i < _brepArray.Length; i++)
            {
                lofts.Add(_brepArray[i]);
            }
        }

        void CutCone(double _bottomDiameter, Brep _coneAsBrep)
        {
            Vector3d cutVector = circle.Normal;
            cutVector.Reverse();
            Plane cutPlane = new Plane(circle.Center, cutVector);

            double boxSize = _bottomDiameter;
            Interval boxInterval = new Interval(boxSize * -1, boxSize);
            Box cutBox = new Box(cutPlane, boxInterval, boxInterval, new Interval(0, boxSize));
            Brep[] cutCone = Brep.CreateBooleanDifference(_coneAsBrep, cutBox.ToBrep(), 0.1);
            AddToLofts(cutCone);
        }

       /* public double GetWidth()
        {
            return circle.Diameter;
        }*/
    }
}
