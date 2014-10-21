using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UMD.HCIL.Piccolo.Event;

namespace zoom.Interfaces
{
    /// <summary>
    /// Handles text entry for interfaces
    /// </summary>
    public class InterfaceTextEntryHandler : PBasicInputEventHandler
    {
        /// <summary>
        /// Handles any changes to the Width of the input box
        /// </summary>
        public delegate void WidthUpdateHandler();

        /// <summary>
        /// Fires when the Width of the entry box changes
        /// </summary>
        public WidthUpdateHandler UpdateWidth;

        /// <summary>
        /// The interface this is handling text entry for
        /// </summary>
        public AbstractInterface Owner { get; protected set; }

        /// <summary>
        /// Create a new InterfaceTextEntryHandler for a specific interface
        /// </summary>
        /// <param Name="owner">The interface this is handling text entry for</param>
        public InterfaceTextEntryHandler(AbstractInterface owner)
        {
            Owner = owner;
            UpdateWidth += OnUpdateWidth;
        }

        /// <summary>
        /// Accept key events
        /// </summary>
        /// <param Name="e">The event it's deciding whether to accept</param>
        /// <returns></returns>
        public override bool DoesAcceptEvent(PInputEventArgs e) { return e.IsKeyEvent && base.DoesAcceptEvent(e); }

        /// <summary>
        /// When the user presses a key, handle it appropriately
        /// </summary>
        /// <param Name="sender">The object that fired the event</param>
        /// <param Name="e">The key press</param>
        public override void OnKeyDown(object sender, PInputEventArgs e)
        {
            base.OnKeyDown(sender, e);
            char c = (char)e.KeyCode;

            //Keydown seems to assume they're all caps
            //All lower case looks better than all caps
            //So we'll force them to lower case
            if (c >= 'A' && c <= 'Z')
            {
                c = (char)(c + 32);
            }

            //If it's a letter or a number, add it to the text
            if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9')) { Owner.Entry.Text += c; }
            else
            {
                //Deal with control characters
                switch (c)
                {
                    case (char)Keys.Back: //Backspace
                        Owner.Entry.Text = Owner.Entry.Text.Substring(0, Owner.Entry.Text.Length - 1);
                        break;

                    case ' ': //Space
                        Owner.Entry.Text += ' ';
                        break;
                }
            }

            //Update the Width of the entry box
            UpdateWidth();
        }

        /// <summary>
        /// Ensures the text is never wider than the box its supposed to be entered in to
        /// </summary>
        protected void OnUpdateWidth()
        {
            //Take the minimum Width of the entry field, or the actual Width of the entry field, whichever is larger
            float basicWidth = Math.Max(Owner.MinWidth, Owner.Entry.Width);

            //Move the icon and resize the box as appropriate
            Owner.Background.Width = basicWidth + (Owner.Padding * 2) + Owner.IconBounds.Width;
            Owner.Icon.X = basicWidth + Owner.Padding;
        }
    }
}
