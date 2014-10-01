using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace zoom.Commands
{
    class SizeCommand : ICommand
    {
        public string Name { get { return "size";  } }

        public void Execute(UMD.HCIL.PiccoloX.Util.PStyledTextHelpers.Selection selection, string[] arguments)
        {
            int size;
            if (arguments.Length > 0 && int.TryParse(arguments[0],out size))
            {
                selection.Active = true;
                Font selectionFont = selection.Model.SelectionFont;
                Font newFont = new Font(selectionFont.FontFamily, size);
                selection.Model.SelectionFont = newFont;
            }
        }
    }
}
