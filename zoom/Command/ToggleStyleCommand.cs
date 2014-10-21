using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using UMD.HCIL.Piccolo;
using UMD.HCIL.PiccoloX.Util.PStyledTextHelpers;
using UMD.HCIL.Piccolo.Nodes;

namespace zoom.Command
{
    /// <summary>
    /// Toggles the value of a specified FontStyle in the font of the selected text
    /// </summary>
    public class ToggleStyleCommand : AbstractStyleCommand
    {
        /// <summary>
        /// The Name of the command - the user needs to type this to execute the command
        /// </summary>
        public override string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// The Name of the command - the user needs to type this to execute the command
        /// </summary>
        protected readonly string _name;

        /// <summary>
        /// The FontStyle that this instance of ToggleStyleCommand will toggle
        /// </summary>
        public readonly FontStyle Style;

        /// <summary>
        /// Create a new ToggleStyleCommand with specified Name and style
        /// </summary>
        /// <param Name="Name">The Name of the command - the user needs to type this to execute the command</param>
        /// <param Name="style">The FontStyle that this instance of ToggleStyleCommand will toggle</param>
        public ToggleStyleCommand(string name, FontStyle style)
        {
            _name = name;
            Style = style;
        }

        /// <summary>
        /// Toggle the value of a FontStyle in the selected text
        /// </summary>
        /// <param Name="selection">The current selection</param>
        /// <param Name="arguments">The arguments provided to the command</param>
        public override void Execute(Selection selection, string[] arguments) { ToggleAndApply(selection, Style); }

        /// <summary>
        /// Creates a PText showing what the selected text would look like with the relevant FontStyle toggled
        /// </summary>
        /// <param Name="selection">The current selection</param>
        /// <param Name="arguments">The arguments provided to the command</param>
        /// <returns>A PText showing what the text would look like with the relevant FontStyle toggled</returns>
        public override PText Preview(Selection selection, string[] arguments)
        {
            //This only makes sense if there'sections a selection
            if (selection == null) { return new PText("Error: Text must be selected for this command to work"); }

            //Create a new PText with the relevant FontStyle toggled
            Style newStyle = ToggleFontStyle(selection, Style);
            return PTextForPreview(newStyle, selection);
        }
    }
}
