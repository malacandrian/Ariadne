using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMD.HCIL.Piccolo;
using UMD.HCIL.Piccolo.Nodes;
using UMD.HCIL.PiccoloX.Util.PStyledTextHelpers;

namespace zoom.Commands
{
    public abstract class AbstractStyleCommand : ICommand
    {
        public abstract string Name { get; }


        public abstract void Execute(Selection selection, string[] arguments);
        public abstract PText Preview(Selection selection, string[] arguments);

        protected Style GetStyle(Selection selection)
        {
            return new Style(selection.Font, selection.Color);
        }

        protected Style ToggleFontStyle(Selection selection, FontStyle style)
        {
            if (selection != null)
            {
                FontStyle curStyle = selection.Font.Style;
                if (curStyle.HasFlag(style)) 
                { curStyle &= ~style; }
                else { curStyle |= style; }

                return MergeStyles(GetStyle(selection), style: curStyle);
            }
            return null;
        }

        protected Style MergeStyles(Style toMerge, float? size = null, Color? color = null, FontStyle? style = null, bool overwriteFontStyle = false, FontFamily family = null)
        {
            float newSize = toMerge.Font.Size;
            if (size != null) { newSize = (float)size; }

            FontStyle newStyle = toMerge.Font.Style;
            if (style != null)
            {
                if (overwriteFontStyle) { newStyle = (FontStyle)style; }
                else { newStyle |= (FontStyle)style; }
            }

            Color newColor = toMerge.Color;
            if (color != null) { newColor = (Color)color; }

            FontFamily newFamily = toMerge.Font.FontFamily;
            if (family != null) { newFamily = family; }

            Font outFont = new Font(newFamily, newSize, newStyle);
            return new Style(outFont, newColor);
        }

        protected void ApplyStyle(Selection selection, Style style)
        {
            if (selection != null)
            {
                selection.Font = style.Font;
                selection.Color = style.Color;
            }
        }

        protected void MergeAndApply(Selection selection, float? size = null, Color? color = null, FontStyle? style = null, bool overwriteFontStyle = false, FontFamily family = null)
        {
            ApplyStyle(selection, MergeStyles(GetStyle(selection), size, color, style, overwriteFontStyle, family));
        }

        protected void ToggleAndApply(Selection selection, FontStyle style)
        {
            ApplyStyle(selection, ToggleFontStyle(selection, style));
        }

        protected PText PTextForPreview(Style style, Selection selection)
        {
            PText output = new PText(selection.Text);
            output.Font = style.Font;
            output.TextBrush = new SolidBrush(style.Color);
            return output;
        }
    }
}
