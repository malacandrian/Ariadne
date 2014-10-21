using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using UMD.HCIL.PiccoloX.Util.PStyledTextHelpers;
using UMD.HCIL.Piccolo.Nodes;

namespace zoom.Command
{
    /// <summary>
    /// Changes the size of the selected text
    /// </summary>
    class SizeCommand : AbstractStyleCommand
    {
        /// <summary>
        /// The Name of the command - the user needs to type this to execute the command
        /// </summary>
        public override string Name { get { return "size"; } }

        /// <summary>
        /// Change the size of the selected text to the one specified in the arguments
        /// </summary>
        /// <param Name="selection">The current selection</param>
        /// <param Name="arguments">The arguments passed to the command</param>
        public override void Execute(Selection selection, string[] arguments)
        {
            int size;
            //If there is a valid selection, and the first argument is an integer
            if (selection != null && arguments.Length > 0 && int.TryParse(arguments[0], out size))
            {
                //Change the size to the number specified in the first argument
                MergeAndApply(selection, size: size);
            }
        }

        /// <summary>
        /// Preview what the text will look like with the new size
        /// </summary>
        /// <param Name="selection">The current selection</param>
        /// <param Name="arguments">The arguments passed to the command</param>
        /// <returns>A PText showing the selected text in the new size</returns>
        public override PText Preview(Selection selection, string[] arguments)
        {
            int size;
            //If there is no selection, abort
            if (selection == null) { return new PText("Error: Text must be selected for this command to work"); }
            //If there are no arguments, abort
            if (arguments.Length == 0) { return new PText("Please enter a size for the text"); }
            //If the first argument isn't an integer, abort
            if (!int.TryParse(arguments[0], out size)) { return new PText(String.Format("Error: {0} is not a valid size", arguments[0])); }

            //Change the style of the text to have the new size
            Style newStyle = MergeStyles(GetStyle(selection), size: size);
            return PTextForPreview(newStyle, selection);
        }
    }
}
