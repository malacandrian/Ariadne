using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace zoom.Commands
{
    public class ItalicCommand : ICommand
    {

        string ICommand.Name
        {
            get { return "italic"; }
        }

        public ItalicCommand()
        {
        }

        void ICommand.Execute(UMD.HCIL.PiccoloX.Util.PStyledTextHelpers.Selection selection, string arguments)
        {
            if (selection != null)
            {
                selection.Active = true;
                selection.Text.SelectionFont = new Font(selection.Text.Font, FontStyle.Italic);
            }
        }

    }
}
