using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace zoom.Commands
{
    class StyleCommand : ICommand
    {
        private Dictionary<string, Style> styles = new Dictionary<string, Style>();
        public ReadOnlyDictionary<string, Style> Styles
        {
            get
            {
                return new ReadOnlyDictionary<string, Style>(styles);
            }
        }

        public string Name { get { return "style"; } }

        public StyleCommand()
        {
            styles.Add("title", new Style(new Font("Century Gothic", 22), Color.FromArgb(128, 0, 0)));
            styles.Add("h1", new Style(new Font("Century Gothic", 16), Color.FromArgb(128, 0, 0)));
            styles.Add("h2", new Style(new Font("Century Gothic", 13), Color.FromArgb(128, 0, 0)));
            styles.Add("h3", new Style(new Font("Century Gothic", 11, FontStyle.Bold), Color.FromArgb(128, 0, 0)));
            styles.Add("normal", new Style(new Font("Century Gothic", 11), Color.FromArgb(0, 0, 0)));
        }

        public void Execute(UMD.HCIL.PiccoloX.Util.PStyledTextHelpers.Selection selection, string[] arguments)
        {
            if (arguments.Length > 0 && styles.ContainsKey(arguments[0]))
            {
                selection.Active = true;
                selection.Text.SelectionFont = styles[arguments[0]].Font;
                selection.Text.SelectionColor = styles[arguments[0]].Color;
            }
        }
    }
}
