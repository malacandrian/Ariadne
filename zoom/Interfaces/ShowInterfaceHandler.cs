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
    public class ShowInterfaceHandler : PBasicInputEventHandler
    {

        public PCamera Camera { get; protected set; }
        public AbstractInterface Interface { get; protected set; }
        public bool IsPressed { get; protected set; }
        private PPickPath keyFocus;
        public Keys ActivateKey { get; protected set; }

        public ShowInterfaceHandler(PCamera c, Keys activateKey, AbstractInterface showInterface)
        {
            Camera = c;
            IsPressed = false;
            ActivateKey = activateKey;
            Interface = showInterface;
            //So it can detect the activate key being lifted
            Interface.Entry.AddInputEventListener(this);

        }

        public override bool DoesAcceptEvent(PInputEventArgs e)
        {
            if (Interface.Accepts(e) && base.DoesAcceptEvent(e))
            {
                e.Handled = true;
                return true;
            }
            return false;
        }

        public override void OnKeyDown(object sender, PInputEventArgs e)
        {
            if (!IsPressed)
            {
                base.OnKeyDown(sender, e);


                Interface.Press(sender, e);

                Interface.Entry.Text = "";
                Camera.AddChild(Interface);
                keyFocus = e.InputManager.KeyboardFocus;
                e.InputManager.KeyboardFocus = Interface.Entry.ToPickPath(e.Camera, Interface.Entry.Bounds);


            }
            IsPressed = true;
        }

        public override void OnKeyUp(object sender, PInputEventArgs e)
        {
            if (IsPressed)
            {
                base.OnKeyUp(sender, e);

                //Execute the code
                Interface.Release(sender, e);

                //Remove Command Interface
                Camera.RemoveChild(Interface);
                e.InputManager.KeyboardFocus = keyFocus;
                IsPressed = false;
            }
        }
    }
}
