using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using UMD.HCIL.Piccolo.Nodes;
using UMD.HCIL.PiccoloX.Util.PStyledTextHelpers;

namespace zoom.Commands
{
    class StyleCommand : AbstractStyleCommand
    {
        private Dictionary<string, Style> styles = new Dictionary<string, Style>();
        public ReadOnlyDictionary<string, Style> Styles
        {
            get
            {
                return new ReadOnlyDictionary<string, Style>(styles);
            }
        }

        public override string Name { get { return "style"; } }

        public StyleCommand()
        {
            styles.Add("title", new Style(new Font("Century Gothic", 22), Color.FromArgb(128, 0, 0)));
            styles.Add("h1", new Style(new Font("Century Gothic", 16), Color.FromArgb(128, 0, 0)));
            styles.Add("h2", new Style(new Font("Century Gothic", 13), Color.FromArgb(128, 0, 0)));
            styles.Add("h3", new Style(new Font("Century Gothic", 11, FontStyle.Bold), Color.FromArgb(128, 0, 0)));
            styles.Add("normal", new Style(new Font("Century Gothic", 11), Color.FromArgb(0, 0, 0)));
        }

        public override void Execute(UMD.HCIL.PiccoloX.Util.PStyledTextHelpers.Selection selection, string[] arguments)
        {
            if (selection != null && arguments.Length > 0 && styles.ContainsKey(arguments[0]))
            {
                ApplyStyle(selection, styles[arguments[0]]);
            }
        }

        public override PText Preview(Selection selection, string[] arguments)
        {
            string styleName = arguments[0];
            if (selection == null) { return new PText("Error: Text must be selected for this command to work"); }
            if (arguments.Length == 0) { return new PText("Please specify a style"); }
            if (!styles.ContainsKey(styleName)) { return new PText(String.Format(@"Error: {0} is not a valid style", styleName)); }

            return PTextForPreview(styles[styleName], selection);
        }
    }
}
