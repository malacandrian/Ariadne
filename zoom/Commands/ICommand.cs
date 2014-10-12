using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMD.HCIL.Piccolo;
using UMD.HCIL.Piccolo.Nodes;
using UMD.HCIL.PiccoloX.Util.PStyledTextHelpers;

namespace zoom.Commands
{
    public interface ICommand
    {
        string Name { get; }

        void Execute(Selection selection, string[] arguments);

        PText Preview(Selection selection, string[] arguments);
    }
}
