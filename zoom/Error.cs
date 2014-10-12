using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using UMD.HCIL.Piccolo;
using UMD.HCIL.Piccolo.Activities;
using UMD.HCIL.Piccolo.Nodes;

namespace zoom
{
    class Error : PNode
    {
        public Error(string error, PCamera camera, int duration = 3000)
        {
            PPath background = PPath.CreateRectangle(0, 0, camera.ViewBounds.Width, camera.ViewBounds.Height);
            background.Brush = new SolidBrush(Color.FromArgb(30,220,220,220));
            
            AddChild(background);

            PText errorText = new PText(error);
            errorText.ConstrainWidthToTextWidth = false;
            errorText.Font = new Font("Century Gothic", 18);
            errorText.TextAlignment = StringAlignment.Center;

            float height = errorText.Font.Height;
            float width = camera.Canvas.FindForm().Width;
            float y = (camera.Canvas.FindForm().Height - height) / 2;
            errorText.Bounds = new RectangleF(0, y, width, height);

            AddChild(errorText);

            camera.AddChild(this);

            PActivity dissapear = new PActivity(duration);
            dissapear.ActivityFinished += activityFinished;
            camera.Canvas.Root.AddActivity(dissapear);
        }

        protected void activityFinished(PActivity activity)
        {
            closeMessage();
        }

        public void closeMessage()
        {
            RemoveFromParent();
        }

        public override void OnMouseDown(UMD.HCIL.Piccolo.Event.PInputEventArgs e)
        {
            base.OnMouseDown(e);
            closeMessage();
        }
    }
}
