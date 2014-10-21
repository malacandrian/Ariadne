using System;
using UMD.HCIL.Piccolo;
using UMD.HCIL.PiccoloX;
using UMD.HCIL.Piccolo.Event;
using System.Drawing;
using System.Windows.Forms;
using UMD.HCIL.Piccolo.Nodes;
using UMD.HCIL.PiccoloX.Util.PStyledTextHelpers;
using zoom.Generator;
using zoom.Interfaces;
using System.Collections.Generic;
using UMD.HCIL.Piccolo.Util;
using System.Linq;
using UMD.HCIL.PiccoloX.Nodes;

namespace zoom
{
    /// <summary>
    /// Window is the main Windows form form that everything is displayed within
    /// </summary>
    public class Window : PForm
    {
        /// <summary>
        /// The current selection
        /// </summary>
        public Selection Selection { get; protected set; }

        /// <summary>
        /// Shows, hides, and executes the command interface on request
        /// </summary>
        public ShowInterfaceHandler CommandHandler { get; protected set; }

        /// <summary>
        /// Shows, hides, and executes the find interface on request
        /// </summary>
        public ShowInterfaceHandler FindHandler { get; protected set; }

        /// <summary>
        /// Automatically produces documents when the user enters text on the background
        /// </summary>
        public DocCreateHandler DocHandler { get; protected set; }

        /// <summary>
        /// The list of documents attached to the Window
        /// </summary>
        public Document[] Documents
        {
            get
            {
                //Filter all children, adding any documents to the list
                List<Document> output = new List<Document>();
                PNodeList children = Canvas.Layer.ChildrenReference;
                foreach (PNode node in children)
                {
                    if (node is Document) { output.Add((Document)node); }
                }

                //Sort the documents first by their X coordinate, then by their Y
                return output.OrderBy(a => a.X).ThenBy(a => a.Y).ToArray();
            }
        }

        /// <summary>
        /// Set the form up first time it is run
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            //The point is to emulate an OS, so make the window full screen
            WindowState = FormWindowState.Maximized;

            //Shift keyboard focus to the background
            Canvas.Root.DefaultInputManager.KeyboardFocus = Canvas.Camera.ToPickPath();

            //Set up event listeners
            Canvas.ZoomEventHandler.AcceptsEvent = delegate(PInputEventArgs e) { return e.PickedNode is PCamera && e.IsMouseEvent && AcceptsMouseButton(MouseButtons.Right, e.Button); };
            Canvas.PanEventHandler.AcceptsEvent = delegate(PInputEventArgs e) { return e.PickedNode is PCamera && e.IsMouseEvent && AcceptsMouseButton(MouseButtons.Left | MouseButtons.Right, e.Button); };

            CommandHandler = new ShowInterfaceHandler(Canvas.Camera, new CommandInterface(Canvas.Camera));
            Canvas.Camera.AddInputEventListener(CommandHandler);

            FindHandler = new ShowInterfaceHandler(Canvas.Camera, new FindInterface(this));
            Canvas.Camera.AddInputEventListener(FindHandler);

            DocHandler = new DocCreateHandler(Canvas);
            Canvas.AddInputEventListener(DocHandler);

            //Generate sample documents
            GenerateDocs();
        }

        /// <summary>
        /// Return true if the expected and actual values exactly match
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        /// <returns></returns>
        private static bool AcceptsMouseButton(MouseButtons expected, MouseButtons actual)
        {
            return (expected & actual) == expected && (expected ^ actual) == 0;
        }

        /// <summary>
        /// Generate a set of documents, and add them to the window
        /// </summary>
        private void GenerateDocs()
        {
            Document[] docs = SampleDocs(4);
            foreach (Document doc in docs) { Canvas.Layer.AddChild(doc); }
        }

        /// <summary>
        /// Generate a set of documents
        /// </summary>
        /// <param name="numDocs">How many documents to create</param>
        /// <returns>The documents</returns>
        private Document[] SampleDocs(int numDocs)
        {
            //Generate Sections
            Section title = new Section(new Font("Century Gothic", 22), Color.FromArgb(128, 0, 0), 1, 0);
            Section h1 = new Section(new Font("Century Gothic", 16), Color.FromArgb(128, 0, 0), 1, 0);
            Section h2 = new Section(new Font("Century Gothic", 13), Color.FromArgb(128, 0, 0), 1, 0);
            Section h3 = new Section(new Font("Century Gothic", 11, FontStyle.Bold), Color.FromArgb(128, 0, 0), 1, 0);
            Section text1 = new Section(new Font("Century Gothic", 11), 4, 2);
            Section text2 = new Section(new Font("Century Gothic", 11), 4, 2);
            Section text3 = new Section(new Font("Century Gothic", 11), 4, 2);

            //Fill out selectors

            //Title Selector
            title.NextSection.TerminateWeight = 0;
            title.NextSection.AddSection(h1, 4);
            title.NextSection.AddSection(text1, 1);

            //H1 Selector
            h1.NextSection.TerminateWeight = 0;
            h1.NextSection.AddSection(text1, 2);
            h1.NextSection.AddSection(text2, 1);
            h1.NextSection.AddSection(h2, 2);

            //H2 Selector
            h2.NextSection.TerminateWeight = 0;
            h2.NextSection.AddSection(text2, 2);
            h2.NextSection.AddSection(text3, 1);
            h2.NextSection.AddSection(h3, 2);

            //H3 Selector
            h3.NextSection.TerminateWeight = 0;
            h3.NextSection.AddSection(text3, 1);

            //Text1 Selector
            text1.NextSection.TerminateWeight = 1;
            text1.NextSection.AddSection(text1, 4);
            text1.NextSection.AddSection(h1, 1);

            //Text2 Selector
            text2.NextSection.TerminateWeight = 2;
            text2.NextSection.AddSection(text2, 8);
            text2.NextSection.AddSection(h1, 1);
            text2.NextSection.AddSection(h2, 1);

            //Text3 Selector
            text3.NextSection.TerminateWeight = 3;
            text3.NextSection.AddSection(text3, 12);
            text3.NextSection.AddSection(h1, 1);
            text3.NextSection.AddSection(h2, 1);
            text3.NextSection.AddSection(h3, 1);

            //Generate the documents
            DocGenerator docGen = new DocGenerator(title, this);
            return docGen.GenerateDocSet(numDocs);
        }

        /// <summary>
        /// Ensure there is only one selection active on the entire window
        /// </summary>
        /// <remarks>Hooked in to each PStyledText's ConfirmSelection listener</remarks>
        /// <param name="oldSelection">The selection that was removed</param>
        /// <param name="newSelection">The selection that replaced it</param>
        public void ConfirmSelection(Selection oldSelection, Selection newSelection)
        {
            //If there is currently a selection, remove it
            if (Selection != null)
            {
                Selection.Active = false;
                if (Selection.Parent != null)
                {
                    Selection.RemoveFromParent();
                }
            }

            //Add the new selection
            Selection = newSelection;
        }
    }
}
