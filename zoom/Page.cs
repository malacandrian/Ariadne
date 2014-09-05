using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMD.HCIL.Piccolo;
using UMD.HCIL.Piccolo.Nodes;
using UMD.HCIL.PiccoloX.Nodes;
using System.Drawing;
using UMD.HCIL.PiccoloX.Util.PStyledTextHelpers;
using System.Drawing.Drawing2D;
using UMD.HCIL.Piccolo.Event;
using System.Windows.Forms;

namespace zoom
{
    public class Page:PNode 
    {
        public PStyledText Text { get; protected set; }
        public Document Doc { get; protected set; }
        public Page Prev { get; protected set; }
        public Page Next { get; protected set; }
        protected PPath Cursor { get; set; }
        private PCamera Camera;

        public Window Window
        {
            get
            {
                return Doc.Window;
            }
        }

        public override void OnClick(UMD.HCIL.Piccolo.Event.PInputEventArgs e)
        {
            base.OnClick(e);
            Text.OnClick(e);
            e.InputManager.KeyboardFocus = Text.ToPickPath(e.Camera,Bounds);
        }

        public Page(int x, int y, char c, Document parent, Page pr, Page nx, PCamera camera)
        {
            parent.AddChild(this);

            Doc = parent;
            Prev = pr;
            Next = nx;

            Camera = camera;

            SetBounds(x, y, PageSize.A4.Width, PageSize.A4.Height);
            PPath border = PPath.CreateRectangle(x, y, PageSize.A4.Width, PageSize.A4.Height);
            border.Pen = Pens.Black;

            AddChild(border);

            Text = new PStyledText("" + c);
            Text.ConstrainHeightToTextHeight = false;
            Text.ConstrainWidthToTextWidth = false;
            Text.Bounds = new RectangleF(x, y, PageSize.A4.Width, PageSize.A4.Height);        
            AddChild(Text);

            Text.RemoveInputEventListener(Text.DefaultHandler);
            Text.AddInputEventListener(new PageTextHandler(Text));
            Text.AddInputEventListener(new ShowCommandHandler(Camera));

            Text.ConfirmSelection += Window.ConfirmSelection;

            Text.Reflow += new ReflowEvent(Reflow);

            

        }



        protected void Reflow()
        {
            if (Text.OverFlow != "")
            {
                if (Next != null)
                {
                    Next.Text.Text = Text.OverFlow + Next.Text.Text;

                }
                else
                {
                    Next = new Page((int)X, (int)(Y + Height + 10), ' ', Doc, this, null, Camera);
                    Next.Text.Text = Text.OverFlow;
                    Doc.AddChild(Next);
                }
                Text.ClearOverFlow();
            }
        }
    }

    public class PageTextHandler : TextEntryInputHandler
    {
        public PageTextHandler(PStyledText o)
            : base(o)
        {
        }

        public override bool DoesAcceptEvent(PInputEventArgs e)
        {
            return ((e.IsKeyEvent && e.KeyCode != Keys.ControlKey) || e.IsKeyPressEvent) && base.DoesAcceptEvent(e);
        }

        public override void OnKeyDown(object sender, PInputEventArgs e)
        {
            base.OnKeyDown(sender, e);
        }

        public override void OnKeyPress(object sender, PInputEventArgs e)
        {
            base.OnKeyPress(sender, e);
        }
    }
}
