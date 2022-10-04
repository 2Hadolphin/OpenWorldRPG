using UnityEngine.InputSystem;
using Return.Inputs;

namespace Return.Humanoid
{



    public partial class Slide
    {
        public class InputHandle : UserInputExtendHandle<Slide>
        {
            protected override void SubscribeInput()
            {
                Input.Humanoid.Squat.Subscribe(Slide);
            }

            protected override void UnsubscribeInput()
            {
                Input.Humanoid.Squat.Unsubscribe(Slide);
            }

 
            private void Slide(InputAction.CallbackContext obj)
            {
                module.DoSlide();
            }
        }

    }
}