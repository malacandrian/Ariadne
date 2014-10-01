using System;
using System.Collections.Generic;
using System.Drawing;
using UMD.HCIL.Piccolo;
using UMD.HCIL.Piccolo.Nodes;
using UMD.HCIL.PiccoloX.Nodes;
using UMD.HCIL.Piccolo.Util;
using UMD.HCIL.Piccolo.Event;
using System.Windows.Forms;


namespace zoom
{
    public class Document : PNode
    {

        public Window Window { get; protected set; }

        public Page[] Pages
        {
            get
            {
                List<Page> output = new List<Page>();
                PNodeList children = ChildrenReference;
                foreach (PNode node in children)
                {
                    if (node is Page)
                    {
                        output.Add((Page)node);
                    }
                }
                return output.ToArray();
            }
        }


       public Document(int x, int y, char c, Window w, PCamera camera)
       {
           Window = w;
           Page first = new Page(x, y, c, this, null, null, camera);
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