using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMD.HCIL.Piccolo;
using UMD.HCIL.Piccolo.Nodes;
using UMD.HCIL.PiccoloX.Util.PStyledTextHelpers;

namespace zoom.Command
{
    /// <summary>
    /// The ICommand interface provides the method signatures required for a command 
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// The Name of the command - the user needs to type this to execute the command
        /// </summary>
        string Name { get; }

        /// <summary>
        /// What to do when the user releases the command key
        /// typically this will modify the text of the selection
        /// </summary>
        /// <param Name="selection">The current selection, used as input, and modified for the output</param>
        /// <param Name="argument">Anything added after the command Name, used as input</param>
        void Execute(Selection selection, string[] arguments);

        /// <summary>
        /// A preview of what will happen if the user releases the command key
        /// </summary>
        /// <param Name="selection">The current selection, used as input</param>
        /// <param Name="argument">Anything added after the command Name, used as input</param>
        /// <returns>A PText of what would happen to the selection, should the user release the key</returns>
        PText Preview(Selection selection, string[] arguments);
    }
}
