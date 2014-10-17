using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using UMD.HCIL.Piccolo;
using UMD.HCIL.Piccolo.Util;
using UMD.HCIL.PiccoloX.Nodes;
using UMD.HCIL.PiccoloX.Util.PStyledTextHelpers;
using System.Windows.Forms;
using UMD.HCIL.Piccolo.Nodes;
using UMD.HCIL.Piccolo.Activities;
using UMD.HCIL.Piccolo.Event;

namespace zoom.Interfaces
{
    public class FindInterface : AbstractInterface
    {
        public enum Direction { Forward, Backward, Highlight, None };

        public Window Window { get; protected set; }


        public Location StartLocation { get; protected set; }

        public PText DirectionText { get; protected set; }
        public PText HighlightText { get; protected set; }

        private bool BackPressed, ForwardPressed;
        private static readonly Keys Forward = Keys.F3;
        private static readonly Keys Backward = Keys.F2;
        public Direction SearchDirection { get; protected set; }

        public Location LastJumpStart { get; protected set; }
        public Location LastJumpEnd { get; protected set; }

        public FindInterface(Window window)
            : base(new PImage(Properties.Resources.MagnifyingGlass))
        {
            Window = window;
            SearchDirection = Direction.None;
            DirectionText = new PText();

            DirectionText.ConstrainHeightToTextHeight = false;
            DirectionText.ConstrainWidthToTextWidth = false;

            DirectionText.Bounds = new RectangleF(320, 20, 60, 60);
            DirectionText.Font = new Font("Century Gothic", 16, FontStyle.Bold);

            AddChild(DirectionText);

            HighlightText = new PText("Select last jump");

            HighlightText.Font = Entry.Font;
            HighlightText.Bounds = Entry.Bounds;
            HighlightText.Visible = false;

            AddChild(HighlightText);

            Handler.UpdateWidth += UpdateWidth;
        }

        protected void UpdateWidth()
        {
            float basicWidth = MinWidth;
            if (Entry.Width > MinWidth) { basicWidth = Entry.Width; }

            DirectionText.X = basicWidth + Padding;
        }

        public override bool Accepts(PInputEventArgs e)
        {
            return e.IsKeyEvent && (MatchKeys(Keys.F2, e.KeyData) || MatchKeys(Keys.F3, e.KeyData));
        }

        public override void Activate(object sender, PInputEventArgs e)
        {
            //Wait a fraction of a second before continuing to allow for Chording
            System.Timers.Timer WaitForChord = new System.Timers.Timer(20);
            WaitForChord.Elapsed += delegate(object s, System.Timers.ElapsedEventArgs e2)
            {
                //The timer launches a new thread, but all interaction needs to happen on the UI thread
                //So the timer tells the UI thread to do what it needs to do
                Window.BeginInvoke((Action)delegate() { WaitForChord_Elapsed(s, e2); });
            };

            WaitForChord.Start();
        }

        public override void RegisterActivateButtonPress(object sender, PInputEventArgs e)
        {
            //Activate the relevant flag
            if (MatchKeys(Forward, e.KeyData)) { ForwardPressed = true; }
            else if (MatchKeys(Backward, e.KeyData)) { BackPressed = true; }
        }

        void WaitForChord_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            StopTimer(sender);
            //If the user has release already, abort
            if (!(ForwardPressed || BackPressed)) { return; }

            SearchDirection = GetDirection();
            SetDirectionText();

            StartLocation = FindStartLocation(GetKeyFocus());

        }

        protected Location FindStartLocation(PNode focused)
        {
            Document startDoc;
            Page startPage;
            int startLoc;
            if (focused is Page)
            {
                startPage = (Page)focused;
                startDoc = startPage.Doc;
                startLoc = startPage.Model.SelectionStart;
            }
            else if (focused is PCamera)
            {
                PointF lastClick = Window.DocHandler.lastPoint;
                Document[] docs = Window.Documents;

                startDoc = docs.FirstOrDefault(a => a.Pages[0].X >= lastClick.X);
                if (startDoc == null)
                {
                    startDoc = docs[0];
                }

                startPage = startDoc.Pages[0];
                startLoc = 0;
            }
            else
            {
                throw new Exception("Focus is on unexpected node");
            }

            return new Location(Window, startDoc, startPage, startLoc);
        }



