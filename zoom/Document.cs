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
    /// <summary>
    /// A Document is a set of pages that overflow to each other, and can be moved as one
    /// </summary>
    public class Document : PNode
    {
        /// <summary>
        /// The Window the document is attached to
        /// </summary>
        public Window Window { get; protected set; }

        /// <summary>
        /// The list of pages owned by this Document
        /// </summary>
        public Page[] Pages
        {
            get
            {
                //Filter all children, adding any Pages to the list
                List<Page> output = new List<Page>();
                PNodeList children = ChildrenReference;
                foreach (PNode node in children)
                {
                    if (node is Page) { output.Add((Page)node); }
                }
                return output.ToArray();
            }
        }

        /// <summary>
        /// Create a new document
        /// </summary>
        /// <param Name="x">The x coordinate of the top-left corner of the document</param>
        /// <param Name="y">The y coordinate of the top-left corner of the document</param>
        /// <param Name="c">The first character of the document</param>
        /// <param Name="window">The window the document is attached to</param>
        public Document(float x, float y, char c, Window window)
        {
            Window = window;
            AddChild(new Page(x, y, c, this, null, null));
            AddInputEventListener(new DocDragHandler());
        }
    }
}