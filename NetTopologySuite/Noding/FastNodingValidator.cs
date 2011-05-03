using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.IO;

namespace GisSharpBlog.NetTopologySuite.Noding
{
    /**
     * Validates that a collection of {@link ISegmentString}s is correctly noded.
     * Indexing is used to improve performance.
     * This class assumes that at least one round of noding has already been performed
     * (which may still leave intersections, due to rounding issues).
     * Does NOT check a-b-a collapse situations. 
     * Also does not check for endpt-interior vertex intersections.
     * This should not be a problem, since the noders should be
     * able to compute intersections between vertices correctly.
     * User may either test the valid condition, or request that a 
     * {@link TopologyException} 
     * be thrown.
     *
     * @version 1.7
     */
    public class FastNodingValidator
    {
        private readonly LineIntersector _li = new RobustLineIntersector();

        private readonly List<ISegmentString> _segStrings = new List<ISegmentString>();
        private InteriorIntersectionFinder _segInt;
        private Boolean _isValid = true;

        public FastNodingValidator(IEnumerable<ISegmentString> segStrings)
        {
            _segStrings.AddRange(segStrings);
        }

        ///<summary>
        /// Checks for an intersection and reports if one is found.
        ///</summary>
        public Boolean IsValid
        {
            get
            {
                Execute();
                return _isValid;
            }
        }

        ///<summary>
        /// Returns an error message indicating the segments containing the intersection.
        ///</summary>
        ///<returns>an error message documenting the intersection location</returns>
        public String GetErrorMessage()
        {
            if (IsValid)
                return "no intersections found";

            ICoordinate[] intSegs = _segInt.IntersectionSegments;
            return "found non-noded intersection between "
                + WKTWriter.ToLineString(intSegs[0], intSegs[1])
                + " and "
                + WKTWriter.ToLineString(intSegs[2], intSegs[3]);
        }

        ///<summary>
        /// Checks for an intersection and throws
        /// a TopologyException if one is found.
        ///</summary>
        ///<exception cref="TopologyException">if an intersection is found</exception>
        public void CheckValid()
        {
            if (!IsValid)
                throw new TopologyException(GetErrorMessage(), _segInt.InteriorIntersection);
        }

        private void Execute()
        {
            if (_segInt != null)
                return;
            CheckInteriorIntersections();
        }

        private void CheckInteriorIntersections()
        {
            /**
             * MD - It may even be reliable to simply check whether 
             * end segments (of SegmentStrings) have an interior intersection,
             * since noding should have split any true interior intersections already.
             */
            _isValid = true;
            _segInt = new InteriorIntersectionFinder(_li);
            MCIndexNoder noder = new MCIndexNoder(_segInt);
            noder.ComputeNodes(_segStrings); //.ComputeNodes(segStrings);
            if (_segInt.HasIntersection)
            {
                _isValid = false;
                return;
            }
        }

    }
}
