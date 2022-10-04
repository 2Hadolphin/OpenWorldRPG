using UnityEngine;
using UnityEngine.UI;

namespace Michsky.UI.Zone
{
    public class SliderGamepadManager : UIElemant
    {
        [Header("SLIDER")]
        public Slider mainSlider;
        public float changeValue = 0.5f;

        public override void ReleaseElement()
        {
            mainSlider.onValueChanged.RemoveAllListeners();
            base.ReleaseElement();
        }

        //[Header("INPUT")]
        //public string horizontalAxis = "Xbox Right Stick Horizontal";

        //void Update()
        //{
        //    float h = Inputs.GetAxis(horizontalAxis);

        //    if (h == 1)
        //    {
        //        sliderObject.value += changeValue;
        //    }

        //    else if (h == -1)
        //    {
        //        sliderObject.value -= changeValue;
        //    }
        //}
    }
}