using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Relate
{
    /// <summary>
    /// A RelateNode is a Node that maintains a list of EdgeStubs
    /// for the edges that are incident on it.
    /// </summary>
    public class RelateNode<TCoordinate, TEdgeEnd> : Node<TCoordinate, TEdgeEnd>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<TCoordinate>, IConvertible
        where TEdgeEnd : EdgeEnd<TCoordinate>
    {
        public RelateNode(TCoordinate coord, EdgeEndStar<TCoordinate, TEdgeEnd> edges) :
            base(coord, edges) {}

        /// <summary>
        /// Update the IM with the contribution for this component.
        /// A component only contributes if it has a labeling for both parent geometries.
        /// </summary>
        public override void ComputeIntersectionMatrix(IntersectionMatrix im)
        {
            im.SetAtLeastIfValid(Label.GetLocation(0), Label.GetLocation(1), Dimensions.Point);
        }

        /// <summary>
        /// Update the IM with the contribution for the EdgeEnds incident 
        /// on this node.
        /// </summary>
        public void UpdateIntersectionMatrixFromEdges(IntersectionMatrix im)
        {
            ((EdgeEndBundleStar<TCoordinate>) Edges).UpdateIntersectionMatrix(im);
        }
    }
}