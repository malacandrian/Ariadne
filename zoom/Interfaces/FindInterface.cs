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
    /// <summary>
    /// The Find interface handles full-text searches of the entire document space
    /// </summary>
    public class FindInterface : AbstractInterface
    {
        /// <summary>
        /// Which kind of search is being performed
        /// </summary>
        public enum Direction { Forward, Backward, Highlight, None };

        /// <summary>
        /// The window the interface is attached to
        /// </summary>
        public Window Window { get; protected set; }

        /// <summary>
        /// Where the search begins
        /// </summary>
        public Location StartLocation { get; protected set; }

        /// <summary>
        /// The text to display, telling the user whether they are searching
        /// forwards, or backwards
        /// </summary>
        public PText DirectionText { get; protected set; }

        /// <summary>
        /// The text to display to the user when they are chosing to highlight the last jump
        /// </summary>
        public PText HighlightText { get; protected set; }

        /// <summary>
        /// A flag for whether the user has pressed the key indicating a backwards search
        /// </summary>
        private bool _BackPressed;

        /// <summary>
        /// A flag for whether the user has pressed the key indicating a forwards search
        /// </summary>
        private bool _ForwardPressed;

        /// <summary>
        /// The key used to initiate a forwards search
        /// </summary>
        private static readonly Keys _Forward = Keys.F3;

        /// <summary>
        /// The key used to initiate a backwards search
        /// </summary>
        private static readonly Keys _Backward = Keys.F2;

        /// <summary>
        /// The type of search currently being performed
        /// </summary>
        public Direction SearchType { get; protected set; }

        /// <summary>
        /// Where the previous search began
        /// </summary>
        public Location LastJumpStart { get; protected set; }

        /// <summary>
        /// Where the previous search ended
        /// </summary>
        public Location LastJumpEnd { get; protected set; }

        /// <summary>
        /// Create a new find interface attached to a specific window
        /// </summary>
        /// <param Name="window"></param>
        public FindInterface(Window window)
            : base(new PImage(Properties.Resources.MagnifyingGlass))
        {
            Window = window;
            SearchType = Direction.None;

            //Initialise the text to indicate to the user which direction they're searching
            DirectionText = new PText();
            DirectionText.ConstrainHeightToTextHeight = false;
            DirectionText.ConstrainWidthToTextWidth = false;
            DirectionText.Bounds = new RectangleF(320, 20, 60, 60);
            DirectionText.Font = new Font("Century Gothic", 16, FontStyle.Bold);
            AddChild(DirectionText);

            //Initilise the text to indicate to the user that they're going to highlight the last jump
            HighlightText = new PText("Select last jump");
            HighlightText.Font = Entry.Font;
            HighlightText.Bounds = Entry.Bounds;
            HighlightText.Visible = false;
            AddChild(HighlightText);

            //Hook the update Width event up to the listener
            Handler.UpdateWidth += UpdateWidth;
        }

        /// <summary>
        /// Ensure that the direction text moves with the icon
        /// </summary>
        protected void UpdateWidth()
        {
            float basicWidth = MinWidth;
            if (Entry.Width > MinWidth) { basicWidth = Entry.Width; }

            DirectionText.X = basicWidth + Padding;
        }

        /// <summary>
        /// Accept key events involving f2 or f3
        /// </summary>
        /// <param Name="e">The event</param>
        /// <returns>Whether the event is a key event that involves F2 or F3</returns>
        public override bool Accepts(PInputEventArgs e) { return e.IsKeyEvent && (MatchKeys(Keys.F2, e.KeyData) || MatchKeys(Keys.F3, e.KeyData)); }

        /// <summary>
        /// When the activate key is first pressed, start the check of which actual have been pressed
        /// </summary>
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

        /// <summary>
        /// Whenever the activate key is pressed, raise the flag indicating that that key has been pressed
        /// </summary>
        public override void RegisterActivateButtonPress(object sender, PInputEventArgs e)
        {
            //Activate the relevant flag
            if (MatchKeys(_Forward, e.KeyData)) { _ForwardPressed = true; }
            else if (MatchKeys(_Backward, e.KeyData)) { _BackPressed = true; }
        }

        /// <summary>
        /// Fires a fraction of a second after the user first presses an activate button
        /// </summary>
        /// <param Name="sender">The time that fired the method</param>
        /// <param Name="e">The arguments of the timer tick</param>
        private void WaitForChord_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //Make sure the timer only fires once
            StopTimer(sender);
            //If the user has release already, abort
            if (!(_ForwardPressed || _BackPressed)) { return; }

            //Work out which direction the user will be searching in
            SearchType = GetDirection();
            SetDirectionText();

            //Find where the search is starting from
            StartLocation = FindStartLocation(GetKeyFocus());
        }

        /// <summary>
        /// Find where the search is starting from
        /// </summary>
        /// <param Name="focused"></param>
        /// <returns></returns>
        protected Location FindStartLocation(PNode focused)
        {
            Document startDoc;
            Page startPage;
            int startLoc;

            //If a page is currently in focus
            //Start at the user's current location
            if (focused is Page)
            {
                startPage = (Page)focused;
                startDoc = startPage.Document;
                startLoc = startPage.Model.SelectionStart;
            }

            //If the background is in focus
            //Start at the next document after the point of the background in focus
            else if (focused is PCamera)
            {
                //Find where the user last clicked
                PointF lastClick = Window.DocHandler.LastPoint;

                //Get the first document to the right of the last clicked location
                Document[] docs = Window.Documents;
                startDoc = docs.FirstOrDefault(a => a.Pages[0].X >= lastClick.X);
                if (startDoc == null) { startDoc = docs[0]; }

                //Get the first character of the first page of that document
                startPage = startDoc.Pages[0];
                startLoc = 0;
            }

            //If neither is in focus, the system is in an invalid state
            else { throw new Exception("Focus is on unexpected node"); }

            return new Location(Window, startDoc, startPage, startLoc);
        }

        /// <summary>
        /// Work out which node is currently taking keyboard input
        /// </summary>
        /// <returns>The node currently taking keyboard input</returns>
        protected PNode GetKeyFocus()
        {
            PNode focused;
            if (Page.LastPage != null) { focused = Page.LastPage; }
            else { focused = Window.Canvas.Camera; }
            return focused;
        }

        /// <summary>
        /// Stop the timer
        /// </summary>
        /// <param Name="sender">The timer to stop</param>
        protected static void StopTimer(object sender)
        {
            //If the object is, in fact, a timer
            //Stop it and safely dispose of any resources it owns
            if (sender is System.Timers.Timer)
            {
                System.Timers.Timer timer = (System.Timers.Timer)sender;

                timer.Stop();
                timer.Dispose();
            }
        }

        /// <summary>
        /// Work out what kind of search is being peformed
        /// </summary>
        /// <returns>The kind of search being performed</returns>
        protected Direction GetDirection()
        {
            //If both keys are pressed, the user is highlighting the last jump
            if (_ForwardPressed && _BackPressed) { return Direction.Highlight; }
            //If only the forward button is pressed, the user is searching forwards
            else if (_ForwardPressed) { return Direction.Forward; }
            //If only the backwards button is pressed, the user is searching backwards
            else if (_BackPressed) { return Direction.Backward; }

            throw new Exception("Invalid state. At least one of left and right alt must be depressed");
        }

        /// <summary>
        /// Change the Direction Text to indicate to the user what kind of search is being performed
        /// </summary>
        protected void SetDirectionText()
        {
            if (SearchType == Direction.Highlight)
            {
                Entry.Visible = false;
                HighlightText.Visible = true;
            }
            else if (SearchType == Direction.Forward)
            {
                DirectionText.Text = "Find\nNext";
            }
            else if (SearchType == Direction.Backward)
            {
                DirectionText.Text = "Find\nLast";
            }
            else
            {
                throw new ArgumentException("direction is an unexpected value");
            }
        }

        /// <summary>
        /// Perform the search
        /// </summary>
        public override void Execute(object sender, UMD.HCIL.Piccolo.Event.PInputEventArgs e)
        {
            //If the user is highlighting the last jump, do that
            if (SearchType == Direction.Highlight) { Highlight(); }
            //If the user is searching, perform the search
            else if (SearchType == Direction.Forward || SearchType == Direction.Backward) { Search(); }

            //Reset the direction detection flags
            _ForwardPressed = _BackPressed = false;
            SearchType = Direction.None;
        }

        /// <summary>
        /// Highlight the last jump
        /// </summary>
        protected void Highlight()
        {
            //Reset the visibility of the direction and highlight texts
            HighlightText.Visible = false;
            Entry.Visible = true;

            //If the last jump performed is stored, highlight it
            if (LastJumpStart != null && LastJumpEnd != null)
            {
                Page startPage = LastJumpStart.Page;
                Page endPage = LastJumpEnd.Page;
                //The system can only highlight a jump that happened within a single page
                if (startPage == endPage)
                {
                    //Select the last jump
                    Selection select = startPage.Select((int)LastJumpStart.CharIndex, (int)LastJumpEnd.CharIndex);
                    startPage.ConfirmSelection(null, select);
                }
            }
        }

        /// <summary>
        /// Search all the documents until the phrase has been found
        /// </summary>
        protected void Search()
        {
            string phrase = Entry.Text;
            //Clear the direction text
            DirectionText.Text = "";

            //Decide which direction the search should happen in
            bool reverse = (SearchType == Direction.Backward);

            //Perform the search
            Document[] documents = OrderDocuments(Window.Documents, StartLocation.DocumentIndex, reverse);
            Location foundLocation = SearchDocuments(documents, StartLocation, phrase, reverse);

            //If the phrase was found
            if (foundLocation != null)
            {
                //Find the next space after the found location
                Location wordEnd = FindNearestWordEnd(foundLocation, reverse);

                //Point the camera at the space, and set the keyboard focus to it
                NavigateToFoundPage(wordEnd);
                ActivateFoundPage(wordEnd);

                //Store the locations of this jump for possible later highlighting
                LastJumpStart = StartLocation;
                LastJumpEnd = wordEnd;
            }
        }

        /// <summary>
        /// Find the next space before or after a specific location
        /// </summary>
        /// <param Name="location">Where to start searching</param>
        /// <param Name="reverse">True if it should search backwards, false if it should search forwards</param>
        /// <returns>The location of the next space in the relevant direction</returns>
        protected static Location FindNearestWordEnd(Location location, bool reverse)
        {
            //Try to find a space in the current page in the relevant direction
            Location output = SearchPage(location, " ", reverse);

            //If it fails take either the start, or end, of the document as appropriate
            if (output == null)
            {
                int newCharIndex;
                if (reverse) { newCharIndex = 0; }
                else { newCharIndex = location.Page.Text.Length; }

                output = new Location(location.Window, location.Document, location.Page, newCharIndex);
            }
            return output;
        }

        /// <summary>
        /// Set the keyboard focus to be the found location
        /// </summary>
        /// <param Name="foundLocation">The location to focus on</param>
        protected void ActivateFoundPage(Location foundLocation)
        {
            Window.FindHandler.KeyFocus = foundLocation.Page.ToPickPath();
            foundLocation.Page.Model.Select((int)foundLocation.CharIndex, 0);
        }

        /// <summary>
        /// Move the camera so that its focusing on the found location
        /// </summary>
        /// <param Name="foundLocation">The location to focus on</param>
        protected void NavigateToFoundPage(Location foundLocation)
        {
            Page foundPage = foundLocation.Page;

            //The camera should first zoom out so that it is looking at both the start location and the found page
            //And then zoom in so that it is only looking at the found page
            RectangleF planeBounds = RectangleF.Union(Window.Canvas.Camera.ViewBounds, foundPage.GlobalBounds);
            RectangleF pageBounds = foundPage.GlobalBounds;
            PTransformActivity zoomOut = Window.Canvas.Camera.AnimateViewToCenterBounds(planeBounds, true, 400);
            PTransformActivity zoomIn = Window.Canvas.Camera.AnimateViewToCenterBounds(pageBounds, true, 400);

            //The camera action can happen faster without causing the user to lose track of where they are
            //or getting motion sick if the zoom accelerates toward the middle of each animation
            //and decelerates toward the end of the animation than if the animation moves at constant speed
            zoomOut.SlowInSlowOut = true;
            zoomIn.SlowInSlowOut = true;

            //Order the animations
            zoomIn.StartAfter(zoomOut);
        }

        /// <summary>
        /// Search a page for a specific phrase
        /// </summary>
        /// <param Name="location">The location to begin the search at.</param>
        /// <param Name="phrase">The phrase to search for</param>
        /// <param Name="reverse">Whether the search should be forwards or backwards</param>
        /// <returns>The location of the start of the phrase, or null if not found</returns>
        protected static Location SearchPage(Location location, string phrase, bool reverse)
        {
            int startLoc = GetCharIndexFromLocation(location, reverse);

            int phraseLocation, minFind;

            if (reverse == false) //If the search is forwards
            {
                minFind = startLoc;
                //Take the part of the text following the start location
                string textPart = location.Page.Text.Substring(startLoc).ToLower();
                //Find the first instance of the phrase within that text part
                phraseLocation = startLoc + textPart.IndexOf(phrase);
            }
            else //If the search is backwards
            {
                minFind = 0;
                //Take the part of the text preceding the start location
                string textPart = location.Page.Text.Substring(0, startLoc).ToLower();
                //Find the last instance of the phrase within that text part
                phraseLocation = textPart.LastIndexOf(phrase);
            }

            //If the phrase was found, return the location of the
            if (phraseLocation >= minFind) { return new Location(location.Window, location.Document, location.Page, phraseLocation); }

            //IF the phrase wasn't found, return null
            else { return null; }
        }

        /// <summary>
        /// Find what character index the location object says to begin at
        /// </summary>
        /// <remarks>Because null is used as a wildcard, that can mean multiple things, it isn't as simple as just reading location.charindex</remarks>
        /// <param Name="location">The location to check</param>
        /// <param Name="reverse">Whether the search is backwards</param>
        /// <returns>The location to begin searching at</returns>
        protected static int GetCharIndexFromLocation(Location location, bool reverse)
        {
            //Ensure the user is searching a real page
            if (location.Page == null) { throw new ArgumentException("Cannot find a character index from a null page"); }

            //If an actual value has been provided, return that
            if (location.CharIndex != null) { return (int)location.CharIndex; }

            else //If the wildcard value was used
            {
                //If it's a forwards search, return the start of the document
                if (reverse == false) { return 0; }
                //If it's a backwards search, return the end of the document
                else { return location.Page.Text.Length; }
            }
        }

        /// <summary>
        /// Reorders an array of pages to the correct order for searching through
        /// </summary>
        /// <param Name="pages">The array of pages to search</param>
        /// <param Name="startPage">The page to start on. Null indicates all pages should be searched</param>
        /// <param Name="reverse">The direction the search should happen in</param>
        /// <returns>The reorded array</returns>
        protected static Page[] OrderPages(Page[] pages, int? startPage, bool reverse)
        {
            //If it needs to scan everything, make sure it starts in the right place
            if (startPage == null)
            {
                if (!reverse) { startPage = 0; }
                else { startPage = pages.Length - 1; }
            }

            //If the search is forwards, take only the start page and following pages
            if (!reverse) { return pages.Skip((int)startPage).ToArray(); }

            //If the search is backwards, take only the start page and preceding pages, then reverse the order
            else { return pages.Take((int)startPage + 1).Reverse().ToArray(); }
        }

        /// <summary>
        /// Search every page of a document for a specific phrase
        /// </summary>
        /// <param Name="location">The location to start searching in</param>
        /// <param Name="phrase">The phrase to search for</param>
        /// <param Name="reverse">Whether the search should be backwards</param>
        /// <returns>The found location, or null if nothing is found</returns>
        protected static Location SearchDocument(Location location, string phrase, bool reverse)
        {
            //Get the list of pages of the document
            Page[] pages = location.Document.Pages;
            //If the document had no pages, exit
            if (pages.Length == 0) { return null; }

            //Put the pages in the correct order for searching
            Page[] newPages = OrderPages(pages, location.PageIndex, reverse);

            //Search the list of pages for the phrase
            Location newLocation = new Location(location.Window, location.Document, newPages[0], location.CharIndex);
            return SearchPages(newPages, newLocation, phrase, reverse);
        }
        
        /// <summary>
        /// Recursively search every page in an array for a phrase, returning when it has been found
        /// </summary>
        /// <param Name="pages">The list of pages to search</param>
        /// <param Name="location">Where to start the search</param>
        /// <param Name="phrase">The phrase to search for</param>
        /// <param Name="reverse">Whether the search should be backwards</param>
        /// <returns>The location the phrase was found in, or null if it wasn't found</returns>
        protected static Location SearchPages(Page[] pages, Location location, string phrase, bool reverse)
        {
            //Search for the phrase, and return it if found
            Location output = SearchPage(location, phrase, reverse);
            if (output != null) { return output; }

            //Remove the first item of the array and return null if no pages remain
            Page[] newPages = pages.Skip(1).ToArray();
            if (newPages.Length == 0) { return null; }

            //Start the next recursion
            Location newLocation = new Location(location.Window, location.Document, newPages[0], null);
            return SearchPages(newPages, newLocation, phrase, reverse);
        }

        /// <summary>
        /// Order documents for searching
        /// </summary>
        /// <param Name="documents">The list of documents to search</param>
        /// <param Name="startDoc">The index of the document to start searching at</param>
        /// <param Name="reverse">Whether the search should be backwards</param>
        /// <returns></returns>
        protected static Document[] OrderDocuments(Document[] documents, int startDoc, bool reverse)
        {
            //If the search is backwards, reverse the list
            //And update the startDoc pointer to the new location
            if (reverse)
            {
                documents = documents.Reverse().ToArray();
                startDoc = documents.Length - startDoc - 1;
            }

            //Reorder the list such that all documents from before the start document 
            //are removed and added to the end of the list
            return documents.Skip(startDoc).Concat(documents.Take(startDoc + 1)).ToArray();
        }

        /// <summary>
        /// Recursively search all documents for a phrase
        /// </summary>
        /// <param Name="documents">The list of documents to search</param>
        /// <param Name="location">The location to start searching in</param>
        /// <param Name="phrase">The phrase to search for</param>
        /// <param Name="reverse">Whether the search should be backwards</param>
        /// <returns>The location of the phrase, or null if not found</returns>
        protected static Location SearchDocuments(Document[] documents, Location location, string phrase, bool reverse)
        {
            //Search for the phrase, and return if found
            Location output = SearchDocument(location, phrase, reverse);
            if (output != null) { return output; }

            //Remove the first document fro the array, and exit if no documents remain
            Document[] newDocuments = documents.Skip(1).ToArray();
            if (newDocuments.Length == 0) { return null; }

            //Begin the next recursion
            Location newLocation = new Location(location.Window, newDocuments[0], null, null);
            return SearchDocuments(newDocuments, newLocation, phrase, reverse);
        }
    }
}
