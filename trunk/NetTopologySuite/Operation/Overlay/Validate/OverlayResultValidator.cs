using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Operation.Overlay.Snap;

namespace NetTopologySuite.Operation.Overlay.Validate
{
    ///<summary>
    /// Validates that the result of an overlay operation is geometrically correct within a given tolerance.
    /// Uses fuzzy point location to find points which are.
    ///</summary>
    /// <remarks>
    /// This algorithm is only useful where the inputs are polygonal.
    /// This is a heuristic test, and may return false positive results
    /// (I.e. it may fail to detect an invalid result.)
    /// It should never return a false negative result, however
    /// (I.e. it should never report a valid result as invalid.)
    /// </remarks>
    /// <author>Martin Davis</author>
    /// <see cref="OverlayOp"/>
    public class OverlayResultValidator
    {
        public static bool IsValid(IGeometry a, IGeometry b, SpatialFunction overlayOp, IGeometry result)
        {
            OverlayResultValidator validator = new OverlayResultValidator(a, b, result);
            return validator.IsValid(overlayOp);
        }

        private static double ComputeBoundaryDistanceTolerance(IGeometry g0, IGeometry g1)
        {
            return Math.Min(GeometrySnapper.ComputeSizeBasedSnapTolerance(g0),
                    GeometrySnapper.ComputeSizeBasedSnapTolerance(g1));
        }

        private const double Tolerance = 0.000001;

        private readonly IGeometry[] _geom;
        private readonly FuzzyPointLocator[] _locFinder;
        private readonly Locations[] _location = new Locations[3];
        private readonly double _boundaryDistanceTolerance = Tolerance;
        private readonly List<ICoordinate> _testCoords = new List<ICoordinate>();

        private ICoordinate _invalidLocation;


        public OverlayResultValidator(IGeometry a, IGeometry b, IGeometry result)
        {
            /**
             * The tolerance to use needs to depend on the size of the geometries.
             * It should not be more precise than double-precision can support. 
             */
            _boundaryDistanceTolerance = ComputeBoundaryDistanceTolerance(a, b);
            _geom = new[] {a, b, result};
            _locFinder = new[]
                             {
                                 new FuzzyPointLocator(_geom[0], _boundaryDistanceTolerance),
                                 new FuzzyPointLocator(_geom[1], _boundaryDistanceTolerance),
                                 new FuzzyPointLocator(_geom[2], _boundaryDistanceTolerance)
                             };
        }

        public bool IsValid(SpatialFunction overlayOp)
        {
            AddTestPts(_geom[0]);
            AddTestPts(_geom[1]);
            bool isValid = CheckValid(overlayOp);

            /*
            System.out.println("OverlayResultValidator: " + isValid);
            System.out.println("G0");
            System.out.println(geom[0]);
            System.out.println("G1");
            System.out.println(geom[1]);
            System.out.println("Result");
            System.out.println(geom[2]);
            */

            return isValid;
        }

        public ICoordinate InvalidLocation
        {
            get { return _invalidLocation; }
        }

        private void AddTestPts(IGeometry g)
        {
            OffsetPointGenerator ptGen = new OffsetPointGenerator(g, 5 * _boundaryDistanceTolerance);
            _testCoords.AddRange(ptGen.GetPoints());
        }

        private bool CheckValid(SpatialFunction overlayOp)
        {
            for (int i = 0; i < _testCoords.Count; i++)
            {
                ICoordinate pt = _testCoords[i];
                if (!CheckValid(overlayOp, pt))
                {
                    _invalidLocation = pt;
                    return false;
                }
            }
            return true;
        }

        private bool CheckValid(SpatialFunction overlayOp, ICoordinate pt)
        {
            _location[0] = _locFinder[0].GetLocation(pt);
            _location[1] = _locFinder[1].GetLocation(pt);
            _location[2] = _locFinder[2].GetLocation(pt);

            /**
             * If any location is on the Boundary, can't deduce anything, so just return true
             */
            if (HasLocation(_location, Locations.Boundary))
                return true;

            return IsValidResult(overlayOp, _location);
        }

        private static bool HasLocation(Locations[] location, Locations loc)
        {
            for (int i = 0; i < 3; i++)
            {
                if (location[i] == loc)
                    return true;
            }
            return false;
        }

        private static bool IsValidResult(SpatialFunction overlayOp, Locations[] location)
        {
            bool expectedInterior = OverlayOp.IsResultOfOp(location[0], location[1], overlayOp);

            bool resultInInterior = (location[2] == Locations.Interior);
            // MD use simpler: boolean isValid = (expectedInterior == resultInInterior);
            bool isValid = !(expectedInterior ^ resultInInterior);

            if (!isValid) ReportResult(overlayOp, location, expectedInterior);

            return isValid;
        }

        private static void ReportResult(SpatialFunction overlayOp, Locations[] location, bool expectedInterior)
        {
            Console.WriteLine(overlayOp + ":"
                    + " A:" + Location.ToLocationSymbol(location[0])
                    + " B:" + Location.ToLocationSymbol(location[1])
                    + " expected:" + (expectedInterior ? 'i' : 'e')
                    + " actual:" + Location.ToLocationSymbol(location[2])
                    );
        }
    }
}