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
using zoom.Interfaces;
using System.Collections.ObjectModel;
using UMD.HCIL.Piccolo.Util;

namespace zoom
{
    /// <summary>
    /// Page extends PStyledText with logic for pagination, a border, and hooks for using the interfaces
    /// </summary>
    public class Page : PStyledText
    {
        /// <summary>
        /// The Document the page belongs to
        /// </summary>
        public Document Document { get; protected set; }

        /// <summary>
        /// The border around the page
        /// </summary>
        public PPath Border { get; protected set; }

        /// <summary>
        /// The page before this one in the document
        /// </summary>
        public Page Prev { get; protected set; }

        /// <summary>
        /// The page after this one in the document
        /// </summary>
        public Page Next { get; protected set; }

        /// <summary>
        /// The primary camera linked to the page
        /// </summary>
        public PCamera Camera { get { return Window.Canvas.Camera; } }

        /// <summary>
        /// The page that currently has keyboard focus
        /// </summary>
        public static Page LastPage;

        /// <summary>
        /// The list of keys that cause an interface to launch, and the handler for launching that interface
        /// </summary>
        public static readonly ReadOnlyDictionary<Keys, ShowInterfaceHandler> EscapeKeys
            = new ReadOnlyDictionary<Keys, ShowInterfaceHandler>(
                new Dictionary<Keys, ShowInterfaceHandler>()
                {
                    {Keys.F1, Window.CommandHandler},
                    {Keys.F2, Window.FindHandler},
                    {Keys.F3, Window.FindHandler}
                });

        /// <summary>
        /// The Window this page is attached to
        /// </summary>
        public Window Window { get { return Document.Window; } }

        /// <summary>
        /// Create a new Page
        /// </summary>
        /// <param Name="x">The x coordinate of the top-left corner of the page</param>
        /// <param Name="y">The y coordinate of the top-left corner of the page</param>
        /// <param Name="c">The character the page is initialised with</param>
        /// <param Name="document">The document the page is attached to</param>
        /// <param Name="pr">The previous page in the document</param>
        /// <param Name="nx">The next page in the document</param>
        public Page(float x, float y, char c, Document document, Page pr, Page nx)
            : base(c.ToString(), document.Window)
        {
            Document = document;
            Prev = pr;
            Next = nx;

            //Set the size of the page
            ConstrainHeightToTextHeight = false;
            ConstrainWidthToTextWidth = false;
            Bounds = new RectangleF(x, y, PageSize.A4.Width, PageSize.A4.Height);
            DrawBorder();

            //Add event listeners
            AddInputEventListener(Window.CommandHandler);
            AddInputEventListener(Window.FindHandler);

            ConfirmSelection += Window.ConfirmSelection;
            Reflow += OnReflow;

            Model.KeyDown += Model_KeyDown;
        }

        /// <summary>
        /// When the user clicks, record this as the page with focus
        /// </summary>
        /// <param Name="e"></param>
        public override void OnMouseDown(PInputEventArgs e)
        {
            LastPage = this;
            base.OnMouseDown(e);
        }

        /// <summary>
        /// When keyboard focus is assigned to this page, record this as the page with focus
        /// </summary>
        /// <param Name="e"></param>
        public override void OnGotFocus(PInputEventArgs e)
        {
            LastPage = this;
            base.OnGotFocus(e);
        }

        /// <summary>
        /// Draw around the outside of the page
        /// </summary>
        private void DrawBorder()
        {
            //Border = PPath.CreateRectangle(X, Y, PageSize.A4.Width, PageSize.A4.Height);
            Border = PPath.CreateRectangle(X, Y, Width, Height);
            Border.Pen = Pens.Black;
            AddChild(Border);
        }

        /// <summary>
        /// When the model recieves keyboard input, check if it was an escape key
        /// And launch the appropriate interface if it was
        /// </summary>
        /// <param Name="sender"></param>
        /// <param Name="e"></param>
        void Model_KeyDown(object sender, KeyEventArgs e)
        {
            //Check each escape key
            foreach (KeyValuePair<Keys, ShowInterfaceHandler> keyPair in EscapeKeys)
            {
                //If the key press is the escape key
                if (e.KeyCode == keyPair.Key)
                {
                    //Lose keyboard focus
                    Active = false;

                    //Fire the interface
                    PInputEventArgs eventArgs = new PInputEventArgs(Window.Canvas.Root.DefaultInputManager, e, PInputType.KeyDown);
                    eventArgs.Path = new PPickPath(Camera, this.Bounds);
                    eventArgs.Path.PushNode(Camera);
                    eventArgs.Path.PushNode(this);

                    keyPair.Value.OnKeyDown(this, eventArgs);
                }
            }
        }

        /// <summary>
        /// When the layout updates, move any overflow to the next page and ensure the border still exists
        /// </summary>
        protected void OnReflow()
        {
            //Ensure the border still exists
            if (IndexOfChild(Border) < 0)
            {
                AddChild(Border);
                Border.MoveToBack();
            }

            //Move any overflow to the next page
            if (Overflow != null)
            {
                //Create a new page if required
                if (Next == null) 
                { 
                    Next = new Page((int)X, (int)(Y + Height + 10), ' ', Document, this, null);
                    Document.AddChild(Next);
                }

                //Move the overflow to the next page
                Next.AddRtfAt(Overflow.Rtf, 0);
                ClearOverflow();
            }
        }
    }
}
