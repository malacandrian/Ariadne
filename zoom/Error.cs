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
    /// <summary>
    /// Error is a humane alternative to modal alert boxes.
    /// 
    /// Where the alert box captures user focus, and requires input before it dissapears,
    /// despite not having any meaningful decisions, making it 0% efficient,
    /// Error doesn't capture focus, and dissapears of its own accord
    /// </summary>
    class Error : PNode
    {
        /// <summary>
        /// Create and show a new error
        /// </summary>
        /// <param Name="error">The text to display to the user</param>
        /// <param Name="camera">The camera to display the error on</param>
        /// <param Name="duration">How long to display the error for</param>
        public Error(string error, PCamera camera, int duration = 3000)
        {
            //Slightly shade the background to bring attention to the error
            PPath background = PPath.CreateRectangle(0, 0, camera.ViewBounds.Width, camera.ViewBounds.Height);
            background.Brush = new SolidBrush(Color.FromArgb(30, 220, 220, 220));
            AddChild(background);

            //Add the error text to the center of the screen
            PText errorText = new PText(error);
            errorText.ConstrainWidthToTextWidth = false;
            errorText.Font = new Font("Century Gothic", 18);
            errorText.TextAlignment = StringAlignment.Center;

            float height = errorText.Font.Height;
            float width = camera.Canvas.FindForm().Width;
            float y = (camera.Canvas.FindForm().Height - height) / 2;
            errorText.Bounds = new RectangleF(0, y, width, height);

            AddChild(errorText);

            //Display the error
            camera.AddChild(this);

            //Remove the error after the required time
            PActivity dissapear = new PActivity(duration);
            dissapear.ActivityFinished += activityFinished;
            camera.Canvas.Root.AddActivity(dissapear);
        }

        /// <summary>
        /// Once the time has expired, automatically remove the error
        /// </summary>
        protected void activityFinished(PActivity activity) { closeMessage(); }

        /// <summary>
        /// Remove the error
        /// </summary>
        public void closeMessage() { RemoveFromParent(); }

        /// <summary>
        /// If the user clicks anywhere, remove the error
        /// </summary>
        public override void OnMouseDown(UMD.HCIL.Piccolo.Event.PInputEventArgs e)
        {
            base.OnMouseDown(e);
            closeMessage();
        }
    }
}
