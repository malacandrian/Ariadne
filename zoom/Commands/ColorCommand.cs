using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace zoom.Commands
{
    class ColorCommand : ICommand
    {
        public string Name { get { return "color"; } }

        public void Execute(UMD.HCIL.PiccoloX.Util.PStyledTextHelpers.Selection selection, string[] arguments)
        {
            Color color = Color.FromArgb(0,0,0);

            if (arguments.Length == 0) //If no color is supplied
            {
                return;
            }
            else if (arguments.Length == 1) //Name or Hex code
            {
                if (arguments[0][0].Equals('#')) // If it's a Hex code
                {
                    int red, green, blue;
                    if (arguments[0].Length == 7
                        && int.TryParse(arguments[0].Substring(1, 2), out red)
                        && int.TryParse(arguments[0].Substring(3, 2), out green)
                        && int.TryParse(arguments[0].Substring(5, 2), out blue))
                    {
                        color = Color.FromArgb(red, green, blue);
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    //It might be a name
                    string enteredName = arguments[0];
                    enteredName = char.ToUpper(enteredName[0]) + enteredName.Substring(1);

                    KnownColor colorName;
                    if (Enum.TryParse<KnownColor>(enteredName, out colorName))
                    {
                        color = Color.FromKnownColor(colorName);
                    }
                    else
                    {
                        return;
                    }
                }
            }
            else if (arguments.Length == 3) //RGB values
            {
                int red, green, blue;
                if (int.TryParse(arguments[0], out red)
                    && int.TryParse(arguments[1], out green)
                    && int.TryParse(arguments[2], out blue))
                {
                    color = Color.FromArgb(red, green, blue);
                }
                else
                {
                    return;
                }
            }

            selection.Active = true;
            selection.Model.SelectionColor = color;
            return;
        }
    }
}
