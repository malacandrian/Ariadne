using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMD.HCIL.Piccolo;
using UMD.HCIL.Piccolo.Nodes;
using UMD.HCIL.Piccolo.Event;
using System.Drawing;
using System.Windows.Forms;
using UMD.HCIL.Piccolo.Util;

namespace zoom.Interfaces
{
    public abstract class AbstractInterface : PNode
    {
        public PPath Background { get; protected set; }
        public PText Entry { get; protected set; }

        public AbstractInterface()
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

        public abstract void Release(object sender, PInputEventArgs e);
    }

    public class TextEntryHandler : PBasicInputEventHandler
    {
        public AbstractInterface Owner { get; protected set; }

        public TextEntryHandler(AbstractInterface o)
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

   
}
