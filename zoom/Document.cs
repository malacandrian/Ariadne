using System;
using System.Drawing;
using UMD.HCIL.Piccolo;
using UMD.HCIL.Piccolo.Nodes;
using UMD.HCIL.Piccolo.Util;

namespace zoom
{
    public class Document : PNode
    {
        public PText Text { get; protected set; }
        public override void OnKeyPress(UMD.HCIL.Piccolo.Event.PInputEventArgs e)
        {
            base.OnKeyPress(e);
            Text.Text += e.KeyChar;
        }

        public override void OnClick(UMD.HCIL.Piccolo.Event.PInputEventArgs e)
        {
            base.OnClick(e);
            e.InputManager.KeyboardFocus = this.ToPickPath(e.Camera,Bounds);
        }

        public Document(int x, int y, char c)
        {
            SetBounds(x, y, Page.A4.Width, Page.A4.Height);
            PPath border = PPath.CreateRectangle(x, y, Page.A4.Width, Page.A4.Height);
            border.Pen = Pens.Black;

            AddChild(border);

            Text = new PText("" + c);
            Text.ConstrainHeightToTextHeight = false;
            Text.ConstrainWidthToTextWidth = false;
            Text.SetBounds(x, y, Page.A4.Width, Page.A4.Height);
            AddChild(Text);

        }
    }
}