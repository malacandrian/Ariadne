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
    public class Window : PForm
    {
        public Selection Selection { get; protected set; }

        public ShowInterfaceHandler CommandHandler { get; protected set; }
        public ShowInterfaceHandler FindHandler { get; protected set; }
        public DocCreateHandler DocHandler { get; protected set; }

        public Document[] Documents
        {
            get
            {
                List<Document> output = new List<Document>();
                PNodeList children = Canvas.Layer.ChildrenReference;
                foreach (PNode node in children)
                {
                    if (node is Document)
                    {
                        output.Add((Document)node);
                    }
                }

                return output.OrderBy(a => a.X).ThenBy(a => a.Y).ToArray();
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            WindowState = FormWindowState.Maximized;

            Canvas.Root.DefaultInputManager.KeyboardFocus = Canvas.Camera.ToPickPath();
            Canvas.ZoomEventHandler = new NewZoomEventHandler();
            Canvas.PanEventHandler = new NewPanEventHandler();

            CommandHandler = new ShowInterfaceHandler(Canvas.Camera, Keys.F1, new CommandInterface(Canvas.Camera));
            Canvas.Camera.AddInputEventListener(CommandHandler);

            FindHandler = new ShowInterfaceHandler(Canvas.Camera, Keys.F2 | Keys.F3, new FindInterface(this));
            Canvas.Camera.AddInputEventListener(FindHandler);

            DocHandler = new DocCreateHandler(Canvas);
            Canvas.AddInputEventListener(DocHandler);

            GenerateDocs();

        }

        private void GenerateDocs()
        {
            Document[] docs = SampleDocs(4);
            foreach (Document doc in docs)
            {
                Canvas.Layer.AddChild(doc);
            }
        }

        private Document[] SampleDocs(int numDocs)
        {
            //Generate Sections
            Section title = new Section(new Font("Century Gothic", 22), Color.FromArgb(128, 0, 0), new SectionSelector(), 1);
            Section h1 = new Section(new Font("Century Gothic", 16), Color.FromArgb(128, 0, 0), new SectionSelector(), 1);
            Section h2 = new Section(new Font("Century Gothic", 13), Color.FromArgb(128, 0, 0), new SectionSelector(), 1);
            Section h3 = new Section(new Font("Century Gothic", 11, FontStyle.Bold), Color.FromArgb(128, 0, 0), new SectionSelector(), 1);
            Section text1 = new Section(new Font("Century Gothic", 11), new SectionSelector(), 4);
            Section text2 = new Section(new Font("Century Gothic", 11), new SectionSelector(), 4);
            Section text3 = new Section(new Font("Century Gothic", 11), new SectionSelector(), 4);

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

            DocGenerator docGen = new DocGenerator(title, this);
            return docGen.GenerateDocSet(numDocs);
        }

        public void ConfirmSelection(Selection selected)
        {
            if (Selection != null)
            {
                Selection.Active = false;
                if (Selection.Parent != null)
                {
                    Selection.RemoveFromParent();
                }
            }
            Selection = selected;
        }
    }

    public class NewZoomEventHandler : PZoomEventHandler
    {
        protected override bool PZoomEventHandlerAcceptsEvent(PInputEventArgs e)
        {
            if (base.PZoomEventHandlerAcceptsEvent(e))
            {
                return (e.PickedNode is PCamera);  
            }
            return false;
        }
    }

    public class NewPanEventHandler : PPanEventHandler
    {
        protected override bool PPanEventHandlerAcceptsEvent(PInputEventArgs e)
        {
            if ( base.PPanEventHandlerAcceptsEvent(e))
            {
                return (e.PickedNode is PCamera);
            }
            return false;
        }
    }

    public class DocCreateHandler : PBasicInputEventHandler
    {
        public PointF lastPoint { get; protected set; }
        public PCanvas Owner { get; protected set; }

        public DocCreateHandler(PCanvas owner)
        {
            RectangleF curView = owner.Camera.Bounds;
            float x = curView.X + (curView.Width / 3);
            float y = curView.Y + (curView.Height / 3);
            lastPoint = new PointF(x, y);
            Owner = owner;
        }

        public override void OnClick(object sender, PInputEventArgs e)
        {
            base.OnClick(sender, e);
            lastPoint = e.Position;
            e.InputManager.KeyboardFocus = e.Path;
        }

        public override void OnKeyPress(object sender, PInputEventArgs e)
        {
            base.OnKeyPress(sender, e);
            int x = (int)lastPoint.X;
            int y = (int)lastPoint.Y;
            Document created = new Document(x, y,e.KeyChar, (Window)Owner.FindForm(), Owner.Camera);
            Owner.Layer.AddChild(created);
            PStyledText firstPage = created.Pages[0].Text;
            e.InputManager.KeyboardFocus = firstPage.ToPickPath(e.Camera,firstPage.Bounds);

        }

        public override bool DoesAcceptEvent(PInputEventArgs e)
        {
            bool isLeftClick = e.IsClickEvent && e.Button == MouseButtons.Left;
            return base.DoesAcceptEvent(e) && (isLeftClick || e.IsKeyPressEvent) && e.PickedNode.GetType() == (new PCamera().GetType());
        }
    }

    
}
