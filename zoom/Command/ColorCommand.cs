using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using UMD.HCIL.PiccoloX.Util.PStyledTextHelpers;
using UMD.HCIL.Piccolo.Nodes;
using System.Globalization;

namespace zoom.Command
{
    /// <summary>
    /// ColorCommand changes the color of selected text
    /// </summary>
    class ColorCommand : AbstractStyleCommand
    {
        /// <summary>
        /// The Name of the command - the user needs to type this to execute the command
        /// </summary>
        public override string Name { get { return "color"; } }

        /// <summary>
        /// Replace the current selection'sections text color with  the color specified in the argument
        /// </summary>
        /// <param Name="selection">The selection to modify</param>
        /// <param Name="argument">The argument provided to the command</param>
        public override void Execute(Selection selection, string[] arguments)
        {
            //Detect which color the user wanted
            Color? color = GetColor(arguments);
            //If there was a valid color and a valid selection, apply the color to the selection
            if (color != null && selection != null) { MergeAndApply(selection, color: color); }
        }

        /// <summary>
        /// Preview what the text would look like with the specified color
        /// </summary>
        /// <param Name="selection">The selection to modify</param>
        /// <param Name="argument">The argument provided to the command</param>
        /// <returns>A PText showing what the selection would look like if executed</returns>
        public override PText Preview(Selection selection, string[] arguments)
        {
            //If nothing is selected, this cannot run
            if (selection == null) { return new PText("Error: Text must be selected for this command to work"); }

            //Attempt to get the color the user specified
            Color? color = GetColor(arguments);
            if (color == null) { return new PText(String.Format(@"Error: {0} is not a valid color", String.Join(" ", arguments))); }

            //If the color did exist, create a new PText that matches the current selection with the new color
            Style newStyle = MergeStyles(GetStyle(selection), color: GetColor(arguments));
            return PTextForPreview(newStyle, selection);
        }

        /// <summary>
        /// Detect what Color the user specified, in Hex, RGB, or named colors
        /// </summary>
        /// <param Name="argument">The argument provided to the command</param>
        /// <returns>The detected Color, null if no Color is detected</returns>
        public Color? GetColor(string[] arguments)
        {
            //If the user supplied one argument then it'sections either a Hex code or a named color
            if (arguments.Length == 1 && arguments[0].Length > 0)
            {
                // If it looks like a hex code, try to parse it as one
                if (arguments[0][0].Equals('#') && arguments[0].Length == 7) { return ParseHex(arguments[0]); }
                // Otherwise try to parse it as a Name
                else { return ParseNamedColor(arguments[0]); }
            }

            //If the user supplied three argument, then treat them as RGB values
            else if (arguments.Length == 3) { return ParseRGB(arguments); }

            //If it wasn't a recognised format, exit
            return null;
        }

        /// <summary>
        /// Attempt to parse the argument as a hex code
        /// </summary>
        /// <param Name="argument">The hex code to try to parse</param>
        /// <returns>A Color object represented by the specifed hex code, null if it was invalid</returns>
        protected static Color? ParseHex(string argument)
        {
            int red = 0;
            int green = 0;
            int blue = 0;
            CultureInfo culture = new CultureInfo("en-GB");

            //Check that it matches what we expect from a hex code
            bool isHexCode = argument.Length == 7
                && int.TryParse(argument.Substring(1, 2), NumberStyles.HexNumber, culture, out red)
                && int.TryParse(argument.Substring(3, 2), NumberStyles.HexNumber, culture, out green)
                && int.TryParse(argument.Substring(5, 2), NumberStyles.HexNumber, culture, out blue);

            //Return it as a Color if it does
            if (isHexCode) { return Color.FromArgb(red, green, blue); }
            // If it doesn't actually look like a hex code, exit
            else { return null; }
        }

        /// <summary>
        /// Attempt to parse the argument as a named Color
        /// </summary>
        /// <param Name="argument">The Name of the color to parse</param>
        /// <returns>The color specified by the Name</returns>
        protected static Color? ParseNamedColor(string argument)
        {
            //Ensure that the first letter is capitalised, and all others are lower case
            string enteredName = char.ToUpper(argument[0]) + argument.Substring(1).ToLower();

            KnownColor colorName;
            //If it is actually a Name, return that color
            if (Enum.TryParse<KnownColor>(enteredName, out colorName)) { return Color.FromKnownColor(colorName); }
            //Otherwise, exit
            else { return null; }
        }

        /// <summary>
        /// Attempt to parse the arguments as RGB values
        /// </summary>
        /// <param Name="arguments">the arguments supplied to the command</param>
        /// <returns>The color described by the RGB values</returns>
        protected static Color? ParseRGB(string[] arguments)
        {
            int red = 0;
            int green = 0;
            int blue = 0;

            //Check that it does match what we expect from RGB values
            bool isRGB = int.TryParse(arguments[0], out red) && red >= 0 && red < 256
                && int.TryParse(arguments[1], out green) && green >= 0 && green < 256
                && int.TryParse(arguments[2], out blue) && blue >= 0 && blue < 256;

            //If it was RGB values, return that color
            if (isRGB) { return Color.FromArgb(red, green, blue); }
            //Otherwise, exit
            else { return null; }
        }
    }
}
