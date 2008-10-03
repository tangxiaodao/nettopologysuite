using System;
using System.Collections.Generic;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GeoAPI.IO.WellKnownText;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Operation.Linemerge;
using NetTopologySuite.Coordinates;

namespace GisSharpBlog.NetTopologySuite.Samples.Operation.Linemerge
{
    /// <summary> 
    /// Example of using the <see cref="LineMerger{TCoordinate}"/> class to 
    /// sew together a set of fully noded linestrings.
    /// </summary>	
    public class LineMergeExample
    {
        private void InitBlock()
        {
            reader = new WktReader<BufferedCoordinate>(
                GeometryFactory<BufferedCoordinate>.CreateFloatingPrecision(
                    new BufferedCoordinateSequenceFactory()),
                null);
        }

        internal virtual IEnumerable<IGeometry<BufferedCoordinate>> Data
        {
            get
            {
                yield return Read("LINESTRING (220 160, 240 150, 270 150, 290 170)");
                yield return Read("LINESTRING (60 210, 30 190, 30 160)");
                yield return Read("LINESTRING (70 430, 100 430, 120 420, 140 400)");
                yield return Read("LINESTRING (160 310, 160 280, 160 250, 170 230)");
                yield return Read("LINESTRING (170 230, 180 210, 200 180, 220 160)");
                yield return Read("LINESTRING (30 160, 40 150, 70 150)");
                yield return Read("LINESTRING (160 310, 200 330, 220 340, 240 360)");
                yield return Read("LINESTRING (140 400, 150 370, 160 340, 160 310)");
                yield return Read("LINESTRING (160 310, 130 300, 100 290, 70 270)");
                yield return Read("LINESTRING (240 360, 260 390, 260 410, 250 430)");
                yield return Read("LINESTRING (70 150, 100 180, 100 200)");
                yield return Read("LINESTRING (70 270, 60 260, 50 240, 50 220, 60 210)");
                yield return Read("LINESTRING (100 200, 90 210, 60 210)");
            }
        }

        private IWktGeometryReader reader;

        public LineMergeExample()
        {
            InitBlock();
        }

        [STAThread]
        public static void Main(String[] args)
        {
            LineMergeExample test = new LineMergeExample();

            try
            {
                test.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        internal virtual void Run()
        {
            IEnumerable<IGeometry<BufferedCoordinate>> lineStrings = Data;

            LineMerger<BufferedCoordinate> lineMerger
                = new LineMerger<BufferedCoordinate>();

            lineMerger.Add(lineStrings);

            IEnumerable<ILineString<BufferedCoordinate>> mergedLineStrings
                = lineMerger.MergedLineStrings;

            Console.WriteLine("Lines formed (" + Enumerable.Count(mergedLineStrings) + "):");

            foreach (Object obj in mergedLineStrings)
            {
                Console.WriteLine(obj);
            }
        }


        internal virtual IGeometry<BufferedCoordinate> Read(String lineWKT)
        {
            try
            {
                IGeometry<BufferedCoordinate> geom
                    = reader.Read(lineWKT) as IGeometry<BufferedCoordinate>;
                return geom;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }

            return null;
        }
    }
}