        protected PNode GetKeyFocus()
        {
            PNode focused;
            if (Page.LastPage != null) { focused = Page.LastPage; }
            else { focused = Window.Canvas.Camera; }
            return focused;
        }

        protected static void StopTimer(object sender)
        {
            //Make sure the time only runs once
            if (sender is System.Timers.Timer)
            {
                System.Timers.Timer timer = (System.Timers.Timer)sender;

                timer.Stop();
                timer.Dispose();
            }
        }

        protected Direction GetDirection()
        {
            if (ForwardPressed && BackPressed) { return Direction.Highlight; }
            else if (ForwardPressed) { return Direction.Forward; }
            else if (BackPressed) { return Direction.Backward; }

            throw new Exception("Invalid state. At least one of left and right alt must be depressed");
        }

        protected void SetDirectionText()
        {
            if (SearchDirection == Direction.Highlight)
            {
                Entry.Visible = false;
                HighlightText.Visible = true;
            }
            else if (SearchDirection == Direction.Forward)
            {
                DirectionText.Text = "Find\nNext";
            }
            else if (SearchDirection == Direction.Backward)
            {
                DirectionText.Text = "Find\nLast";
            }
            else
            {
                throw new ArgumentException("direction is an unexpected value");
            }
        }

        public override void Release(object sender, UMD.HCIL.Piccolo.Event.PInputEventArgs e)
        {
            if (SearchDirection == Direction.Highlight) { Highlight(); }
            else if (SearchDirection == Direction.Forward || SearchDirection == Direction.Backward) { Search(); }

            //Reset the direction detection flags
            ForwardPressed = BackPressed = false;
            SearchDirection = Direction.None;
        }

        protected void Highlight()
        {
            HighlightText.Visible = false;
            Entry.Visible = true;

            if (LastJumpStart != null && LastJumpEnd != null)
            {
                Page startPage = LastJumpStart.Page;
                Page endPage = LastJumpEnd.Page;
                if (startPage == endPage)
                {
                    Selection select = startPage.Select((int)LastJumpStart.CharIndex, (int)LastJumpEnd.CharIndex);
                    startPage.ConfirmSelection(null, select);
                }
            }
        }

        protected void Search()
        {
            string phrase = Entry.Text;
            //Clear the direction text
            DirectionText.Text = "";

            bool reverse = (SearchDirection == Direction.Backward);


            //Perform the search
            Document[] documents = OrderDocuments(Window.Documents, StartLocation.DocumentIndex, reverse);
            Location foundLocation = SearchDocuments(documents, StartLocation, phrase, reverse);

            if (foundLocation != null)
            {
                Location wordEnd = FindNearestWordEnd(foundLocation, reverse);
                NavigateToFoundPage(wordEnd);
                ActivateFoundPage(wordEnd);

                LastJumpStart = StartLocation;
                LastJumpEnd = wordEnd;
            }
        }

        protected static Location FindNearestWordEnd(Location location, bool reverse)
        {
            Location output = SearchPage(location, " ", reverse);
            if (output == null)
            {
                int newCharIndex;
                if (reverse) { newCharIndex = 0; }
                else { newCharIndex = location.Page.Text.Length; }

                output = new Location(location.Window, location.Document, location.Page, newCharIndex);
            }
            return output;
        }

        protected void ActivateFoundPage(Location foundLocation)
        {
            //foundLocation.Page.Active = true;
            Window.FindHandler.KeyFocus = foundLocation.Page.ToPickPath();
            foundLocation.Page.Model.Select((int)foundLocation.CharIndex, 0);
        }

        protected void NavigateToFoundPage(Location foundLocation)
        {
            Page foundPage = foundLocation.Page;

            RectangleF planeBounds = RectangleF.Union(Window.Canvas.Camera.ViewBounds, foundPage.GlobalBounds);
            RectangleF pageBounds = foundPage.GlobalBounds;
            PTransformActivity zoomOut = Window.Canvas.Camera.AnimateViewToCenterBounds(planeBounds, true, 400);
            PTransformActivity zoomIn = Window.Canvas.Camera.AnimateViewToCenterBounds(pageBounds, true, 400);

            zoomOut.SlowInSlowOut = true;
            zoomIn.SlowInSlowOut = true;

            zoomIn.StartAfter(zoomOut);
        }

