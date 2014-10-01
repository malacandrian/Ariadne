using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace zoom.Commands
{
    public class BoldCommand : ICommand
    {

        string ICommand.Name
        {
            get { return "bold"; }
        }

        public BoldCommand()
        {
        }

        void ICommand.Execute(UMD.HCIL.PiccoloX.Util.PStyledTextHelpers.Selection selection, string[] arguments)
        {
            if (selection != null)
            {
                selection.Active = true;
                Font font = selection.Model.SelectionFont;
                FontStyle style = font.Style;
                if (font.Style.HasFlag(FontStyle.Bold))
                {
                    style &= ~FontStyle.Bold;
                }
                else
                {
                    style |= FontStyle.Bold;
                }
                selection.Model.SelectionFont = new Font(font, style);
            }
        }

    }
}
