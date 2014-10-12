using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UMD.HCIL.Piccolo;
using UMD.HCIL.Piccolo.Event;
using UMD.HCIL.PiccoloX.Nodes;

namespace zoom
{
    public class DocCreateHandler : PBasicInputEventHandler
    {
        public PointF lastPoint { get; protected set; }
        public PCanvas Owner { get; protected set; }

        public DocCreateHandler(PCanvas owner)
        {
            RectangleF curView = owner.Camera.Bounds;
            float x = curView.X + (curView.Width / 3);
            float y = curView.Y + (curView.Height / 3);
            lastPoint = new PointF(x, y);
            Owner = owner;
        }

        public override void OnMouseDown(object sender, PInputEventArgs e)
        {
            base.OnMouseDown(sender, e);
            lastPoint = e.Position;
            Page.LastPage = null;
            Owner.Focus();
            e.InputManager.KeyboardFocus = e.Path;
        }

        public override void OnKeyPress(object sender, PInputEventArgs e)
        {
            base.OnKeyPress(sender, e);
            //Only fire if it isn't a control character
            if (e.KeyChar >= ' ' && e.KeyChar <= '~')
            {
                int x = (int)lastPoint.X;
                int y = (int)lastPoint.Y;
                Document created = new Document(x, y, e.KeyChar, (Window)Owner.FindForm(), Owner.Camera);
                Owner.Layer.AddChild(created);
                PStyledText firstPage = created.Pages[0];
                e.InputManager.KeyboardFocus = firstPage.ToPickPath(e.Camera, firstPage.Bounds);
                SendKeys.Send(e.KeyChar.ToString());
            }
        }

        public override bool DoesAcceptEvent(PInputEventArgs e)
        {
            bool isLeftClick = e.IsMouseEvent && e.Button == MouseButtons.Left;
            return base.DoesAcceptEvent(e) && (isLeftClick || e.IsKeyPressEvent) && e.PickedNode.GetType() == (new PCamera().GetType());
        }
    }
}
