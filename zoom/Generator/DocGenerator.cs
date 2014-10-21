using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMD.HCIL.Piccolo;
using MathNet.Numerics.Distributions;
using MathNet.Numerics;

namespace zoom.Generator
{
    /// <summary>
    /// Generates random documents as sample data for the system using a finite state machine
    /// </summary>
    public class DocGenerator
    {
        /// <summary>
        /// What symbol to startSymbol generation on
        /// </summary>
        public Section StartSymbol { get; protected set; }

        /// <summary>
        /// The Window object generated documents should be attached to
        /// </summary>
        public Window Window { get; protected set; }

        /// <summary>
        /// The random number generator used to randomise paths through the state machine
        /// </summary>
        private static Random _Random = new Random();

        /// <summary>
        /// Create a new DocGenerator with given start symbol and target window
        /// </summary>
        /// <param Name="startSymbol">The symbol to start generation on</param>
        /// <param Name="window">The window to attach the documents to</param>
        public DocGenerator(Section startSymbol, Window window)
        {
            StartSymbol = startSymbol;
            Window = window;
        }

        /// <summary>
        /// Generate an arbitrary number of documents
        /// </summary>
        /// <param Name="numDocs">The number of documents to create</param>
        /// <returns>The set of created documents</returns>
        public Document[] GenerateDocSet(int numDocs)
        {
            //Get random locations for each document to be generated on
            PointF[] coords = MapCluster(new Point(0, 0), 1000, 100, numDocs);
            return GenerateDocSet(coords);
        }

        /// <summary>
        /// Generate a set of documents on given coordinates
        /// </summary>
        /// <param Name="coords">The coordinates of the top-left corner of each document</param>
        /// <returns>The set of created documents</returns>
        public Document[] GenerateDocSet(PointF[] coords)
        {
            List<Document> output = new List<Document>();
            for (int i = 0; i < coords.Length; i += 1) { output.Add(GenerateDocument(coords[i])); }
            return output.ToArray();
        }

        /// <summary>
        /// Creates a cluster of points randomly, but evenly distributed around a focus
        /// </summary>
        /// <param Name="focus">The focus the generated points should be distributed around</param>
        /// <param Name="lengthMean">The mean distance a generated point should be from the focus</param>
        /// <param Name="lengthDev">The standard deviation of the distance a generated point should be from the focus</param>
        /// <param Name="numPoints">The number of points to generate</param>
        /// <returns>The set of generated Points</returns>
        public PointF[] MapCluster(Point focus, double lengthMean, double lengthDev, int numPoints)
        {
            List<PointF> output = new List<PointF>();

            //To distribute the points evenly, each should fall in a region angular size 2Pi/#points
            double piRadsPerNode = 2.0 / numPoints;

            for (int i = 0; i < numPoints; i += 1)
            {
                //Generate the new point in Polar coordinates
                double direction = Normal.Sample(piRadsPerNode * i, piRadsPerNode / 3);
                double magnitude = Normal.Sample(lengthMean, lengthDev);

                //Convert the coordinates to a cartesean point
                output.Add(PolarToPoint(direction * Math.PI, magnitude));
            }

            return output.ToArray();
        }

        /// <summary>
        /// Convert Polar coordianates to a cartesean point
        /// </summary>
        /// <param Name="direction">The direction the point is in</param>
        /// <param Name="magnitude">The distance from the origin of the point</param>
        /// <returns>The point in polar coordinates</returns>
        protected PointF PolarToPoint(double direction, double magnitude)
        {
            //C#'sections only way of dealing with polar coordinates is as a complex number
            //So we're going to create a new complex number, and read off its components
            Complex32 comp = Complex32.FromPolarCoordinates((float)magnitude, (float)direction);
            return new PointF(comp.Real, comp.Imaginary);
        }

        /// <summary>
        /// Generate a document at a given location
        /// </summary>
        /// <param Name="coords">The top-left corner of the new document</param>
        /// <returns>The generated document</returns>
        public Document GenerateDocument(PointF coords) { return GenerateDocument(coords.X, coords.Y); }

        /// <summary>
        /// Generate a document at given x,y coordinates
        /// </summary>
        /// <param Name="x">The X coordinate of the top-left corner of the document</param>
        /// <param Name="y">The Y coordinate of the top-left corner of the document</param>
        /// <returns>The generated document</returns>
        public Document GenerateDocument(float x, float y)
        {
            Document output = new Document(x, y, ' ', Window);
            GenerateContent(output);
            return output;
        }

        /// <summary>
        /// Fill the target document with random content
        /// </summary>
        /// <param Name="doc"></param>
        public void GenerateContent(Document doc)
        {
            //Starting with the start symbol, randomly navigate the Finite State Machine
            //Filling the target document with content until it hits the terminate symbol
            Section next = StartSymbol;
            while ((next = next.generate(doc)) != null) { }
        }
    }
}
