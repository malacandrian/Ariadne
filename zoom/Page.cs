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

        /*public override void OnKeyDown(UMD.HCIL.Piccolo.Event.PInputEventArgs e)
        {
            base.OnKeyDown(e);
            Text.OnKeyDown(e);
        }
        public override void OnKeyPress(UMD.HCIL.Piccolo.Event.PInputEventArgs e)
        {
            base.OnKeyPress(e);
            Text.OnKeyPress(e);
        }
        */
        public override void OnClick(UMD.HCIL.Piccolo.Event.PInputEventArgs e)
        {
            base.OnClick(e);
            Text.OnClick(e);
            e.InputManager.KeyboardFocus = this.ToPickPath(e.Camera,Bounds);
        }

        public Page(int x, int y, char c, Document parent, Page pr, Page nx)
        {
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

            Text.Reflow += new ReflowEvent(Reflow);

            Doc = parent;
            Prev = pr;
            Next = nx;

            AddInputEventListener(new SelectEventHandler());

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
                    Next = new Page((int)X, (int)(Y + Height + 10), ' ', Doc, this, null);
                    Next.Text.Text = Text.OverFlow;
                    Doc.AddChild(Next);
                    Doc.Pages.Add(Next);
                }
                Text.ClearOverFlow();
            }
        }
    }
    public class SelectEventHandler : PDragSequenceEventHandler
    {
        protected PointF DragStart;

        public override bool DoesAcceptEvent(PInputEventArgs e)
        {
            if (base.DoesAcceptEvent(e))
            {
                if (!e.Handled && e.IsMouseEvent && e.Button == MouseButtons.Left)
                {
                    e.Handled = true;
                    return true;
                }
            }
            return false;
        }

        protected override void OnStartDrag(object sender, PInputEventArgs e)
        {
            base.OnStartDrag(sender, e);
            DragStart = e.Position;      
        }

        protected override void OnDrag(object sender, PInputEventArgs e)
        {
            base.OnDrag(sender, e);
            ((Page)sender).Text.Select(DragStart, e.Position);
        }

        protected override void OnEndDrag(object sender, PInputEventArgs e)
        {
            base.OnEndDrag(sender, e);
        }
    }

    public class PageTextHandler : TextEntryInputHandler
    {
        public PageTextHandler(PStyledText o)
            : base(o)
        {
        }

        /*public override bool DoesAcceptEvent(PInputEventArgs e)
        {
            return e.IsKeyEvent && e.KeyCode != Keys.ControlKey && base.DoesAcceptEvent(e);
        }
         */
    }
}