        protected static Location SearchPage(Location location, string phrase, bool reverse)
        {
            int startLoc = GetCharIndexFromLocation(location, reverse);

            int phraseLocation, minFind;
            if (reverse == false)
            {
                minFind = startLoc;
                string textPart = location.Page.Text.Substring(startLoc).ToLower();
                phraseLocation = startLoc + textPart.IndexOf(phrase);
            }
            else
            {
                minFind = 0;
                string textPart = location.Page.Text.Substring(0, startLoc).ToLower();
                phraseLocation = textPart.LastIndexOf(phrase);
            }

            if (phraseLocation >= minFind) { return new Location(location.Window, location.Document, location.Page, phraseLocation); }
            else { return null; }
        }

        protected static int GetCharIndexFromLocation(Location location, bool reverse)
        {
            if (location.Page == null) { throw new ArgumentException("Cannot find a character index from a null page"); }
            if (location.CharIndex != null) { return (int)location.CharIndex; }
            else
            {
                if (reverse == false) { return 0; }
                else { return location.Page.Text.Length; }
            }
        }

        protected static Page[] OrderPages(Page[] pages, int? startPage, bool reverse)
        {
            //If it needs to scan everything, make sure it starts in the right place
            if (startPage == null)
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
                return pages.Skip((int)startPage).ToArray();
            }
            else
            {
                return pages.Take((int)startPage + 1).Reverse().ToArray();
            }
        }

        protected static Location SearchDocument(Location location, string phrase, bool reverse)
        {
            Page[] pages = location.Document.Pages;
            if (pages.Length == 0) { return null; }

            Page[] newPages = OrderPages(pages, location.PageIndex, reverse);

            Location newLocation = new Location(location.Window, location.Document, newPages[0], location.CharIndex);

            return SearchPages(newPages, newLocation, phrase, reverse);
        }

        protected static Location SearchPages(Page[] pages, Location location, string phrase, bool reverse)
        {
            //Search for the phrase, and return it if found
            Location output = SearchPage(location, phrase, reverse);
            if (output != null) { return output; }

            //Remove the first item of the array and begin the next recursion
            Page[] newPages = pages.Skip(1).ToArray();
            if (newPages.Length == 0) { return null; }

            Location newLocation = new Location(location.Window, location.Document, newPages[0], null);
            return SearchPages(newPages, newLocation, phrase, reverse);
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

        protected static Location SearchDocuments(Document[] documents, Location location, string phrase, bool reverse)
        {
            //Search for the phrase, and return if found
            Location output = SearchDocument(location, phrase, reverse);
            if (output != null) { return output; }

            Document[] newDocuments = documents.Skip(1).ToArray();
            if (newDocuments.Length == 0) { return null; }

            Location newLocation = new Location(location.Window, newDocuments[0], null, null);
            return SearchDocuments(newDocuments, newLocation, phrase, reverse);
        }
    }

    public class Location
    {
        public readonly Window Window;
        public readonly Document Document;
        public readonly Page Page;
        public readonly int? CharIndex;

        public int? PageIndex
        {
            get
            {
                if (Page != null)
                {
                    int output = Array.IndexOf(Document.Pages, Page);
                    if (output < 0) { throw new Exception(String.Format(@"Page {0} is not a member of Document {1}", Page.ToString(), Document.ToString())); }
                    return output;
                }
                else { return null; }
            }
        }

        public int DocumentIndex
        {
            get
            {
                int output = Array.IndexOf(Window.Documents, Document);
                if (output < 0) { throw new Exception(String.Format(@"Document {0} is not a member of Window {1}", Document.ToString(), Window.ToString())); }
                return output;
            }
        }

        public Location(Window window, Document document, Page page, int? charIndex)
        {
            Window = window;
            Document = document;
            Page = page;
            CharIndex = charIndex;
        }
    }
}
