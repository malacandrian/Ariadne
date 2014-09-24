using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMD.HCIL.Piccolo;
using UMD.HCIL.Piccolo.Nodes;
using UMD.HCIL.Piccolo.Util;
using System.Drawing;
using UMD.HCIL.Piccolo.Event;
using System.Windows.Forms;
using UMD.HCIL.PiccoloX.Nodes;
using System.Runtime.InteropServices;
using zoom.Commands;


namespace zoom.Interfaces
{
    public class CommandInterface : AbstractInterface
    {
        private List<ICommand> commands = new List<ICommand>();
        public PCamera Camera { get; protected set; }

        public ICommand[] Commands
        {
            get
            {
                return commands.ToArray();
            }
        }

        public CommandInterface(PCamera camera) : base(new PImage(Properties.Resources.Gear))
        {
            Camera = camera;

            commands.Add(new BoldCommand());
            commands.Add(new ItalicCommand());
            commands.Add(new StyleCommand());
            commands.Add(new SizeCommand());
            commands.Add(new ColorCommand());
        }

        public override void Release(object sender, PInputEventArgs e)
        {
            //Check if the user entered anything
            if (Entry.Text != null)
            {
                //Attempt to execute a command
                string[] parts = Entry.Text.Split(' ');
                string commandName = parts[0];

                string[] args = parts.Skip(1).ToArray();

                ICommand c = Commands.FirstOrDefault(a => a.Name == commandName);
                if (c != null)
                {
                    c.Execute(((Window)Camera.Canvas.FindForm()).Selection, args);
                }

            }
        }

        public override void Press(object sender, PInputEventArgs e)
        {
            //Do Nothing
        }

        public override bool Accepts(PInputEventArgs e)
        {
            return e.IsKeyEvent && MatchKeys(Keys.F1, e.KeyData);
        }
    }
}
