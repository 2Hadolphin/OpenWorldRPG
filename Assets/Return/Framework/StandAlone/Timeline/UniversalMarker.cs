using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using Sirenix.OdinInspector;
using UnityEngine.Playables;

namespace Return
{
    /// <summary>
    /// Timeline marker with UTag
    /// </summary>
    public class UniversalMarker : Marker, INotification,INotificationOptionProvider
    {
        public bool Retroactive;
        public bool EmitOnce;

        [SerializeField][HideInInspector]
        public UTagPicker EventID;

        

        public PropertyName id =>
#if UNITY_EDITOR
            new(EventID.Definition ? EventID.Definition.name : nameof(UniversalTagDefinition));
#else
            new(nameof(UniversalTagDefinition));
#endif

        public NotificationFlags flags =>
            (Retroactive ? NotificationFlags.Retroactive : default) |
            (EmitOnce ? NotificationFlags.TriggerOnce : default);
    }
}