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
    public class Style
    {
        protected static readonly Font DefaultFont = new Font("Century Gothic", 11);
        protected static readonly Color DefaultColor = Color.FromArgb(0, 0, 0);

        public Font Font { get; protected set; }
        public Color Color { get; protected set; }

        public Style(Font font, Color color)
        {
            Font = font;
            Color = color;
        }

        public Style() : this(DefaultFont, DefaultColor) { }

        public void ApplyStyle(PStyledText target) { ApplyStyle(target.Model); }

        public void ApplyStyle(Model target)
        {
            target.SelectionFont = Font;
            target.SelectionColor = Color;
        }
    }
}
