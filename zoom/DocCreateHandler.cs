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
    /// <summary>
    /// DocCreateHandler handles all the input for the automatic generation of documents 
    /// when the user starts typing to the background
    /// </summary>
    public class DocCreateHandler : PBasicInputEventHandler
    {
        /// <summary>
        /// The last location on the background that the user clicked
        /// </summary>
        public PointF LastPoint { get; protected set; }

        /// <summary>
        /// The canvas that the documents are created on
        /// </summary>
        public PCanvas Owner { get; protected set; }

        /// <summary>
        /// Create a new DocCreateHandler
        /// </summary>
        /// <param Name="owner">The canvas that the documents are created on</param>
        public DocCreateHandler(PCanvas owner)
        {
            //Initialise such that if the user starts typing right away
            //The document will be created in the middle of their current view
            RectangleF curView = owner.Camera.Bounds;
            float x = curView.X + (curView.Width / 3);
            float y = curView.Y + (curView.Height / 3);
            LastPoint = new PointF(x, y);
            Owner = owner;
        }

        /// <summary>
        /// When the user clicks on the background, move the focus to the background
        /// And update the last clicked location
        /// </summary>
        public override void OnMouseDown(object sender, PInputEventArgs e)
        {
            //Move the last clicked location
            base.OnMouseDown(sender, e);
            LastPoint = e.Position;
            Page.LastPage = null;

            //Move the keyboard focus to the background
            Owner.Focus();
            e.InputManager.KeyboardFocus = e.Path;
        }

        /// <summary>
        /// When the user types, create a new document and put the input inside of it
        /// </summary>
        public override void OnKeyPress(object sender, PInputEventArgs e)
        {
            base.OnKeyPress(sender, e);
            //Only fire if it isn't a control character
            if (e.KeyChar >= ' ' && e.KeyChar <= '~')
            {
                //Create the document in the last clicked location
                float x = LastPoint.X;
                float y = LastPoint.Y;
                Document created = new Document(x, y, e.KeyChar, (Window)Owner.FindForm());
                Owner.Layer.AddChild(created);

                //Shift the keyboard focus to the newly created document
                PStyledText firstPage = created.Pages[0];
                e.InputManager.KeyboardFocus = firstPage.ToPickPath(e.Camera, firstPage.Bounds);
            }
        }

        /// <summary>
        /// Accept left clicks and all key events sent to the camera
        /// </summary>
        /// <param Name="e">The event it's deciding whether to accept</param>
        /// <returns>Whether the event is accepted</returns>
        public override bool DoesAcceptEvent(PInputEventArgs e)
        {
            bool isLeftClick = e.IsMouseEvent && e.Button == MouseButtons.Left;
            return base.DoesAcceptEvent(e) && (isLeftClick || e.IsKeyPressEvent) && e.PickedNode is PCamera;
        }
    }
}
