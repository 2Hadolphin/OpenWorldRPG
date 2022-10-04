using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using System;

namespace Return
{
    public class DefinitionMarker : Marker, INotification, INotificationOptionProvider,IEquatable<DefinitionMarker>
    {
        public bool Retroactive;
        public bool EmitOnce;

        [SerializeField]
        [HideInInspector]
        public UTags EventID;



        public PropertyName id => new(EventID.Definition ? EventID.Definition.name : nameof(UniversalTagDefinition));

        public NotificationFlags flags =>
            (Retroactive ? NotificationFlags.Retroactive : default) |
            (EmitOnce ? NotificationFlags.TriggerOnce : default);

        public bool Equals(DefinitionMarker other)
        {
            return other.EventID.Tag == EventID.Tag;
        }
    }

    public static class MarkerExtension
    {
        public static bool TryGetUTagMarker(this TimelineAsset asset, UTags tag, out IMarker marker)
        {
            return TryGetUTagMarker(asset.markerTrack, (x) => x.Equals(tag), out marker);
        }

        public static bool TryGetUTagMarker(this TimelineAsset asset, Func<UTags,bool> perdict, out IMarker marker)
        {
            return TryGetUTagMarker(asset.markerTrack, perdict, out marker);
        }

        public static bool TryGetUTagMarker(this MarkerTrack track, UTags tag, out IMarker marker)
        {
            return TryGetUTagMarker(track, (x) => x.Equals(tag), out marker);
        }

        public static bool TryGetUTagMarker(this MarkerTrack track, Func<UTags, bool> perdict, out IMarker marker)
        {
            var count = track.GetMarkerCount();

            for (int i = 0; i < count; i++)
            {
                var _marker = track.GetMarker(i);

                if(_marker is DefinitionMarker def &&  perdict(def.EventID))
                {
                    marker = _marker;
                    return true;
                }    
            }

            marker = null;
            return false;
        }
    }

}