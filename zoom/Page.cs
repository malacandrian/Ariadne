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
    public class Page : PStyledText
    {
        public Document Doc { get; protected set; }
        public PPath Border { get; protected set; }
        public Page Prev { get; protected set; }
        public Page Next { get; protected set; }
        protected PPath Cursor { get; set; }
        private PCamera Camera;

        public static Page LastPage;
        public static readonly ReadOnlyDictionary<Keys, ShowInterfaceHandler> EscapeKeys
            = new ReadOnlyDictionary<Keys, ShowInterfaceHandler>(
                new Dictionary<Keys, ShowInterfaceHandler>()
                {
                    {Keys.F1, Window.CommandHandler},
                    {Keys.F2, Window.FindHandler},
                    {Keys.F3, Window.FindHandler}
                });

        public Window Window
        {
            get
            {
                return Doc.Window;
            }
        }

        public Page(int x, int y, char c, Document parent, Page pr, Page nx, PCamera camera) : base(c.ToString(), parent.Window)
        {
            parent.AddChild(this);

            Doc = parent;
            Prev = pr;
            Next = nx;

            Camera = camera;

            
            
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

        public override void OnMouseDown(PInputEventArgs e)
        {
            LastPage = this;
            base.OnMouseDown(e);
        }

        public override void OnGotFocus(PInputEventArgs e)
        {
            LastPage = this;
            base.OnGotFocus(e);
        }

        private void DrawBorder()
        {
            Border = PPath.CreateRectangle(X, Y, PageSize.A4.Width, PageSize.A4.Height);
            Border.Pen = Pens.Black;
            AddChild(Border);
        }

        void Model_KeyDown(object sender, KeyEventArgs e)
        {
            foreach (KeyValuePair<Keys, ShowInterfaceHandler> keyPair in EscapeKeys)
            {
                if (e.KeyCode == keyPair.Key)
                {
                    Active = false;
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
                if (Next == null) { Next = new Page((int)X, (int)(Y + Height + 10), ' ', Doc, this, null, Camera); }

                Next.AddRtfAt(Overflow.Rtf, 0);

                ClearOverflow();
            }
        }
    }
}
