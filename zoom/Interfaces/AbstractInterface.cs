using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMD.HCIL.Piccolo;
using UMD.HCIL.Piccolo.Nodes;
using UMD.HCIL.Piccolo.Event;
using System.Drawing;
using System.Windows.Forms;
using UMD.HCIL.Piccolo.Util;

namespace zoom.Interfaces
{
    /// <summary>
    /// AbstractInterface handles the common logic for the text-entry interfaces 
    /// </summary>
    public abstract class AbstractInterface : PNode
    {
        /// <summary>
        /// The box that the user sees when they activate the interface
        /// </summary>
        public PPath Background { get; protected set; }

        /// <summary>
        /// The text box the user's input is placed into
        /// </summary>
        public PText Entry { get; protected set; }

        /// <summary>
        /// The icon that differentiates one interface from another
        /// </summary>
        public PImage Icon { get; protected set; }

        /// <summary>
        /// The smallest Width the text entry section of the interface can take
        /// </summary>
        public virtual int MinWidth { get { return 320; } }

        /// <summary>
        /// The Height of the text entry section of the interface
        /// </summary>
        public virtual int TextHeight { get { return 60; } }

        /// <summary>
        /// The size of the icon
        /// </summary>
        public virtual RectangleF IconBounds
        {
            get
            {
                GraphicsUnit pixel = GraphicsUnit.Pixel;
                return Icon.Image.GetBounds(ref pixel);
            }
        }

        /// <summary>
        /// The padding between any two items in the interface
        /// </summary>
        public virtual int Padding { get { return 20; } }

        /// <summary>
        /// Handles text entry into the interface
        /// </summary>
        protected InterfaceTextEntryHandler Handler;

        /// <summary>
        /// Create a new interface with a specific icon
        /// </summary>
        /// <param Name="icon"></param>
        protected AbstractInterface(PImage icon)
        {
            //Create the icon
            Icon = icon;
            Icon.Bounds = new RectangleF(MinWidth + Padding, Padding, IconBounds.Width, IconBounds.Height);
            AddChild(Icon);

            //Create the background
            Background = PPath.CreateRectangle(0, 0, MinWidth + IconBounds.Width + (Padding * 2), 100);
            AddChild(Background);

            //Create the text entry field
            Entry = new PText();
            Entry.ConstrainHeightToTextHeight = false;
            Entry.Bounds = new RectangleF(Padding, Padding, 0, TextHeight);
            Entry.Font = new Font("Century Gothic", 32);
            AddChild(Entry);

            //Create the text entry handler
            Handler = new InterfaceTextEntryHandler(this);
            Entry.AddInputEventListener(Handler);
        }

        /// <summary>
        /// Fired when the user releases the key, performing the action of the interface
        /// </summary>
        /// <param Name="sender"></param>
        /// <param Name="e"></param>
        public abstract void Execute(object sender, PInputEventArgs e);

        /// <summary>
        /// Fired when the user first depresses the key
        /// </summary>
        /// <param Name="sender"></param>
        /// <param Name="e"></param>
        public abstract void Activate(object sender, PInputEventArgs e);

        /// <summary>
        /// Fired every time the user depresses the key
        /// </summary>
        /// <param Name="sender"></param>
        /// <param Name="e"></param>
        public abstract void RegisterActivateButtonPress(object sender, PInputEventArgs e);

        /// <summary>
        /// Based on the event args, should the interface be inititated / executed?
        /// </summary>
        /// <remarks>This is only for showing or executing the interface. Text entry is handled seperately</remarks>
        /// <param Name="e">The event to respond to</param>
        /// <returns>Whether the interface should respond to the event</returns>
        public abstract bool Accepts(PInputEventArgs e);

        /// <summary>
        /// Determine whether two Keys values are exactly equal
        /// </summary>
        /// <param Name="expected">The expected value</param>
        /// <param Name="actual">The actual value</param>
        /// <returns>Whether the expected value exactly matches the actual value</returns>
        protected static bool MatchKeys(Keys expected, Keys actual) { return (expected & actual) == expected && (expected ^ actual) == 0; }
    }
}
