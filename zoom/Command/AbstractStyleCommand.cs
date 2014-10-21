using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMD.HCIL.Piccolo;
using UMD.HCIL.Piccolo.Nodes;
using UMD.HCIL.PiccoloX.Util.PStyledTextHelpers;

namespace zoom.Command
{
    /// <summary>
    /// AbstractStyleCommand provides a set of hooks for manipulating the style of selected text
    /// </summary>
    public abstract class AbstractStyleCommand : ICommand
    {
        /// <summary>
        /// The Name of the command - the user needs to type this to execute the command
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// What to do when the user releases the command key
        /// typically this will modify the text of the selection
        /// </summary>
        /// <param Name="selection">The current selection, used as input, and modified for the output</param>
        /// <param Name="argument">Anything added after the command Name, used as input</param>
        public abstract void Execute(Selection selection, string[] arguments);

        /// <summary>
        /// A preview of what will happen if the user releases the command key
        /// </summary>
        /// <param Name="selection">The current selection, used as input</param>
        /// <param Name="argument">Anything added after the command Name, used as input</param>
        /// <returns>A PText of what would happen to the selection, should the user release the key</returns>
        public abstract PText Preview(Selection selection, string[] arguments);

        /// <summary>
        /// Gets the style of a given selection
        /// </summary>
        /// <param Name="selection">The selection to get the style of</param>
        /// <returns>The style of the given selection</returns>
        protected Style GetStyle(Selection selection) { return new Style(selection.Font, selection.Color); }

        /// <summary>
        /// Creates a new style that matches the style of a selection, with a given FontStyle toggled
        /// </summary>
        /// <param Name="selection">The selection to get the style from</param>
        /// <param Name="style">The FontStyle to toggle</param>
        /// <returns>A new Style with the FontStyle toggled</returns>
        protected Style ToggleFontStyle(Selection selection, FontStyle style)
        {
            //This cannot work on a null selection
            if (selection != null)
            {
                //Get the current FontStyle of the selection
                FontStyle curStyle = selection.Font.Style;

                //If the specified style is active, deactivate it
                if (curStyle.HasFlag(style)) { curStyle &= ~style; }
                //If the specified style is inactive, activate it
                else { curStyle |= style; }

                //Merge the new FontStyle with the Selection'sections Style and return it
                return MergeStyles(GetStyle(selection), style: curStyle, overwriteFontStyle: true);
            }
            return null;
        }

        /// <summary>
        /// Adjust a given Style with other properties. 
        /// Any property left unspecified uses the value from the source Style
        /// </summary>
        /// <param Name="toMerge">The source Style</param>
        /// <param Name="size">The size of the new style</param>
        /// <param Name="color">The color of the new style</param>
        /// <param Name="style">the FontStyle of the new style</param>
        /// <param Name="overwriteFontStyle">
        /// If true - the new FontStyle should replace the source one.
        /// If false - the new FontStyle should be or-ed with the source one
        /// </param>
        /// <param Name="family">The font family of the new Style</param>
        /// <returns>The new, merged style</returns>
        protected Style MergeStyles(Style toMerge, float? size = null, Color? color = null, FontStyle? style = null, bool overwriteFontStyle = false, FontFamily family = null)
        {
            //Merge sizes
            float newSize = toMerge.Font.Size;
            if (size != null) { newSize = (float)size; }

            //Merge FontStyles
            FontStyle newStyle = toMerge.Font.Style;
            if (style != null)
            {
                if (overwriteFontStyle) { newStyle = (FontStyle)style; }
                else { newStyle |= (FontStyle)style; }
            }

            //Merge Colors
            Color newColor = toMerge.Color;
            if (color != null) { newColor = (Color)color; }

            //Merge Families
            FontFamily newFamily = toMerge.Font.FontFamily;
            if (family != null) { newFamily = family; }

            //Output the new Style
            Font outFont = new Font(newFamily, newSize, newStyle);
            return new Style(outFont, newColor);
        }

        /// <summary>
        /// Replaces the style of a given selection with a specified style
        /// </summary>
        /// <param Name="selection">The selection to change</param>
        /// <param Name="style">The style to change to</param>
        protected void ApplyStyle(Selection selection, Style style)
        {
            //This only works if the selection exists
            if (selection != null)
            {
                selection.Font = style.Font;
                selection.Color = style.Color;
            }
        }

        /// <summary>
        /// Change some aspects of the style of the given selection
        /// any fields left blank are left as it currently is
        /// </summary>
        /// <param Name="selection">The selection to modify</param>
        /// <param Name="size">The new size of the selection text</param>
        /// <param Name="color">The new color of the selection text</param>
        /// <param Name="style">The new FontStyle of the selection text</param>
        /// <param Name="overwriteFontStyle">
        /// If true - the new FontStyle should replace the source one.
        /// If false - the new FontStyle should be or-ed with the source one
        /// </param>
        /// <param Name="family">The new Font Family of the selection text</param>
        protected void MergeAndApply(Selection selection, float? size = null, Color? color = null, FontStyle? style = null, bool overwriteFontStyle = false, FontFamily family = null)
        {
            ApplyStyle(selection, MergeStyles(GetStyle(selection), size, color, style, overwriteFontStyle, family));
        }

        /// <summary>
        /// Toggle a FontStyle of the specified selection
        /// </summary>
        /// <param Name="selection">The selection to modify</param>
        /// <param Name="style">The FontStyle to toggle</param>
        protected void ToggleAndApply(Selection selection, FontStyle style) { ApplyStyle(selection, ToggleFontStyle(selection, style)); }

        /// <summary>
        /// Produce a PText containing the text of the current selection in a particular style
        /// </summary>
        /// <param Name="style">The Style the text should be in</param>
        /// <param Name="selection">The selection to get the text from</param>
        /// <returns>A PText containing the text of the selection in the specified style</returns>
        protected PText PTextForPreview(Style style, Selection selection)
        {
            PText output = new PText(selection.Text);
            output.Font = style.Font;
            output.TextBrush = new SolidBrush(style.Color);
            return output;
        }
    }
}
