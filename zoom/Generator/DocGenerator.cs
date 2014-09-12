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
    public class DocGenerator
    {
        public Section StartSymbol {get; protected set;}
        public Window Window {get; protected set;}

        private static Random RGen = new Random();

        public PCamera Camera
        {
            get
            {
                return Window.Canvas.Camera;
            }
        }

        public DocGenerator(Section start, Window window)
        {
            StartSymbol = start;
            Window = window;
        }

        public Document[] GenerateDocSet(int numDocs)
        {
            PointF[] coords = MapCluster(new Point(0, 0), 1000, 100, numDocs);
            return GenerateDocSet(coords);
        }

        public Document[] GenerateDocSet(PointF[] coords)
        {
            List<Document> output = new List<Document>();
            
            for (int i = 0; i < coords.Length; i += 1)
            {
                output.Add(GenerateDocument(coords[i]));
            }

            return output.ToArray();
        }

        public PointF[] MapCluster(Point focus, double lengthMean, double lengthDev, int numPoints)
        {
            List<PointF> output = new List<PointF>();

            double piRadsPerNode = 2.0 / numPoints;
            for (int i = 0; i < numPoints; i += 1)
            {
                double direction = Normal.Sample(piRadsPerNode * i, piRadsPerNode / 3);
                double magnitude = Normal.Sample(lengthMean, lengthDev);
                output.Add(PolarToPoint(direction * Math.PI, magnitude));
            }

            return output.ToArray();
        }

        protected PointF PolarToPoint(double direction, double magnitude)
        {
            Complex32 comp = Complex32.FromPolarCoordinates((float)magnitude,(float) direction);
            return new PointF(comp.Real, comp.Imaginary);
        }


        public Document GenerateDocument(PointF coords)
        {
            return GenerateDocument((int)coords.X, (int)coords.Y);
        }

        public Document GenerateDocument(int x, int y)
        {
            Document output = new Document(x,y,' ',Window, Camera);
            GenerateContent(output);
            return output;
        }

        public void GenerateContent(Document doc)
        {
            Section next = StartSymbol;
            while ((next = next.generate(doc)) != null) { }
        }
    }
}
