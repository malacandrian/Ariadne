using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using UMD.HCIL.PiccoloX.Util.PStyledTextHelpers;
using UMD.HCIL.Piccolo.Nodes;

namespace zoom.Commands
{
    class ColorCommand : AbstractStyleCommand
    {
        public override string Name { get { return "color"; } }

        public override void Execute(Selection selection, string[] arguments)
        {
            Color? color = GetColor(arguments);
            //If there was a valid color and a valid selection, apply the color to the selection
            if (color != null && selection != null)
            {
                //ApplyStyle(selection, MergeStyles(GetStyle(selection), color: color));
                MergeAndApply(selection, color: color);
            }
        }

        public override PText Preview(Selection selection, string[] arguments)
        {
            Color? color = GetColor(arguments);
            if (selection == null) { return new PText("Error: Text must be selected for this command to work"); }
            if (color == null) { return new PText(String.Format(@"Error: {0} is not a valid color", String.Join(" ", arguments))); }

            Style newStyle = MergeStyles(GetStyle(selection), color: GetColor(arguments));
            return PTextForPreview(newStyle, selection);
        }

        public Color? GetColor(string[] arguments)
        {
            if (arguments.Length == 0) //If no color is supplied, exit
            {
                return null;
            }

            else if (arguments.Length == 1 && arguments[0].Length > 0) //Name or Hex code
            {
                if (arguments[0][0].Equals('#')) // If it's a Hex code
                {
                    return TestHex(arguments);
                }
                else
                {
                    //It might be a name
                    return TestNamedColor(arguments);
                }
            }
            else if (arguments.Length == 3) //RGB values
            {
                return TestRGB(arguments);
            }

            //If it wasn't a recognised format, exit
            return null;
        }

        protected static Color? TestHex(string[] arguments)
        {
            int red, green, blue;
            //Check that it matches what we expect from a hex code
            if (arguments[0].Length == 7
                && int.TryParse(arguments[0].Substring(1, 2), out red)
                && int.TryParse(arguments[0].Substring(3, 2), out green)
                && int.TryParse(arguments[0].Substring(5, 2), out blue))
            {
                return Color.FromArgb(red, green, blue);
            }
            else // If it doesn't actually look like a hex code, exit
            {
                return null;
            }
        }

        protected static Color? TestNamedColor(string[] arguments)
        {
            string enteredName = arguments[0];
            enteredName = char.ToUpper(enteredName[0]) + enteredName.Substring(1);

            KnownColor colorName;
            //If it is actually a name, return that color
            if (Enum.TryParse<KnownColor>(enteredName, out colorName))
            {
                return Color.FromKnownColor(colorName);
            }
            else //Otherwise, exit
            {
                return null;
            }
        }

        protected static Color? TestRGB(string[] arguments)
        {
            int red, green, blue;
            if (int.TryParse(arguments[0], out red)
                && int.TryParse(arguments[1], out green)
                && int.TryParse(arguments[2], out blue))
            {
                return Color.FromArgb(red, green, blue);
            }
            else
            {
                return null;
            }
        }
    }
}
