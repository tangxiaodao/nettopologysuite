using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GeoAPI.Utilities;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries.Utilities;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using GisSharpBlog.NetTopologySuite.Noding;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Buffer
{
    /// <summary>
    /// Creates all the raw offset curves for a buffer of a <see cref="Geometry{TCoordinate}"/>.
    /// Raw curves need to be noded together and polygonized to form the final buffer area.
    /// </summary>
    public class OffsetCurveSetBuilder<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        private readonly IGeometry<TCoordinate> _inputGeometry;
        private readonly Double _distance;
        private readonly OffsetCurveBuilder<TCoordinate> _curveBuilder;

        private readonly List<SegmentString<TCoordinate>> _curveList 
            = new List<SegmentString<TCoordinate>>();

        public OffsetCurveSetBuilder(IGeometry<TCoordinate> inputGeom, Double distance, OffsetCurveBuilder<TCoordinate> curveBuilder)
        {
            _inputGeometry = inputGeom;
            _distance = distance;
            _curveBuilder = curveBuilder;
        }

        /// <summary>
        /// Computes the set of raw offset curves for the buffer.
        /// Each offset curve has an attached {Label} indicating
        /// its left and right location.
        /// </summary>
        /// <returns>
        /// A set of <see cref="SegmentString{TCoordinate}"/>s 
        /// representing the raw buffer curves.
        /// </returns>
        public IEnumerable<SegmentString<TCoordinate>> GetCurves()
        {
            add(_inputGeometry);
            return _curveList;
        }

        private void addCurves(IEnumerable<IEnumerable<TCoordinate>> lineList, Locations leftLoc, Locations rightLoc)
        {
            foreach (IEnumerable<TCoordinate> line in lineList)
            {
                addCurve(line, leftLoc, rightLoc);
            }
        }

        /// <summary>
        /// Creates a <see cref="SegmentString{TCoordinate}"/> for a coordinate list
        /// which is a raw offset curve, and adds it to the list of buffer curves.
        /// </summary>
        /// <remarks>
        /// The SegmentString is tagged with a Label giving the topology of the curve.
        /// The curve may be oriented in either direction.
        /// If the curve is oriented CW, the locations will be:
        /// Left: Locations.Exterior.
        /// Right: Locations.Interior.
        /// </remarks>
        private void addCurve(IEnumerable<TCoordinate> coord, Locations leftLoc, Locations rightLoc)
        {
            // don't add null curves!
            if (!Slice.CountGreaterThan(2, coord))
            {
                return;
            }

            // add the edge for a coordinate list which is a raw offset curve
            SegmentString<TCoordinate> e = new SegmentString<TCoordinate>(coord, new Label(0, Locations.Boundary, leftLoc, rightLoc));
            _curveList.Add(e);
        }

        private void add(IGeometry g)
        {
            if (g.IsEmpty)
            {
                return;
            }

            if (g is IPolygon<TCoordinate>)
            {
                addPolygon(g as IPolygon<TCoordinate>);
            }
                // LineString also handles LinearRings
            else if (g is ILineString<TCoordinate>)
            {
                addLineString(g as ILineString<TCoordinate>);
            }
            else if (g is IPoint<TCoordinate>)
            {
                addPoint(g as IPoint<TCoordinate>);
            }
            else if (g is IMultiPoint<TCoordinate>)
            {
                addCollection(g as IMultiPoint<TCoordinate>);
            }
            else if (g is IMultiLineString<TCoordinate>)
            {
                addCollection(g as IMultiLineString<TCoordinate>);
            }
            else if (g is IMultiPolygon<TCoordinate>)
            {
                addCollection(g as IMultiPolygon<TCoordinate>);
            }
            else if (g is IGeometryCollection<TCoordinate>)
            {
                addCollection(g as IGeometryCollection<TCoordinate>);
            }
            else
            {
                throw new NotSupportedException(g.GetType().FullName);
            }
        }

        private void addCollection(IGeometryCollection<TCoordinate> gc)
        {
            foreach (IGeometry<TCoordinate> geometry in gc)
            {
                add(geometry);
            }
        }

        /// <summary>
        /// Add a Point to the graph.
        /// </summary>
        private void addPoint(IPoint<TCoordinate> p)
        {
            if (_distance <= 0.0)
            {
                return;
            }

            IEnumerable<TCoordinate> coord = p.Coordinates;
            IEnumerable<TCoordinate> lineList = _curveBuilder.GetLineCurve(coord, _distance);
            addCurves(lineList, Locations.Exterior, Locations.Interior);
        }

        private void addLineString(ILineString<TCoordinate> line)
        {
            if (_distance <= 0.0)
            {
                return;
            }

            IEnumerable<TCoordinate> coord = CoordinateHelper.RemoveRepeatedPoints(line.Coordinates);
            IEnumerable<TCoordinate> lineList = _curveBuilder.GetLineCurve(coord, _distance);
            addCurves(lineList, Locations.Exterior, Locations.Interior);
        }

        private void addPolygon(IPolygon<TCoordinate> p)
        {
            Double offsetDistance = _distance;
            Positions offsetSide = Positions.Left;
            if (_distance < 0.0)
            {
                offsetDistance = -_distance;
                offsetSide = Positions.Right;
            }

            ILinearRing<TCoordinate> shell = p.Shell;
            IEnumerable<TCoordinate> shellCoord = CoordinateHelper.RemoveRepeatedPoints(shell.Coordinates);
            
            // optimization - don't bother computing buffer
            // if the polygon would be completely eroded
            if (_distance < 0.0 && isErodedCompletely(shellCoord, _distance))
            {
                return;
            }

            addPolygonRing(shellCoord, offsetDistance, offsetSide,
                           Locations.Exterior, Locations.Interior);

            for (Int32 i = 0; i < p.InteriorRingsCount; i++)
            {
                ILinearRing hole = (ILinearRing) p.InteriorRings[i];
                ICoordinate[] holeCoord = CoordinateArrays.RemoveRepeatedPoints(hole.Coordinates);

                // optimization - don't bother computing buffer for this hole
                // if the hole would be completely covered
                if (_distance > 0.0 && isErodedCompletely(holeCoord, -_distance))
                {
                    continue;
                }

                // Holes are topologically labeled opposite to the shell, since
                // the interior of the polygon lies on their opposite side
                // (on the left, if the hole is oriented CCW)
                addPolygonRing(holeCoord, offsetDistance, Position.Opposite(offsetSide),
                               Locations.Interior, Locations.Exterior);
            }
        }

        /// <summary>
        /// Add an offset curve for a ring.
        /// The side and left and right topological location arguments
        /// assume that the ring is oriented CW.
        /// If the ring is in the opposite orientation,
        /// the left and right locations must be interchanged and the side flipped.
        /// </summary>
        /// <param name="coord">The coordinates of the ring (must not contain repeated points).</param>
        /// <param name="offsetDistance">The distance at which to create the buffer.</param>
        /// <param name="side">The side of the ring on which to construct the buffer line.</param>
        /// <param name="cwLeftLoc">The location on the L side of the ring (if it is CW).</param>
        /// <param name="cwRightLoc">The location on the R side of the ring (if it is CW).</param>
        private void addPolygonRing(IEnumerable<TCoordinate> coord, Double offsetDistance,
                                    Positions side, Locations cwLeftLoc, Locations cwRightLoc)
        {
            Locations leftLoc = cwLeftLoc;
            Locations rightLoc = cwRightLoc;

            if (CGAlgorithms<TCoordinate>.IsCCW(coord))
            {
                leftLoc = cwRightLoc;
                rightLoc = cwLeftLoc;
                side = Position.Opposite(side);
            }

            IEnumerable<TCoordinate> lineList = _curveBuilder.GetRingCurve(coord, side, offsetDistance);
            addCurves(lineList, leftLoc, rightLoc);
        }

        /// <summary>
        /// The ringCoord is assumed to contain no repeated points.
        /// It may be degenerate (i.e. contain only 1, 2, or 3 points).
        /// In this case it has no area, and hence has a minimum diameter of 0.
        /// </summary>
        private Boolean isErodedCompletely(IEnumerable<TCoordinate> ringCoord, Double bufferDistance)
        {
            Double minDiam = 0.0;

            // degenerate ring has no area
            if (ringCoord.Length < 4)
            {
                return bufferDistance < 0;
            }

            // important test to eliminate inverted triangle bug
            // also optimizes erosion test for triangles
            if (ringCoord.Length == 4)
            {
                return isTriangleErodedCompletely(ringCoord, bufferDistance);
            }

            /*
             * The following is a heuristic test to determine whether an
             * inside buffer will be eroded completely.
             * It is based on the fact that the minimum diameter of the ring pointset
             * provides an upper bound on the buffer distance which would erode the
             * ring.
             * If the buffer distance is less than the minimum diameter, the ring
             * may still be eroded, but this will be determined by
             * a full topological computation.
             *
             */
            ILinearRing ring = _inputGeometry.Factory.CreateLinearRing(ringCoord);
            MinimumDiameter md = new MinimumDiameter(ring);
            minDiam = md.Length;
            return minDiam < 2*Math.Abs(bufferDistance);
        }

        /// <summary>
        /// Tests whether a triangular ring would be eroded completely by the given
        /// buffer distance.
        /// This is a precise test.  It uses the fact that the inner buffer of a
        /// triangle converges on the inCentre of the triangle (the point
        /// equidistant from all sides).  If the buffer distance is greater than the
        /// distance of the inCentre from a side, the triangle will be eroded completely.
        /// This test is important, since it removes a problematic case where
        /// the buffer distance is slightly larger than the inCentre distance.
        /// In this case the triangle buffer curve "inverts" with incorrect topology,
        /// producing an incorrect hole in the buffer.       
        /// </summary>
        private Boolean isTriangleErodedCompletely(IEnumerable<TCoordinate> triangleCoord, Double bufferDistance)
        {
            Triple<TCoordinate> points = Slice.GetTriple(triangleCoord);
            Triangle<TCoordinate> tri = new Triangle<TCoordinate>(points.First, points.Second, points.Third);
            TCoordinate inCenter = tri.InCenter;
            Double distToCentre = CGAlgorithms<TCoordinate>.DistancePointLine(inCenter, tri.P0, tri.P1);
            return distToCentre < Math.Abs(bufferDistance);
        }
    }
}