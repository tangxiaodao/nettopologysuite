using System;
using System.Collections;

namespace GisSharpBlog.NetTopologySuite.Planargraph
{
    /// <summary>
    /// The base class for all graph component classes.
    /// </summary>
    /// <remarks>
    /// Maintains flags of use in generic graph algorithms.
    /// Provides two flags:
    /// <list type="table">
    /// <item>
    /// <term>Marked</term>
    /// <description>
    /// Typically this is used to indicate a state that persists
    /// for the course of the graph's lifetime.  For instance, it can be
    /// used to indicate that a component has been logically deleted from the graph.
    /// </description>
    /// <term>Visited</term>
    /// <description>
    /// This is used to indicate that a component has been processed
    /// or visited by an single graph algorithm.  For instance, a breadth-first traversal of the
    /// graph might use this to indicate that a node has already been traversed.
    /// The visited flag may be set and cleared many times during the lifetime of a graph.
    /// </description>
    /// </list>
    /// </remarks>
    public abstract class GraphComponent
    {
        #region Static

        /// <summary>
        /// Sets the <see cref="GraphComponent.Visited" /> state 
        /// for all <see cref="GraphComponent" />s in an <see cref="IEnumerator" />.
        /// </summary>
        /// <param name="i">A <see cref="IEnumerator" /> to scan.</param>
        /// <param name="visited">The state to set the <see cref="GraphComponent.Visited" /> flag to.</param>
        public static void SetVisited(IEnumerator i, Boolean visited)
        {
            while (i.MoveNext())
            {
                GraphComponent comp = (GraphComponent) i.Current;
                comp.Visited = visited;
            }
        }

        /// <summary>
        /// Sets the <see cref="GraphComponent.Marked" /> state 
        /// for all <see cref="GraphComponent" />s in an <see cref="IEnumerator" />.
        /// </summary>
        /// <param name="i">A <see cref="IEnumerator" /> to scan.</param>
        /// <param name="marked">The state to set the <see cref="GraphComponent.Marked" /> flag to.</param>
        public static void SetMarked(IEnumerator i, Boolean marked)
        {
            while (i.MoveNext())
            {
                GraphComponent comp = (GraphComponent) i.Current;
                comp.Marked = marked;
            }
        }

        /// <summary>
        /// Finds the first <see cref="GraphComponent" /> 
        /// in a <see cref="IEnumerator" /> set
        /// which has the specified <see cref="GraphComponent.Visited" /> state.
        /// </summary>
        /// <param name="i">A <see cref="IEnumerator" /> to scan.</param>
        /// <param name="visitedState">The <see cref="GraphComponent.Visited" /> state to test.</param>
        /// <returns>The first <see cref="GraphComponent" /> found, or <see langword="null" /> if none found.</returns>
        public static GraphComponent GetComponentWithVisitedState(IEnumerator i, Boolean visitedState)
        {
            while (i.MoveNext())
            {
                GraphComponent comp = (GraphComponent) i.Current;
                if (comp.IsVisited == visitedState)
                    return comp;
            }
            return null;
        }
        
        #endregion

        protected Boolean _isMarked = false;
        protected Boolean _isVisited = false;

        /// <summary>
        /// Tests if a component has been visited during the course of a graph algorithm.
        /// </summary>              
        public Boolean IsVisited
        {
            get
            {
                return Visited;
            }
        }

        /// <summary> 
        /// Gets/Sets the visited flag for this component.
        /// </summary>
        public Boolean Visited
        {
            get
            {
                return _isVisited;
            }
            set
            {
                _isVisited = value;
            }
        }

        /// <summary>
        /// Tests if a component has been marked at some point during the processing
        /// involving this graph.
        /// </summary>
        public Boolean IsMarked
        {
            get
            {
                return Marked;
            }
        }

        /// <summary>
        /// Gets/Sets the marked flag for this component.
        /// </summary>
        public Boolean Marked
        {
            get
            {
                return _isMarked;
            }
            set
            {
                _isMarked = value;
            }
        }

        /// <summary>
        /// Tests whether this component has been removed from its containing graph.
        /// </summary>
        public abstract Boolean IsRemoved { get; }
    }
}