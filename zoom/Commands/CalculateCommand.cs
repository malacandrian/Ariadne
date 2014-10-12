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

namespace zoom.Commands
{
    class CalculateCommand : ICommand
    {
        public string Name { get { return "calculate"; } }
        public PCamera Camera { get; protected set; }

        public CalculateCommand(PCamera camera)
        {
            Camera = camera;
        }

        public void Execute(Selection selection, string[] arguments)
        {
            //string sum = selection.Text;
            //sum = sum.Trim();
            //try { selection.Text = new Expression(sum).Evaluate().ToString(); }
            //catch { new Error("Sum \"" + sum + "\" is not formatted correctly", Camera); }
            try { selection.Text = PerformCalculation(selection.Text); }
            catch { new Error(String.Format(@"Sum'{0}' is not formatted correctly", selection.Text), Camera); }
        }

        public PText Preview(Selection selection, string[] arguments)
        {
            PText output = new PText();
            output.Font = selection.Font;
            output.TextBrush = new SolidBrush(selection.Color);
            try { output.Text = PerformCalculation(selection.Text); }
            catch { output.Text = String.Format(@"Sum'{0}' is not formatted correctly", selection.Text); }

            return output;
        }

        protected string PerformCalculation(string text)
        {
            text = text.Trim();
            return new Expression(text).Evaluate().ToString();
        }
    }
}
