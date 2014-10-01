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

        void ICommand.Execute(UMD.HCIL.PiccoloX.Util.PStyledTextHelpers.Selection selection, string[] arguments)
        {
            if (selection != null)
            {
                selection.Active = true;
                Font font = selection.Model.SelectionFont;
                FontStyle style = font.Style;
                if (font.Style.HasFlag(FontStyle.Italic))
                {
                    style &= ~FontStyle.Italic;
                }
                else
                {
                    style |= FontStyle.Italic;
                }
                selection.Model.SelectionFont = new Font(font, style);
            }
        }

    }
}
