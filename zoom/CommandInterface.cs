using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMD.HCIL.Piccolo;
using UMD.HCIL.Piccolo.Nodes;
using System.Drawing;
using UMD.HCIL.Piccolo.Event;
using System.Windows.Forms;
using UMD.HCIL.PiccoloX.Nodes;


namespace zoom
{
    public class CommandInterface : PNode 
    {
        public PPath Background { get; protected set; }
        public PText Entry { get; protected set; }

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

        public ShowCommandHandler(PCamera c)
        {
            Camera = c;
            IsPressed = false;
        }

        public override bool DoesAcceptEvent(PInputEventArgs e)
        {
            if (base.DoesAcceptEvent(e) && e.IsKeyEvent && e.KeyCode == Keys.ControlKey)
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
                e.InputManager.KeyboardFocus = Command.Entry.ToPickPath(e.Camera, Command.Entry.Bounds);
                Command.Entry.AddInputEventListener(this);
            }
            IsPressed = true;
        }

        public override void OnKeyUp(object sender, PInputEventArgs e)
        {
            base.OnKeyUp(sender, e);
            Camera.RemoveChild(Command);
            e.InputManager.KeyboardFocus = Camera.ToPickPath();
            Command = null;
            IsPressed = false;
        }
    }
}
