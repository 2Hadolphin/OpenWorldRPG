using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Michsky.UI.Zone
{
    public class UIElementSound : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
    {
        [Header("SETTINGS")]
        public bool enableHoverSound = true;
        public bool HoverChild=false;

        public bool enableClickSound = true;
        public bool ClickChild = true;
        public bool isNotification = false;

        void PlayUIPassBy()
        {
            UIAudioPlayer.PlayUIPassBy();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (enableHoverSound&&(HoverChild || eventData.pointerEnter == gameObject))
                UIAudioPlayer.PlayUIPassBy();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (enableClickSound && (ClickChild || eventData.pointerClick == gameObject))
                UIAudioPlayer.PlayUIClick();

        }

        public void Notification()
        {
            if (isNotification == true)
            {
                UIAudioPlayer.PlayUIClick();
            }
        }


    }
}