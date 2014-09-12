using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMD.HCIL.PiccoloX.Util.PStyledTextHelpers;

namespace zoom.Commands
{
    public interface ICommand
    {
        string Name { get; }

        void Execute(Selection selection, string[] arguments);
    }
}
