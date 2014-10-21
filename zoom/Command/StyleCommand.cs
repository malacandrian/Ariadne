using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using UMD.HCIL.Piccolo.Nodes;
using UMD.HCIL.PiccoloX.Util.PStyledTextHelpers;

namespace zoom.Command
{
    /// <summary>
    /// Change the style of the text to a pre-specified, named style
    /// </summary>
    class StyleCommand : AbstractStyleCommand
    {
        /// <summary>
        /// The list of styles and their names
        /// </summary>
        public static readonly ReadOnlyDictionary<string, Style> Styles =
            new ReadOnlyDictionary<string, Style>(
                new Dictionary<string, Style>() 
                {
                    {"title", new Style(new Font("Century Gothic", 22), Color.FromArgb(128, 0, 0))},
                    {"h1", new Style(new Font("Century Gothic", 16), Color.FromArgb(128, 0, 0))},
                    {"h2", new Style(new Font("Century Gothic", 13), Color.FromArgb(128, 0, 0))},
                    {"h3", new Style(new Font("Century Gothic", 11, FontStyle.Bold), Color.FromArgb(128, 0, 0))},
                    {"normal", new Style(new Font("Century Gothic", 11), Color.FromArgb(0, 0, 0))}
                });

        /// <summary>
        /// The Name of the command - the user needs to type this to execute the command
        /// </summary>
        public override string Name { get { return "style"; } }

        /// <summary>
        /// Replace the style of the current text with the one the user specified
        /// </summary>
        /// <param Name="selection">The current selection</param>
        /// <param Name="arguments">The arguments provided to the command</param>
        public override void Execute(UMD.HCIL.PiccoloX.Util.PStyledTextHelpers.Selection selection, string[] arguments)
        {
            //If the selection exists, and the first argument is a style
            if (selection != null && arguments.Length > 0 && Styles.ContainsKey(arguments[0]))
            {
                //Apply that style to the text
                ApplyStyle(selection, Styles[arguments[0]]);
            }
        }

        /// <summary>
        /// Show what the selected text would look like with the specified style
        /// </summary>
        /// <param Name="selection">The current selection</param>
        /// <param Name="arguments">The arguments provided to the command</param>
        /// <returns>A PText containing the selection text, styled in the specified manner</returns>
        public override PText Preview(Selection selection, string[] arguments)
        {
            string styleName = arguments[0];
            //If the selection doesn't exist, abort
            if (selection == null) { return new PText("Error: Text must be selected for this command to work"); }
            //If no arguments were provided, abort
            if (arguments.Length == 0) { return new PText("Please specify a style"); }
            //If the style specified doesn't exist, abort
            if (!Styles.ContainsKey(styleName)) { return new PText(String.Format(@"Error: {0} is not a valid style", styleName)); }

            //Create and return a PText with the specified style
            return PTextForPreview(Styles[styleName], selection);
        }
    }
}
