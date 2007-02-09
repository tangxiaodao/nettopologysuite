using System;
using System.Collections;
using System.Text;

using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using GisSharpBlog.NetTopologySuite.Operation;

namespace GisSharpBlog.NetTopologySuite.Operation.Relate
{
    /// <summary>
    /// Implements the <c>Relate()</c> operation on <c>Geometry</c>s.
    /// </summary>
    public class RelateOp : GeometryGraphOperation
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static IntersectionMatrix Relate(Geometry a, Geometry b)
        {
            RelateOp relOp = new RelateOp(a, b);
            IntersectionMatrix im = relOp.IntersectionMatrix;
            return im;
        }

        private RelateComputer relate = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="g0"></param>
        /// <param name="g1"></param>
        public RelateOp(Geometry g0, Geometry g1) : base(g0, g1)
        {            
            relate = new RelateComputer(arg);
        }

        /// <summary>
        /// 
        /// </summary>
        public IntersectionMatrix IntersectionMatrix
        {
            get
            {
                return relate.ComputeIM();
            }
        }
    }
}