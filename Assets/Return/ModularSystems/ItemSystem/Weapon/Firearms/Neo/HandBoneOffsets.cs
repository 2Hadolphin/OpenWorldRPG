using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/weaponref-so-handboneoffsets.html")]
    [CreateAssetMenu(fileName = "HandBoneOffsets", menuName = "NeoFPS/Hand Bone Offsets")]
    public class HandBoneOffsets : ScriptableObject
    {

        [SerializeField]
        private Vector3 m_LeftHandPositionOffset = Vector3.zero;
        [SerializeField]
        private Vector3 m_LeftHandRotationOffset = Vector3.zero;
        [SerializeField]
        private Vector3 m_RightHandPositionOffset = Vector3.zero;
        [SerializeField]
        private Vector3 m_RightHandRotationOffset = Vector3.zero;
        [SerializeField]
        private bool m_OffsetLeftFingers = true;
        [SerializeField]
        private bool m_OffsetRightFingers = true;
        [SerializeField]
        private Vector3[] m_FingerOffsets = new Vector3[30];

#if UNITY_EDITOR
#pragma warning disable CS0414
        [HideInInspector] public bool expandLeftHand = true;
        [HideInInspector] public bool expandRightHand = true;
#pragma warning restore CS0414

        private void OnValidate()
        {
            m_LeftHandPositionOffset = ClampPosition(m_LeftHandPositionOffset);
            m_RightHandPositionOffset = ClampPosition(m_RightHandPositionOffset);
            m_LeftHandRotationOffset = ClampRotation(m_LeftHandRotationOffset);
            m_RightHandRotationOffset = ClampRotation(m_RightHandRotationOffset);
            for (int i = 0; i < 30; ++i)
                m_FingerOffsets[i] = ClampRotation(m_FingerOffsets[i]);
        }

        Vector3 ClampPosition(Vector3 input)
        {
            input.x = Mathf.Clamp(input.x, -1f, 1f);
            input.y = Mathf.Clamp(input.y, -1f, 1f);
            input.z = Mathf.Clamp(input.z, -1f, 1f);
            return input;
        }

        Vector3 ClampRotation(Vector3 input)
        {
            input.x = Mathf.Clamp(input.x, -180f, 180f);
            input.y = Mathf.Clamp(input.y, -180f, 180f);
            input.z = Mathf.Clamp(input.z, -180f, 180f);
            return input;
        }
#endif

        public Vector3 leftHandPositionOffset
        {
            get { return m_LeftHandPositionOffset; }
        }

        public Vector3 rightHandPositionOffset
        {
            get { return m_RightHandPositionOffset; }
        }

        public bool offsetLeftFingers
        {
            get { return m_OffsetLeftFingers; }
        }

        public bool offsetRightFingers
        {
            get { return m_OffsetRightFingers; }
        }

#if UNITY_EDITOR

        public Quaternion leftHandRotationOffset
        {
            get { return Quaternion.Euler(m_LeftHandRotationOffset); }
        }

        public Quaternion rightHandRotationOffset
        {
            get { return Quaternion.Euler(m_RightHandRotationOffset); }
        }

        public Quaternion GetFingerRotation(HumanBodyBones bone)
        {
            return Quaternion.Euler(m_FingerOffsets[(int)bone - 24]);
        }

#else

        Quaternion[] m_FingerRotations = new Quaternion[30];

        public Quaternion leftHandRotationOffset
        {
            get;
            private set;
        }

        public Quaternion rightHandRotationOffset
        {
            get;
            private set;
        }
        
        public Quaternion GetFingerRotation(HumanBodyBones bone)
        {
            return m_FingerRotations[(int)bone - 24];
        }

        void Awake()
        {
            // Get hand rotations
            leftHandRotationOffset = Quaternion.Euler(m_LeftHandRotationOffset);
            rightHandRotationOffset = Quaternion.Euler(m_RightHandRotationOffset);

            // Get finger rotations
            for (int i = 0; i < 30; ++i)
                m_FingerRotations[i] = Quaternion.Euler(m_FingerOffsets[i]);
            m_FingerOffsets = null;
        }

#endif
    }
}
