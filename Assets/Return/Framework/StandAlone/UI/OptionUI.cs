using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TheraBytes.BetterUi;
using TMPro;
using System;

namespace Return.UI
{

    /// <summary>
    /// Universal dialogue callback window.
    /// </summary>
    public class OptionUI : CustomWindow
    {
        /// <summary>
        /// Preset UI animation call
        /// </summary>
        public UnityEvent Display;

        [SerializeField]
        public TextMeshProUGUI Description;

        [SerializeField]
        public TextMeshProUGUI[] Description_Yes = new TextMeshProUGUI[2];

        [SerializeField]
        public TextMeshProUGUI[] Description_No = new TextMeshProUGUI[2];

        string DefaultYes, DefaultNo;

        private void Awake()
        {
            DefaultYes = Description_Yes[0].text;
            DefaultNo = Description_No[0].text;
        }

        public virtual void RestOption()
        {
            //if (string.IsNullOrEmpty(yes))
            //    yes = DefaultYes;

            //if (string.IsNullOrEmpty(no))
            //    no = DefaultNo;
            foreach (var ans in Description_Yes)
            {
                ans.text = DefaultYes;
            }

            foreach (var ans in Description_No)
            {
                ans.text = DefaultNo;
            }

        }

        /// <summary>
        /// Set UI operation dialogue.
        /// </summary>
        public virtual void SetOption(string yes = null, string no = null)
        {
            foreach (var ans in Description_Yes)
            {
                if (ans)
                    ans.text = yes;
            }

            foreach (var ans in Description_No)
            {
                if (ans)
                    ans.text = no;
            }
        }

        /// <summary>
        /// Set UI operation callback.
        /// </summary>
        public virtual void SetCallback(Action @true = null, Action @false = null)
        {
            Button_Yes.onClick.RemoveAllListeners();
            Button_No.onClick.RemoveAllListeners();

            if (null != @true)
                Button_Yes.onClick.AddListener(new UnityAction(@true));
            if (null != @false)
                Button_No.onClick.AddListener(new UnityAction(@false));

            Display.Invoke();
        }

        [SerializeField]
        public Button Button_Yes;

        [SerializeField]
        public Button Button_No;

    }
}