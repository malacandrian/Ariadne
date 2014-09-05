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


namespace zoom
{
    public class CommandInterface : PNode
    {
        public PPath Background { get; protected set; }
        public PText Entry { get; protected set; }

        private List<ICommand> commands = new List<ICommand>();
        public ICommand[] Commands
        {
            get
            {
                return commands.ToArray();
            }
        }

        public CommandInterface()
        {
            Background = PPath.CreateRectangle(0, 0, 400, 100);
            Entry = new PText();
            Entry.ConstrainHeightToTextHeight = false;
            Entry.ConstrainWidthToTextWidth = false;
            Entry.Bounds = new RectangleF(20, 20, 360, 60);
            Entry.Font = new Font("Century Gothic", 32);
            AddChild(Background);
            AddChild(Entry);

            Entry.AddInputEventListener(new TextEntryHandler(this));

            commands.Add(new BoldCommand());
            commands.Add(new ItalicCommand());
        }
    }

    public class TextEntryHandler : PBasicInputEventHandler
    {
        public CommandInterface Owner { get; protected set; }

        public TextEntryHandler(CommandInterface o)
        {
            Owner = o;
        }

        public override bool DoesAcceptEvent(PInputEventArgs e)
        {
            return e.IsKeyEvent && e.KeyCode != System.Windows.Forms.Keys.CapsLock && base.DoesAcceptEvent(e);
        }

        public override void OnKeyDown(object sender, PInputEventArgs e)
        {
            base.OnKeyDown(sender, e);
            byte c = (byte)e.KeyCode;
            if (c >= 65 && c <= 90)
            {
                c += 32;
            }
            Owner.Entry.Text += (char)c;
        }
    }

    public class ShowCommandHandler : PBasicInputEventHandler
    {

        public PCamera Camera { get; protected set; }
        public CommandInterface Command { get; protected set; }
        public bool IsPressed { get; protected set; }
        private PPickPath keyFocus;

        public ShowCommandHandler(PCamera c)
        {
            Camera = c;
            IsPressed = false;
        }

        public override bool DoesAcceptEvent(PInputEventArgs e)
        {
            if (e.IsKeyEvent && e.KeyCode == Keys.ControlKey && base.DoesAcceptEvent(e))
            {
                e.Handled = true;
                return true;
            }
            return false;
        }

        public override void OnKeyDown(object sender, PInputEventArgs e)
        {
            if (!IsPressed)
            {
                base.OnKeyDown(sender, e);
                Command = new CommandInterface();
                Camera.AddChild(Command);
                keyFocus = e.InputManager.KeyboardFocus;
                e.InputManager.KeyboardFocus = Command.Entry.ToPickPath(e.Camera, Command.Entry.Bounds);
                Command.Entry.AddInputEventListener(this);
            }
            IsPressed = true;
        }

        public override void OnKeyUp(object sender, PInputEventArgs e)
        {
            base.OnKeyUp(sender, e);
            //Attempt to execute a command
            ICommand c = Command.Commands.FirstOrDefault(a => a.Name == Command.Entry.Text);
            if (c != null)
            {
                c.Execute(((Window)Camera.Canvas.FindForm()).Selection, "");
            }
            //Remove Command Interface
            Camera.RemoveChild(Command);
            e.InputManager.KeyboardFocus = keyFocus;
            Command = null;
            IsPressed = false;
        }
    }
}
