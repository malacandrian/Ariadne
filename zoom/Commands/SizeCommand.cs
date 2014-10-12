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
    class SizeCommand : AbstractStyleCommand
    {
        public override string Name { get { return "size"; } }

        public override void Execute(Selection selection, string[] arguments)
        {
            int size;
            if (selection != null && arguments.Length > 0 && int.TryParse(arguments[0], out size))
            {
                MergeAndApply(selection, size: size);
            }
        }

        public override PText Preview(Selection selection, string[] arguments)
        {
            int size;

            if (selection == null) { return new PText("Error: Text must be selected for this command to work"); }

            if (arguments.Length == 0) { return new PText("Please enter a size for the text"); }

            if (!int.TryParse(arguments[0], out size)) { return new PText(String.Format("Error: {0} is not a valid size", arguments[0])); }

            Style newStyle = MergeStyles(GetStyle(selection), size: size);
            return PTextForPreview(newStyle, selection);
        }
    }
}
