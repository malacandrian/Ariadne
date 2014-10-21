using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NCalc;
using UMD.HCIL.PiccoloX.Util.PStyledTextHelpers;
using UMD.HCIL.Piccolo;
using UMD.HCIL.Piccolo.Nodes;
using System.Drawing;

namespace zoom.Command
{
    /// <summary>
    /// Parses the text of the selection, and attempts to execute it as a calculation
    /// </summary>
    class CalculateCommand : ICommand
    {
        /// <summary>
        /// The Name of the command - the user needs to type this to execute the command
        /// </summary>
        public string Name { get { return "calculate"; } }

        /// <summary>
        /// The PCamera object to attach errors to
        /// </summary>
        public PCamera Camera { get; protected set; }

        /// <summary>
        /// Create a new CalculateCommand with a given PCamera
        /// </summary>
        /// <param Name="camera"></param>
        public CalculateCommand(PCamera camera) { Camera = camera; }

        /// <summary>
        /// Replace the text of the selection with the answer to the calculation
        /// </summary>
        /// <param Name="selection">The current selection, used as input, and modified for the output</param>
        /// <param Name="argument">Anything added after the command Name, used as input</param>
        public void Execute(Selection selection, string[] arguments)
        {
            //Attempt to perform the calculation
            try { selection.Text = PerformCalculation(selection.Text); }
            //If it fails, raise an error to the user
            catch { new Error(String.Format(@"Sum'{0}' is not formatted correctly", selection.Text), Camera); }
        }

        /// <summary>
        /// Send the solution to the equation to the preview box
        /// </summary>
        /// <param Name="selection">The current selection, used as input, and modified for the output</param>
        /// <param Name="argument">Anything added after the command Name, used as input</param>
        /// <returns>The answer to the equation</returns>
        public PText Preview(Selection selection, string[] arguments)
        {
            PText output = new PText();
            //Format the text how it would appear
            output.Font = selection.Font;
            output.TextBrush = new SolidBrush(selection.Color);

            //Attempt to perform the calcuation
            try { output.Text = PerformCalculation(selection.Text); }
            //If it fails, send an error to the preview box
            catch { output.Text = String.Format(@"Sum'{0}' is not formatted correctly", selection.Text); }

            return output;
        }

        /// <summary>
        /// Parse a string and attempt to execute it as a calculation
        /// </summary>
        /// <remarks>If the calculation is poorly formed, it throws an exception</remarks>
        /// <param Name="text">The string to parse</param>
        /// <returns>The result of the calculation</returns>
        protected string PerformCalculation(string text)
        {
            text = text.Trim();
            return new Expression(text).Evaluate().ToString();
        }
    }
}
