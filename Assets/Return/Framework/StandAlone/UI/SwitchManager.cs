using UnityEngine;
using UnityEngine.Events;

namespace Michsky.UI.Zone
{
    public class SwitchManager : UIElemant
    {
        [Header("SWITCH")]
        public bool isOn;
        private Animator switchAnimator;

        [Header("EVENT")]
        public UnityEvent onEvent;
        public UnityEvent offEvent;
        public UnityEvent<bool> OnValueChange;

        const string onTransition = "Switch On";
        const string offTransition = "Switch Off";

        protected override void Awake()
        {
            base.Awake();
            switchAnimator = gameObject.GetComponent<Animator>();
        }

        void Start()
        {
            switchAnimator.Play(offTransition);

            if (isOn == true)
            {
                switchAnimator.Play(onTransition);
                //onEvent.Invoke();
            }

            else
            {
                switchAnimator.Play(offTransition);
                //offEvent.Invoke();
            }
        }
        public override void ReleaseElement()
        {
            //Debug.Log("Release : " + this);
            OnValueChange.RemoveAllListeners();
            base.ReleaseElement();
        }

        public virtual void SetValue(bool enable)
        {
            if (isOn == enable)
                return;

            if(enable)
                switchAnimator.Play(onTransition);
            else
                switchAnimator.Play(offTransition);

            isOn = enable;
        }

        public void AnimateSwitch()
        {
            if (isOn == true)
            {
                switchAnimator.Play(offTransition);
                offEvent.Invoke();
                isOn = false;
            }
            else
            {
                switchAnimator.Play(onTransition);
                onEvent.Invoke();
                isOn = true;
            }

            OnValueChange.Invoke(isOn);
        }
    }
}
