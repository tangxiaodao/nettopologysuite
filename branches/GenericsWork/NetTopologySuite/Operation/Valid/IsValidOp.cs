using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using GisSharpBlog.NetTopologySuite.Utilities;
using Iesi_NTS.Collections;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Valid
{
    /// <summary>
    /// Implements the algorithsm required to compute the <see cref="Geometry{TCoordinate}.IsValid" />
    /// method for <see cref="Geometry{TCoordinate}" />s.
    /// See the documentation for the various geometry types for a specification of validity.
    /// </summary>
    public class IsValidOp<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        /// <summary>
        /// Checks whether a coordinate is valid for processing.
        /// Coordinates are valid iff their x and y ordinates are in the
        /// range of the floating point representation.
        /// </summary>
        /// <param name="coord">The coordinate to validate.</param>
        /// <returns><see langword="true"/> if the coordinate is valid.</returns>
        public static Boolean IsValidCoordinate(TCoordinate coord)
        {
            if (Double.IsNaN(coord[Ordinates.X]))
            {
                return false;
            }

            if (Double.IsInfinity(coord[Ordinates.X]))
            {
                return false;
            }

            if (Double.IsNaN(coord[Ordinates.Y]))
            {
                return false;
            }

            if (Double.IsInfinity(coord[Ordinates.Y]))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Find a point from the list of testCoords
        /// that is NOT a node in the edge for the list of searchCoords.
        /// </summary>
        /// <returns>The point found, or <see langword="null" /> if none found.</returns>
        public static TCoordinate FindPointNotNode(IEnumerable<TCoordinate> testCoords, ILinearRing<TCoordinate> searchRing, GeometryGraph<TCoordinate> graph)
        {
            // find edge corresponding to searchRing.
            Edge<TCoordinate> searchEdge = graph.FindEdge(searchRing);
            
            // find a point in the testCoords which is not a node of the searchRing
            EdgeIntersectionList<TCoordinate> eiList = searchEdge.EdgeIntersectionList;
            
            // somewhat inefficient - is there a better way? (Use a node map, for instance?)
            foreach (ICoordinate pt in testCoords)
            {
                if (!eiList.IsIntersection(pt))
                {
                    return pt;
                }
            }
            return null;
        }

        private IGeometry parentGeometry = null; // the base Geometry to be validated

        /**
         * If the following condition is TRUE JTS will validate inverted shells and exverted holes (the ESRI SDE model).
         */
        private Boolean isSelfTouchingRingFormingHoleValid = false;
        private Boolean isChecked = false;
        private TopologyValidationError validErr = null;

        public IsValidOp(IGeometry parentGeometry)
        {
            this.parentGeometry = parentGeometry;
        }

        /// <summary>
        /// <para>
        /// Gets or sets whether polygons using Self-Touching Rings to form
        /// holes are reported as valid.
        /// If this flag is set, the following Self-Touching conditions
        /// are treated as being valid:
        /// - The shell ring self-touches to create a hole touching the shell.
        /// - A hole ring self-touches to create two holes touching at a point.
        /// </para>
        /// <para>
        /// The default (following the OGC SFS standard)
        /// is that this condition is not valid (<c>false</c>).
        /// </para>
        /// <para>
        /// This does not affect whether Self-Touching Rings
        /// disconnecting the polygon interior are considered valid
        /// (these are considered to be invalid under the SFS, and many other
        /// spatial models as well).
        /// This includes "bow-tie" shells,
        /// which self-touch at a single point causing the interior to be disconnected,
        /// and "C-shaped" holes which self-touch at a single point causing an island to be formed.
        /// </para>
        /// </summary>
        /// <value>States whether geometry with this condition is valid.</value>
        public Boolean IsSelfTouchingRingFormingHoleValid
        {
            get { return isSelfTouchingRingFormingHoleValid; }
            set { isSelfTouchingRingFormingHoleValid = value; }
        }

        public Boolean IsValid
        {
            get
            {
                CheckValid(parentGeometry);
                return validErr == null;
            }
        }

        public TopologyValidationError ValidationError
        {
            get
            {
                CheckValid(parentGeometry);
                return validErr;
            }
        }

        private void CheckValid(IGeometry g)
        {
            if (isChecked)
            {
                return;
            }

            validErr = null;

            if (g.IsEmpty)
            {
                return;
            }

            if (g is IPoint)
            {
                checkValid((IPoint) g);
            }
            else if (g is IMultiPoint)
            {
                checkValid((IMultiPoint) g);
            }
            else if (g is ILinearRing) // LineString also handles LinearRings
            {
                CheckValid((ILinearRing) g);
            }
            else if (g is ILineString)
            {
                checkValid((ILineString) g);
            }
            else if (g is IPolygon)
            {
                CheckValid((IPolygon) g);
            }
            else if (g is IMultiPolygon)
            {
                CheckValid((IMultiPolygon) g);
            }
            else if (g is IGeometryCollection)
            {
                CheckValid((IGeometryCollection) g);
            }
            else
            {
                throw new NotSupportedException(g.GetType().FullName);
            }
        }

        /// <summary>
        /// Checks validity of a Point.
        /// </summary>
        private void checkValid(IPoint<TCoordinate> g)
        {
            checkInvalidCoordinates(g.Coordinates);
        }

        /// <summary>
        /// Checks validity of a MultiPoint.
        /// </summary>
        private void checkValid(IMultiPoint<TCoordinate> g)
        {
            checkInvalidCoordinates(g.Coordinates);
        }

        /// <summary>
        /// Checks validity of a LineString.  
        /// Almost anything goes for lineStrings!
        /// </summary>
        private void checkValid(ILineString<TCoordinate> g)
        {
            checkInvalidCoordinates(g.Coordinates);

            if (validErr != null)
            {
                return;
            }

            GeometryGraph graph = new GeometryGraph(0, g);
            checkTooFewPoints(graph);
        }

        /// <summary>
        /// Checks validity of a LinearRing.
        /// </summary>
        private void checkValid(ILinearRing<TCoordinate> g)
        {
            checkInvalidCoordinates(g.Coordinates);

            if (validErr != null)
            {
                return;
            }

            checkClosedRing(g);

            if (validErr != null)
            {
                return;
            }

            GeometryGraph<TCoordinate> graph = new GeometryGraph<TCoordinate>(0, g);
            checkTooFewPoints(graph);

            if (validErr != null)
            {
                return;
            }

            LineIntersector<TCoordinate> li = new RobustLineIntersector<TCoordinate>();
            graph.ComputeSelfNodes(li, true);
            checkNoSelfIntersectingRings(graph);
        }

        /// <summary>
        /// Checks the validity of a polygon and sets the validErr flag.
        /// </summary>
        private void CheckValid(IPolygon g)
        {
            checkInvalidCoordinates(g);
            if (validErr != null)
            {
                return;
            }
            checkClosedRings(g);
            if (validErr != null)
            {
                return;
            }

            GeometryGraph graph = new GeometryGraph(0, g);
            checkTooFewPoints(graph);
            if (validErr != null)
            {
                return;
            }
            checkConsistentArea(graph);
            if (validErr != null)
            {
                return;
            }
            if (!IsSelfTouchingRingFormingHoleValid)
            {
                checkNoSelfIntersectingRings(graph);
                if (validErr != null)
                {
                    return;
                }
            }
            CheckHolesInShell(g, graph);
            if (validErr != null)
            {
                return;
            }
            CheckHolesNotNested(g, graph);
            if (validErr != null)
            {
                return;
            }
            CheckConnectedInteriors(graph);
        }

        private void CheckValid(IMultiPolygon g)
        {
            foreach (IPolygon p in g.Geometries)
            {
                checkInvalidCoordinates(p);
                if (validErr != null)
                {
                    return;
                }
                checkClosedRings(p);
                if (validErr != null)
                {
                    return;
                }
            }

            GeometryGraph graph = new GeometryGraph(0, g);
            checkTooFewPoints(graph);
            if (validErr != null)
            {
                return;
            }
            checkConsistentArea(graph);
            if (validErr != null)
            {
                return;
            }
            if (!IsSelfTouchingRingFormingHoleValid)
            {
                checkNoSelfIntersectingRings(graph);
                if (validErr != null)
                {
                    return;
                }
            }
            foreach (IPolygon p in g.Geometries)
            {
                CheckHolesInShell(p, graph);
                if (validErr != null)
                {
                    return;
                }
            }
            foreach (IPolygon p in g.Geometries)
            {
                CheckHolesNotNested(p, graph);
                if (validErr != null)
                {
                    return;
                }
            }
            CheckShellsNotNested(g, graph);
            if (validErr != null)
            {
                return;
            }
            CheckConnectedInteriors(graph);
        }

        private void CheckValid(IGeometryCollection gc)
        {
            foreach (IGeometry g in gc.Geometries)
            {
                CheckValid(g);
                if (validErr != null)
                {
                    return;
                }
            }
        }

        private void checkInvalidCoordinates(IEnumerable<TCoordinate> coords)
        {
            foreach (TCoordinate c in coords)
            {
                if (!IsValidCoordinate(c))
                {
                    validErr = new TopologyValidationError(TopologyValidationErrors.InvalidCoordinate, c);
                    return;
                }
            }
        }

        private void checkInvalidCoordinates(IPolygon<TCoordinate> poly)
        {
            checkInvalidCoordinates(poly.ExteriorRing.Coordinates);

            if (validErr != null)
            {
                return;
            }

            foreach (ILineString<TCoordinate> ls in poly.InteriorRings)
            {
                checkInvalidCoordinates(ls.Coordinates);

                if (validErr != null)
                {
                    return;
                }
            }
        }

        private void checkClosedRings(IPolygon<TCoordinate> poly)
        {
            checkClosedRing(poly.Shell);

            if (validErr != null)
            {
                return;
            }

            foreach (ILinearRing hole in poly.Holes)
            {
                checkClosedRing(hole);

                if (validErr != null)
                {
                    return;
                }
            }
        }

        private void checkClosedRing(ILinearRing ring)
        {
            if (!ring.IsClosed)
            {
                validErr = new TopologyValidationError(TopologyValidationErrors.RingNotClosed,
                                                       ring.GetCoordinateN(0));
            }
        }

        private void checkTooFewPoints(GeometryGraph<TCoordinate> graph)
        {
            if (graph.HasTooFewPoints)
            {
                validErr = new TopologyValidationError(TopologyValidationErrors.TooFewPoints,
                                                       graph.InvalidPoint);
                return;
            }
        }

        private void checkConsistentArea(GeometryGraph<TCoordinate> graph)
        {
            ConsistentAreaTester cat = new ConsistentAreaTester(graph);
            Boolean isValidArea = cat.IsNodeConsistentArea;

            if (!isValidArea)
            {
                validErr = new TopologyValidationError(TopologyValidationErrors.SelfIntersection, cat.InvalidPoint);
                return;
            }

            if (cat.HasDuplicateRings)
            {
                validErr = new TopologyValidationError(TopologyValidationErrors.DuplicateRings, cat.InvalidPoint);
                return;
            }
        }

        /// <summary>
        /// Check that there is no ring which self-intersects (except of course at its endpoints).
        /// This is required by OGC topology rules (but not by other models
        /// such as ESRI SDE, which allow inverted shells and exverted holes).
        /// </summary>
        private void checkNoSelfIntersectingRings(GeometryGraph<TCoordinate> graph)
        {
            foreach (Edge<TCoordinate> e in graph.Edges)
            {
                checkNoSelfIntersectingRing(e.EdgeIntersectionList);

                if (validErr != null)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Check that a ring does not self-intersect, except at its endpoints.
        /// Algorithm is to count the number of times each node along edge occurs.
        /// If any occur more than once, that must be a self-intersection.
        /// </summary>
        private void checkNoSelfIntersectingRing(EdgeIntersectionList<TCoordinate> eiList)
        {
            ISet nodeSet = new ListSet();
            Boolean isFirst = true;

            foreach (EdgeIntersection ei in eiList)
            {
                if (isFirst)
                {
                    isFirst = false;
                    continue;
                }

                if (nodeSet.Contains(ei.Coordinate))
                {
                    validErr = new TopologyValidationError(TopologyValidationErrors.RingSelfIntersection, ei.Coordinate);
                    return;
                }
                else
                {
                    nodeSet.Add(ei.Coordinate);
                }
            }
        }

        /// <summary>
        /// Tests that each hole is inside the polygon shell.
        /// This routine assumes that the holes have previously been tested
        /// to ensure that all vertices lie on the shell or inside it.
        /// A simple test of a single point in the hole can be used,
        /// provide the point is chosen such that it does not lie on the
        /// boundary of the shell.
        /// </summary>
        /// <param name="p">The polygon to be tested for hole inclusion.</param>
        /// <param name="graph">A GeometryGraph incorporating the polygon.</param>
        private void CheckHolesInShell(IPolygon<TCoordinate> p, GeometryGraph<TCoordinate> graph)
        {
            ILinearRing<TCoordinate> shell = p.Shell;

            IPointInRing<TCoordinate> pir = new MCPointInRing<TCoordinate>(shell);
            
            for (Int32 i = 0; i < p.InteriorRingsCount; i++)
            {
                ILinearRing<TCoordinate> hole = p.Holes[i];
                TCoordinate holePt = FindPointNotNode(hole.Coordinates, shell, graph);

                /*
                 * If no non-node hole vertex can be found, the hole must
                 * split the polygon into disconnected interiors.
                 * This will be caught by a subsequent check.
                 */
                if (holePt.Equals(default(TCoordinate)))
                {
                    return;
                }

                Boolean outside = !pir.IsInside(holePt);

                if (outside)
                {
                    validErr = new TopologyValidationError(TopologyValidationErrors.HoleOutsideShell, holePt);
                    return;
                }
            }
        }

        /// <summary>
        /// Tests that no hole is nested inside another hole.
        /// This routine assumes that the holes are disjoint.
        /// To ensure this, holes have previously been tested
        /// to ensure that:
        /// They do not partially overlap
        /// (checked by <c>checkRelateConsistency</c>).
        /// They are not identical
        /// (checked by <c>checkRelateConsistency</c>).
        /// </summary>
        private void CheckHolesNotNested(IPolygon p, GeometryGraph graph)
        {
            QuadtreeNestedRingTester nestedTester = new QuadtreeNestedRingTester(graph);
            foreach (ILinearRing innerHole in p.Holes)
            {
                nestedTester.Add(innerHole);
            }
            Boolean isNonNested = nestedTester.IsNonNested();
            if (!isNonNested)
            {
                validErr = new TopologyValidationError(TopologyValidationErrors.NestedHoles,
                                                       nestedTester.NestedPoint);
            }
        }

        /// <summary>
        /// Tests that no element polygon is wholly in the interior of another element polygon.
        /// Preconditions:
        /// Shells do not partially overlap.
        /// Shells do not touch along an edge.
        /// No duplicate rings exists.
        /// This routine relies on the fact that while polygon shells may touch at one or
        /// more vertices, they cannot touch at ALL vertices.
        /// </summary>
        private void CheckShellsNotNested(IMultiPolygon mp, GeometryGraph graph)
        {
            for (Int32 i = 0; i < mp.NumGeometries; i++)
            {
                IPolygon p = (IPolygon) mp.GetGeometryN(i);
                ILinearRing shell = p.Shell;
                for (Int32 j = 0; j < mp.NumGeometries; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }
                    IPolygon p2 = (IPolygon) mp.GetGeometryN(j);
                    CheckShellNotNested(shell, p2, graph);
                    if (validErr != null)
                    {
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Check if a shell is incorrectly nested within a polygon.  This is the case
        /// if the shell is inside the polygon shell, but not inside a polygon hole.
        /// (If the shell is inside a polygon hole, the nesting is valid.)
        /// The algorithm used relies on the fact that the rings must be properly contained.
        /// E.g. they cannot partially overlap (this has been previously checked by
        /// <c>CheckRelateConsistency</c>).
        /// </summary>
        private void CheckShellNotNested(ILinearRing shell, IPolygon p, GeometryGraph graph)
        {
            ICoordinate[] shellPts = shell.Coordinates;
            // test if shell is inside polygon shell
            ILinearRing polyShell = p.Shell;
            ICoordinate[] polyPts = polyShell.Coordinates;
            ICoordinate shellPt = FindPointNotNode(shellPts, polyShell, graph);
            // if no point could be found, we can assume that the shell is outside the polygon
            if (shellPt == null)
            {
                return;
            }
            Boolean insidePolyShell = CGAlgorithms.IsPointInRing(shellPt, polyPts);
            if (!insidePolyShell)
            {
                return;
            }
            // if no holes, this is an error!
            if (p.NumInteriorRings <= 0)
            {
                validErr = new TopologyValidationError(TopologyValidationErrors.NestedShells, shellPt);
                return;
            }

            /*
             * Check if the shell is inside one of the holes.
             * This is the case if one of the calls to checkShellInsideHole
             * returns a null coordinate.
             * Otherwise, the shell is not properly contained in a hole, which is an error.
             */
            ICoordinate badNestedPt = null;
            for (Int32 i = 0; i < p.NumInteriorRings; i++)
            {
                ILinearRing hole = p.Holes[i];
                badNestedPt = CheckShellInsideHole(shell, hole, graph);
                if (badNestedPt == null)
                {
                    return;
                }
            }
            validErr = new TopologyValidationError(TopologyValidationErrors.NestedShells, badNestedPt);
        }

        /// <summary> 
        /// This routine checks to see if a shell is properly contained in a hole.
        /// It assumes that the edges of the shell and hole do not
        /// properly intersect.
        /// </summary>
        /// <returns>
        /// <see langword="null" /> if the shell is properly contained, or
        /// a Coordinate which is not inside the hole if it is not.
        /// </returns>
        private ICoordinate CheckShellInsideHole(ILinearRing shell, ILinearRing hole, GeometryGraph graph)
        {
            ICoordinate[] shellPts = shell.Coordinates;
            ICoordinate[] holePts = hole.Coordinates;
            // TODO: improve performance of this - by sorting pointlists?
            ICoordinate shellPt = FindPointNotNode(shellPts, hole, graph);
            // if point is on shell but not hole, check that the shell is inside the hole
            if (shellPt != null)
            {
                Boolean insideHole = CGAlgorithms.IsPointInRing(shellPt, holePts);
                if (!insideHole)
                {
                    return shellPt;
                }
            }
            ICoordinate holePt = FindPointNotNode(holePts, shell, graph);
            // if point is on hole but not shell, check that the hole is outside the shell
            if (holePt != null)
            {
                Boolean insideShell = CGAlgorithms.IsPointInRing(holePt, shellPts);
                if (insideShell)
                {
                    return holePt;
                }
                return null;
            }
            Assert.ShouldNeverReachHere("points in shell and hole appear to be equal");
            return null;
        }

        private void CheckConnectedInteriors(GeometryGraph graph)
        {
            ConnectedInteriorTester cit = new ConnectedInteriorTester(graph);
            if (!cit.IsInteriorsConnected())
            {
                validErr = new TopologyValidationError(TopologyValidationErrors.DisconnectedInteriors,
                                                       cit.Coordinate);
            }
        }
    }
}