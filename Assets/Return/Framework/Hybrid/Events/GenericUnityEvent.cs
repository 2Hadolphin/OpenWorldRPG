using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Return
{
    [Serializable] public class GameObjectEvent : UnityEvent<GameObject> { }

    [Serializable] public class SpriteEvent : UnityEvent<Sprite> { }

    [Serializable] public class TransformEvent : UnityEvent<Transform> { }

    [Serializable] public class RayCastHitEvent : UnityEvent<RaycastHit> { }

    [Serializable] public class Vector3Event : UnityEvent<Vector3> { }

    [Serializable] public class Vector2Event : UnityEvent<Vector2> { }

    [Serializable] public class ColorEvent : UnityEvent<Color> { }

    [Serializable] public class IntEvent : UnityEvent<int> { }

    [Serializable] public class Int2Event : UnityEvent<int, int> { }

    [Serializable] public class FloatEvent : UnityEvent<float> { }

    [Serializable] public class BoolEvent : UnityEvent<bool> { }

    [Serializable] public class StringEvent : UnityEvent<string> { }

    [Serializable] public class ColliderEvent : UnityEvent<Collider> { }

    [Serializable] public class CollisionEvent : UnityEvent<Collision> { }

    [Serializable] public class ComponentEvent : UnityEvent<Component> { }
}