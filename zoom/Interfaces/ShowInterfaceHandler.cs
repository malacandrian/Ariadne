using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UMD.HCIL.Piccolo;
using UMD.HCIL.Piccolo.Event;
using UMD.HCIL.Piccolo.Util;

namespace zoom.Interfaces
{
    /// <summary>
    /// Handles showing, hiding, and executing interfaces
    /// </summary>
    public class ShowInterfaceHandler : PBasicInputEventHandler
    {
        /// <summary>
        /// The PCamera the interface should be attached to
        /// </summary>
        public PCamera Camera { get; protected set; }

        /// <summary>
        /// The interface to handle events for
        /// </summary>
        public AbstractInterface Interface { get; protected set; }

        /// <summary>
        /// Whether the key is currently pressed
        /// </summary>
        public bool IsPressed { get; protected set; }

        /// <summary>
        /// What to restore keyboard focus to when the 
        /// </summary>
        public PPickPath KeyFocus;

        /// <summary>
        /// Create a new ShowInterfaceHandler for a specific Interface
        /// </summary>
        /// <param Name="camera">The PCamera to attach the interface to</param>
        /// <param Name="showInterface">The interface this should control</param>
        public ShowInterfaceHandler(PCamera camera, AbstractInterface showInterface)
        {
            Camera = camera;
            IsPressed = false;
            Interface = showInterface;
            
            //This handler needs to be hooked in to the interface so it can listen for the keyup event
            Interface.Entry.AddInputEventListener(this);

        }

        /// <summary>
        /// Whether the handler fires for a specific event
        /// </summary>
        /// <param Name="e">The event to test</param>
        /// <returns>Whether it fires</returns>
        public override bool DoesAcceptEvent(PInputEventArgs e)
        {
            //If either the user presses escape, or the interface will accept the event, accept the event
            if ((Interface.Accepts(e) || (e.IsKeyEvent && e.KeyCode == Keys.Escape)) && base.DoesAcceptEvent(e))
            {
                e.Handled = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Show the interface if it isn't already being shown
        /// 
        /// If it is being shown, and the user presses escape, remove it
        /// </summary>
        public override void OnKeyDown(object sender, PInputEventArgs e)
        {
            base.OnKeyDown(sender, e);
            //If the interface isn't shown, and should be
            if (IsPressed == false && Interface.Accepts(e))
            {
                //Clear the text from the last time it was used, and show it to the user
                Interface.Entry.Text = "";
                Camera.AddChild(Interface);

                //Fetch the current keyboard focus to save for later, then shift the keyboard focus to the interface
                if (Page.LastPage != null) { KeyFocus = Page.LastPage.ToPickPath(); }
                else { KeyFocus = Camera.ToPickPath(); }
                e.InputManager.KeyboardFocus = Interface.Entry.ToPickPath(e.Camera, Interface.Entry.Bounds);
                
                //Activate the interface
                IsPressed = true;
                Interface.Activate(sender, e);
            }
            
            //If the user pressed ecape, remove the interface
            else if (e.KeyCode == Keys.Escape) { RemoveInterface(e); }

            //Register the Activate button press, if appropriate
            if (Interface.Accepts(e)) { Interface.RegisterActivateButtonPress(sender, e); }
        }

        /// <summary>
        /// Execute the code of the interface when the user releases the key
        /// </summary>
        public override void OnKeyUp(object sender, PInputEventArgs e)
        {
            //If the interface exists, execute its code then remove it
            if (IsPressed)
            {
                base.OnKeyUp(sender, e);
                Interface.Execute(sender, e);
                RemoveInterface(e);
            }
        }

        /// <summary>
        /// Remove the interface from the camera and restore keyboard focus
        /// </summary>
        protected void RemoveInterface(PInputEventArgs e)
        {
            if (Camera.IndexOfChild(Interface) >= 0)
            {
                Camera.RemoveChild(Interface);
                e.InputManager.KeyboardFocus = KeyFocus;
                IsPressed = false;
            }
        }
    }
}
