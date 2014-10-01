using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMD.HCIL.Piccolo;
using UMD.HCIL.Piccolo.Util;
using UMD.HCIL.PiccoloX.Nodes;
using UMD.HCIL.PiccoloX.Util.PStyledTextHelpers;
using System.Windows.Forms;
using UMD.HCIL.Piccolo.Nodes;
using System.Threading;
using UMD.HCIL.Piccolo.Activities;

namespace zoom.Interfaces
{
    class FindInterface : AbstractInterface
    {
        public Window Window { get; protected set; }
        public Document StartDoc { get; protected set; }
        public Page StartPage { get; protected set; }
        public int StartLoc { get; protected set; }
        public PText DirectionText { get; protected set; }

        public Page FoundPage { get; protected set; }

        public FindInterface(Window window) : base(new PImage(Properties.Resources.MagnifyingGlass))
        {
            Window = window;
            DirectionText = new PText();

            DirectionText.ConstrainHeightToTextHeight = false;
            DirectionText.ConstrainWidthToTextWidth = false;

            DirectionText.Bounds = new RectangleF(320, 20, 60, 60);
            DirectionText.Font = new Font("Century Gothic", 16, FontStyle.Bold);

            AddChild(DirectionText);
        }

        public override bool Accepts(UMD.HCIL.Piccolo.Event.PInputEventArgs e)
        {
            return e.IsKeyEvent && (MatchKeys(Keys.F2, e.KeyData) || MatchKeys(Keys.F3, e.KeyData));
        }

        public override void Press(object sender, UMD.HCIL.Piccolo.Event.PInputEventArgs e)
        {
            PPickPath keyFocus = e.InputManager.KeyboardFocus;
            PNode focused = keyFocus.PickedNode;

            if (MatchKeys(Keys.F3, e.KeyData))
            {
                DirectionText.Text = "Find\nNext";
            }
            else
            {
                DirectionText.Text = "Find\nLast";
            }

            if (focused is PStyledText)
            {
                StartPage = (Page)((PStyledText)focused).Parent;
                StartDoc = StartPage.Doc;
                StartLoc = ((PStyledText)focused).Model.SelectionStart;
            }
            else if (focused is PCamera)
            {
                PointF lastClick = Window.DocHandler.lastPoint;
                Document[] docs = Window.Documents;

                StartDoc = docs.FirstOrDefault(a => a.Pages[0].X >= lastClick.X);
                if (StartDoc == null)
                {
                    StartDoc = docs[0];
                }

                StartPage = StartDoc.Pages[0];
                StartLoc = 0;
            }
            else
            {
                throw new Exception("Focus is on unexpected node");
            }
        }

        public override void Release(object sender, UMD.HCIL.Piccolo.Event.PInputEventArgs e)
        {
            if (Entry.Text != "")
            {
                Document[] docs = Window.Documents;
                int startDoc = Array.IndexOf(docs, StartDoc);
                int startPage = Array.IndexOf(StartDoc.Pages, StartPage);

                if (startDoc < 0 || startPage < 0)
                {
                    throw new Exception("Document or Page doesn't exist");
                }

                bool reverse = MatchKeys(Keys.F2, e.KeyData);

                if (SearchDocuments(docs, Entry.Text, startDoc, startPage, StartLoc, reverse))
                {
                    RectangleF planeBounds = RectangleF.Union(Window.Canvas.Camera.ViewBounds, FoundPage.GlobalBounds);
                    RectangleF pageBounds = FoundPage.GlobalBounds;
                    PTransformActivity zoomOut = Window.Canvas.Camera.AnimateViewToCenterBounds(planeBounds, true, 400);
                    PTransformActivity zoomIn = Window.Canvas.Camera.AnimateViewToCenterBounds(pageBounds, true, 400);

                    zoomOut.SlowInSlowOut = true;
                    zoomIn.SlowInSlowOut = true;

                    zoomIn.StartAfter(zoomOut);
                }
            }
        }

        protected bool SearchPage(Page page, string phrase, int startLoc, bool reverse)
        {
            if (startLoc == -1)
            {
                if (!reverse)
                {
                    startLoc = 0;
                }
                else
                {
                    startLoc = page.Text.Length;
                }
            }

            int caratLocation;
            if (!reverse)
            {
                caratLocation = startLoc + page.Text.Substring(startLoc).ToLower().IndexOf(phrase) + phrase.Length;
            }
            else
            {
                caratLocation = page.Text.Substring(0,startLoc).ToLower().LastIndexOf(phrase);
            }

            if (caratLocation > 0)
            {
                page.Model.Select(caratLocation, caratLocation);
                FoundPage = page;
                FoundPage.UpdateCarat();
                return true;
            }
            else
            {
                return false;
            }
        }

        protected static Page[] OrderPages(Page[] pages, int startPage, bool reverse)
        {
            //If it needs to scan everything, make sure it starts in the right place
            if (startPage == -1)
            {
                if (!reverse)
                {
                    startPage = 0;
                }
                else
                {
                    startPage = pages.Length - 1;
                }
            }

            if (!reverse)
            {
                return pages.Skip(startPage).ToArray();
            }
            else
            {
                return pages.Take(startPage + 1).Reverse().ToArray();
            }
        }

        protected bool SearchDocument(Document document, string phrase, int startPage, int startLoc, bool reverse)
        {
            Page[] pages = document.Pages;

            pages = OrderPages(pages, startPage, reverse);

            foreach (Page page in pages)
            {
                if (SearchPage(page, phrase, startLoc, reverse))
                {
                    return true;
                }

                //It needs to search the whole of the following pages
                startLoc = -1;
            }
            return false;
        }

        protected static Document[] OrderDocuments(Document[] documents, int startDoc, bool reverse)
        {
            if (reverse)
            {
                documents = documents.Reverse().ToArray();
                startDoc = documents.Length - startDoc - 1;
            }

            return documents.Skip(startDoc).Concat(documents.Take(startDoc + 1)).ToArray();
        }

        protected bool SearchDocuments(Document[] documents, string phrase, int startDoc, int startPage, int startLoc, bool reverse)
        {
            FoundPage = null;

            documents = OrderDocuments(documents, startDoc, reverse);
            
            foreach (Document document in documents)
            {
                if (SearchDocument(document, phrase, startPage, startLoc, reverse))
                {
                    return true;
                }

                //It needs to search the whole of the following doucments
                startPage = -1;
                startLoc = -1;
            }

            return false;
        }
    }
}
