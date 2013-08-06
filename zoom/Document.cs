using System;
using System.Collections.Generic;
using System.Drawing;
using UMD.HCIL.Piccolo;
using UMD.HCIL.Piccolo.Nodes;
using UMD.HCIL.PiccoloX.Nodes;
using UMD.HCIL.Piccolo.Event;
using System.Windows.Forms;


namespace zoom
{
    public class Document : PNode
    {
       public  List<Page> Pages {get; protected set;}


       public Document(int x, int y, char c)
       {
           Pages = new List<Page>();
           Page first = new Page(x, y, c, this, null, null);
           Pages.Add(first);
           AddChild(first);
           AddInputEventListener(new DocDragHandler());
           
           
       }
    }
    public class DocDragHandler : PBasicInputEventHandler 
    {

        public override void OnMouseDrag(object sender, PInputEventArgs e)
        {
            PNode aNode = (PNode)sender;
            SizeF delta = e.GetDeltaRelativeTo(aNode);
            aNode.TranslateBy(delta.Width, delta.Height);
            aNode.MoveToFront();
            
        }
        public override bool DoesAcceptEvent(PInputEventArgs e)
        {
            bool output = (base.DoesAcceptEvent(e) && (e.Button == (MouseButtons.Left | MouseButtons.Right)));
            e.Handled = output;
            return output;
            
        }

        

    }
}