using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using UMD.HCIL.Piccolo;
using UMD.HCIL.PiccoloX.Util.PStyledTextHelpers;
using UMD.HCIL.Piccolo.Nodes;

namespace zoom.Commands
{
    public class ToggleStyleCommand : AbstractStyleCommand
    {

        public override string Name
        {
            get { return _name; }
        }

        protected readonly string _name;
        public readonly FontStyle Style;

        public ToggleStyleCommand(string name, FontStyle style)
        {
            _name = name;
            Style = style;
        }

        public override void Execute(Selection selection, string[] arguments)
        {
            ToggleAndApply(selection, Style);
        }


        public override PText Preview(Selection selection, string[] arguments)
        {
            if (selection == null) { return new PText("Error: Text must be selected for this command to work"); }

            Style newStyle = ToggleFontStyle(selection, Style);
            return PTextForPreview(newStyle, selection);
        }
    }
}
