using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMD.HCIL.PiccoloX.Nodes;
using UMD.HCIL.PiccoloX.Util.PStyledTextHelpers;

namespace zoom
{
    /// <summary>
    /// Style represents the Font and color of a piece of text
    /// </summary>
    public class Style
    {
        /// <summary>
        /// The font to use if none is specified
        /// </summary>
        public static readonly Font DefaultFont = new Font("Century Gothic", 11);

        /// <summary>
        /// The Color to use if none is specified
        /// </summary>
        public static readonly Color DefaultColor = Color.FromArgb(0, 0, 0);

        /// <summary>
        /// The Font of the Style
        /// </summary>
        public Font Font { get; protected set; }

        /// <summary>
        /// The Color of the Style
        /// </summary>
        public Color Color { get; protected set; }

        /// <summary>
        /// Create a new Style
        /// </summary>
        /// <param name="font">The font of the style</param>
        /// <param name="color">The color of the style</param>
        public Style(Font font = null, Color? color = null)
        {
            if (font == null) { Font = DefaultFont; }
            else { Font = font; }

            if (color == null) { Color = DefaultColor; }
            else { Color = (Color)color; }
        }

        /// <summary>
        /// Sets the current selection of a PStyledText to the current style
        /// </summary>
        /// <param name="target"></param>
        public void ApplyStyle(PStyledText target) { ApplyStyle(target.Model); }

        /// <summary>
        /// Sets the current selection of a Model to the current style
        /// </summary>
        /// <param name="target"></param>
        public void ApplyStyle(Model target)
        {
            target.SelectionFont = Font;
            target.SelectionColor = Color;
        }
    }
}
