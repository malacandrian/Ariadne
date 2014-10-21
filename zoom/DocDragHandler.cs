using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UMD.HCIL.Piccolo;
using UMD.HCIL.Piccolo.Event;

namespace zoom
{
    /// <summary>
    /// DocDragHandler provides the event methods that allow a user to move the document around the plane
    /// </summary>
    public class DocDragHandler : PBasicInputEventHandler
    {
        /// <summary>
        /// When the user drags with both mouse actual, move the document
        /// </summary>
        public override void OnMouseDrag(object sender, PInputEventArgs e)
        {
            PNode aNode = (PNode)sender;
            SizeF delta = e.GetDeltaRelativeTo(aNode);
            aNode.TranslateBy(delta.Width, delta.Height);
            aNode.MoveToFront();

        }

        /// <summary>
        /// Accept all drag events performed with both mouse actual
        /// </summary>
        /// <param Name="e">The event to test</param>
        /// <returns>Whether the event is accepted</returns>
        public override bool DoesAcceptEvent(PInputEventArgs e)
        {
            bool output = (base.DoesAcceptEvent(e) && (e.Button == (MouseButtons.Left | MouseButtons.Right)));
            e.Handled = output;
            return output;

        }
    }
}
