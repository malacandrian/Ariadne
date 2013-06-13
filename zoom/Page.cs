using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMD.HCIL.Piccolo;
using UMD.HCIL.Piccolo.Nodes;
using UMD.HCIL.PiccoloX.Nodes;
using System.Drawing;

namespace zoom
{
    public class Page:PNode 
    {
        public PStyledText Text { get; protected set; }
        public Document Doc { get; protected set; }
        public Page Prev { get; protected set; }
        public Page Next { get; protected set; }
        public override void OnKeyDown(UMD.HCIL.Piccolo.Event.PInputEventArgs e)
        {
            base.OnKeyDown(e);
            Text.OnKeyDown(e);
        }
        public override void OnKeyPress(UMD.HCIL.Piccolo.Event.PInputEventArgs e)
        {
            base.OnKeyPress(e);
            Text.OnKeyPress(e);
        }
        
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

            Text.Reflow += new ReflowEvent(Reflow);

            Doc = parent;
            Prev = pr;
            Next = nx;

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
        //protected string Sweep(string running,int depth)
        //{
        //    running += Text.Text;
        //    if (Next == null )
        //    {
        //        if (depth > 0)
        //        {
        //            Doc.Pages.Remove(this);
        //            Doc.RemoveChild(this);
        //        }
               
        //    }
        //    else
        //    {
        //        running = Next.Sweep(running, depth + 1);
        //        Next = null;
                
        //    }
        //    return running;
        //}
    }
}
