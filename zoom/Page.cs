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

            SetBounds(x, y, PageSize.A4.Width, PageSize.A4.Height);
            Border = PPath.CreateRectangle(x, y, PageSize.A4.Width, PageSize.A4.Height);
            Border.Pen = Pens.Black;

            AddChild(Border);

            ConstrainHeightToTextHeight = false;
            ConstrainWidthToTextWidth = false;
            Bounds = new RectangleF(x, y, PageSize.A4.Width, PageSize.A4.Height);        

            AddInputEventListener(Window.CommandHandler);
            AddInputEventListener(Window.FindHandler);

            ConfirmSelection += Window.ConfirmSelection;
            Reflow += OnReflow;

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
