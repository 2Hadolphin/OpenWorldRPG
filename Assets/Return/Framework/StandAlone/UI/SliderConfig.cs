using UnityEngine.UI;
using UnityEngine.Events;

public class SliderConfig : ValueConfig<float>, INormalField
{
    public bool WholeNumber = false;
    public float MaxValue;
    public float MinValue;

    public virtual void Apply(Slider slider)
    {
        slider.wholeNumbers = WholeNumber;
        slider.maxValue = MaxValue;
        slider.minValue = MinValue;
        slider.value = Value;

        slider.onValueChanged.AddListener(new UnityAction<float>(SetValue));
    }
}
