using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

namespace Michsky.UI.Zone
{
    public class HorizontalSelector : UIElemant
    {
        const string Forward = "Forward";
        const string Previous = "Previous";

        [Header("GAMEPAD")]
        public bool useGamepadButtons = true;
        public KeyCode previousButton;
        public KeyCode forwardButton;

        [Header("SETTINGS")]
        public int Index = 0;

        //public bool invokeEventAtStart = false;
        public bool disableAtStart = true;

        [Header("ELEMENTS")]
        public List<string> Options = new ();

        [Header("EVENT")]
        public UnityEvent<int> onValueChanged;

        private TextMeshProUGUI label;
        private TextMeshProUGUI labeHelper;
        private Animator selectorAnimator;

        protected override void Awake()
        {
            base.Awake();
            selectorAnimator = gameObject.GetComponent<Animator>();
            label = transform.Find("Text").GetComponent<TextMeshProUGUI>();
            labeHelper = transform.Find("Text Helper").GetComponent<TextMeshProUGUI>();


            //if(invokeEventAtStart == true)
            //    onValueChanged.Invoke(index);
            


            if (disableAtStart == true)
            {
                this.enabled = false;
            }
        }

        public virtual void Activate()
        {
            enabled = true;
        }

        public virtual void Deactivate()
        {
            enabled = false;
        }

        private void OnEnable()
        {
            //??
            if (Options.Count > Index)
            {
                label.text = Options[Index];
                labeHelper.text = label.text;
            }

            //go interface orlink Input
            // link systemcatch Input to do PreviousClick();
        }

        private void OnDisable()
        {
            // link systemcatch Input to cancel PreviousClick();
        }

        void Update()
        {
            //if (useGamepadButtons == true)
            //{
            //    if (Inputs.GetKeyDown(previousButton))
            //    {
            //        PreviousClick();
            //    }

            //    else if (Inputs.GetKeyDown(forwardButton))
            //    {
            //        ForwardClick();
            //    }
            //}
        }

        public virtual void AddOption(params string[] options)
        {
            Options.AddRange(options);
            label.text = Options[Index];
        }

        public void PreviousClick()
        {
            labeHelper.text = label.text;

            if (Index == 0)
            {
                Index = Options.Count - 1;
            }
            else
            {
                Index--;
            }

            onValueChanged.Invoke(Index);
            label.text = Options[Index];

            selectorAnimator.Play(null);
            selectorAnimator.StopPlayback();
            selectorAnimator.Play(Previous);
        }

        public void ForwardClick()
        {
            labeHelper.text = label.text;

            if ((Index + 1) >= Options.Count)
            {
                Index = 0;
            }

            else
            {
                Index++;
            }

            onValueChanged.Invoke(Index);
            label.text = Options[Index];

            selectorAnimator.Play(null);
            selectorAnimator.StopPlayback();
            selectorAnimator.Play(Forward);
        }
    }
}