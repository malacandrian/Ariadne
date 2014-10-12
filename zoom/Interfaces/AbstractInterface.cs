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
        public PImage Icon { get; protected set; }

        public static readonly int MinWidth = 320;
        public static readonly int TextHeight = 60;
        public static readonly int IconSize = TextHeight;
        public static readonly int Padding = 20;

        protected TextEntryHandler Handler;

        public AbstractInterface(PImage icon)
        {
            Background = PPath.CreateRectangle(0, 0, MinWidth + IconSize + (Padding * 2), 100);
            Entry = new PText();
            Entry.ConstrainHeightToTextHeight = false;
            //Entry.ConstrainWidthToTextWidth = false;
            Entry.Bounds = new RectangleF(Padding, Padding, 0, TextHeight);
            Entry.Font = new Font("Century Gothic", 32);
            AddChild(Background);
            AddChild(Entry);

            Icon = icon;
            Icon.Bounds = new RectangleF(MinWidth + Padding, Padding, IconSize, IconSize);
            AddChild(Icon);

            Handler = new TextEntryHandler(this);
            Entry.AddInputEventListener(Handler);
        }

        public abstract void Release(object sender, PInputEventArgs e);
        public abstract void Activate(object sender, PInputEventArgs e);
        public abstract void RegisterActivateButtonPress(object sender, PInputEventArgs e);
        public abstract bool Accepts(PInputEventArgs e);

        protected static bool MatchKeys(Keys expected, Keys actual)
        {
            return (expected & actual) == expected && (expected ^ actual) == 0;
        }
    }

    public class TextEntryHandler : PBasicInputEventHandler
    {
        public delegate void WidthUpdateHandler();
        public WidthUpdateHandler UpdateWidth;

        public AbstractInterface Owner { get; protected set; }

        public TextEntryHandler(AbstractInterface o)
        {
            Owner = o;
            UpdateWidth += OnUpdateWidth;
        }

        public override bool DoesAcceptEvent(PInputEventArgs e)
        {
            return e.IsKeyEvent && base.DoesAcceptEvent(e);
        }

        public override void OnKeyDown(object sender, PInputEventArgs e)
        {
            base.OnKeyDown(sender, e);
            char c = (char)e.KeyCode;

            //Keydown seems to assume they're all caps
            //All lower case looks better than all caps
            //So we'll force them to lower case
            if (c >= 'A' && c <= 'Z')
            {
                c = (char)(c + 32);
            }

            //If it's a letter or a number, add it to the text
            if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
            {
                Owner.Entry.Text += c;
            }
            else
            {
                //Deal with control characters
                switch (c)
                {
                    case (char)Keys.Back: //Backspace
                        Owner.Entry.Text = Owner.Entry.Text.Substring(0, Owner.Entry.Text.Length - 1);
                        break;

                    case ' ': //Space
                        Owner.Entry.Text += ' ';
                        break;
                }
            }

            UpdateWidth();
        }

        protected void OnUpdateWidth()
        {
            float basicWidth = AbstractInterface.MinWidth;
            if (Owner.Entry.Width > AbstractInterface.MinWidth) { basicWidth = Owner.Entry.Width; }

            Owner.Background.Width = basicWidth + (AbstractInterface.Padding * 2) + AbstractInterface.IconSize;
            Owner.Icon.X = basicWidth + AbstractInterface.Padding;
        }
    }


}
