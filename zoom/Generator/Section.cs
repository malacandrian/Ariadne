using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Distributions;
using Faker;
using UMD.HCIL.PiccoloX.Util.PStyledTextHelpers;
using UMD.HCIL.PiccoloX.Nodes;

namespace zoom.Generator
{
    /// <summary>
    /// A paragraph of a dynamically generated document
    /// </summary>
    /// <remarks>
    /// Each section acts as a state in the FSM that generates random documents. 
    /// The next state is determined non-deterministically by each Section'sections SectionSelector
    /// </remarks>
    public class Section
    {
        /// <summary>
        /// The mean length that the generated paragraph should be
        /// </summary>
        public int ParagraphLength { get; protected set; }

        /// <summary>
        /// The standard deviation in the length of the generated paragraph
        /// </summary>
        public int ParagraphVariance { get; protected set; }

        /// <summary>
        /// A SectionSelector to randomly decide which state should follow this one
        /// </summary>
        public SectionSelector NextSection;

        /// <summary>
        /// The style the generated paragraph should be rendered in
        /// </summary>
        public Style Style { get; protected set; }

        /// <summary>
        /// A random number generator for varying paragraph length
        /// </summary>
        protected static Random _Random = new Random();

        /// <summary>
        /// Create a new Section with specified Font
        /// </summary>
        /// <param Name="font">The font the generated paragraph should be rendered in</param>
        /// <param Name="paragraphLength">The mean length of the paragraph</param>
        /// <param Name="paragraphVariance">The standard deviation of paragraph length</param>
        public Section(Font font, int paragraphLength, int paragraphVariance) : this(font, Color.FromArgb(0, 0, 0), paragraphLength, paragraphVariance) { }

        /// <summary>
        /// Create a new Section with specified Font and Color
        /// </summary>
        /// <param Name="font">The font the generated paragraph should be rendered in</param>
        /// <param Name="color">The color the generated paragraph should be rendered in</param>
        /// <param Name="paragraphLength">The mean length of the paragraph</param>
        /// <param Name="paragraphVariance">The standard deviation of paragraph length</param>
        public Section(Font font, Color color, int paragraphLength, int paragraphVariance) : this(new Style(font, color), paragraphLength, paragraphVariance) { }

        /// <summary>
        /// Create a new Section with specified Style
        /// </summary>
        /// <param Name="style">The Style the generated paragraph should be rendered in</param>
        /// <param Name="paragraphLength">The mean length of the paragraph</param>
        /// <param Name="paragraphVariance">The standard deviation of paragraph length</param>
        public Section(Style style, int paragraphLength, int paragraphVariance)
        {
            Style = style;
            NextSection = new SectionSelector();
            ParagraphLength = paragraphLength;
            ParagraphVariance = paragraphVariance;
        }

        /// <summary>
        /// Decide which symbol should follow this one
        /// </summary>
        /// <returns></returns>
        public Section getNext() { return NextSection.Select(); }

        /// <summary>
        /// Generate text and insert it in to the specified Document
        /// </summary>
        /// <param Name="target">The Document to insert the text into</param>
        /// <returns>The symbol that follows this one</returns>
        public Section generate(Document target) { return generate(target.Pages[target.Pages.Length - 1]); }

        /// <summary>
        /// Generate text and insert it in to the specified Page
        /// </summary>
        /// <param Name="target">The Page to insert the text into</param>
        /// <returns>The symbol that follows this one</returns>
        public Section generate(Page target) { return generate(target.Model); }

        /// <summary>
        /// Generate text and insert it in to the specified Model
        /// </summary>
        /// <param Name="target">The Model to insert the text into</param>
        /// <returns>The symbol that follows this one</returns>
        public Section generate(Model target)
        {
            //Add the styled text
            target.Select(target.TextLength, 0);
            Style.ApplyStyle(target);
            target.SelectedText = generateText();

            //Decide which type of paragraph goes next
            return getNext();
        }

        /// <summary>
        /// Generate a random paragraph of the appropriate length
        /// </summary>
        /// <returns>The text of the random paragraph</returns>
        public string generateText()
        {
            int pLength = (int)Math.Max(1,Math.Ceiling(Normal.Sample((double)ParagraphLength, (double)ParagraphVariance)));
            return String.Join(" ", Lorem.Sentences(pLength)) + "\n";

        }
    }
}
