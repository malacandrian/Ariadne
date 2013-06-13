using System;
using UMD.HCIL.Piccolo;
using UMD.HCIL.PiccoloX;
using UMD.HCIL.Piccolo.Event;
using System.Drawing;
using System.Windows.Forms;

namespace zoom
{
    public class Window : PForm
    {
        public override void Initialize()
        {
            WindowState = FormWindowState.Maximized;
            Canvas.AddInputEventListener(new DocCreateHandler(Canvas));
            Canvas.Root.DefaultInputManager.KeyboardFocus = Canvas.Camera.ToPickPath();
        }
    }

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

        public override void OnClick(object sender, PInputEventArgs e)
        {
            base.OnClick(sender, e);
            lastPoint = e.Position;
            e.InputManager.KeyboardFocus = e.Path;
        }

        public override void OnKeyPress(object sender, PInputEventArgs e)
        {
            base.OnKeyPress(sender, e);
            int x = (int)lastPoint.X;
            int y = (int)lastPoint.Y;
            Document created = new Document(x, y,e.KeyChar);
            Owner.Layer.AddChild(created);
            PNode firstPage = created.Pages[0];
            e.InputManager.KeyboardFocus = firstPage.ToPickPath(e.Camera,firstPage.Bounds);

        }

        public override bool DoesAcceptEvent(PInputEventArgs e)
        {
            bool isLeftClick = e.IsClickEvent && e.Button == MouseButtons.Left;
            return base.DoesAcceptEvent(e) && (isLeftClick || e.IsKeyPressEvent) && e.PickedNode.GetType() == (new PCamera().GetType());
        }
    }
}
