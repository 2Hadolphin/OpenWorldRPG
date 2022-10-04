using UnityEngine;
using NeoSaveGames.Serialization;
using NeoSaveGames;
using Return;
using Return.Cameras;
using Sirenix.OdinInspector;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/fpcamref-mb-weaponaimamplifier.html")]
	public class WeaponAimAmplifier : MonoBehaviour, IAdditiveTransform
    {
        [SerializeField, Range(-2f, 2f), Tooltip("The multiplier for the resulting weapon rotation side to side")]
        private float m_HorizontalMultiplier = 1f;

        [SerializeField, Range(-2f, 2f), Tooltip("The multiplier for the resulting weapon rotation up and down")]
        private float m_VerticalMultiplier = 1f;

        [SerializeField, Range(0f, 1f), Tooltip("How sensitive the sway is to camera rotation. Higher sensitivity means the sway approaches its peak with slower rotations")]
        private float m_Sensitivity = 0.5f;

        [SerializeField, Range(0.1f, 1f), Tooltip("Approximately the time it will take to reach the target rotation. A smaller value will reach the target faster")]
		private float m_DampingTime = 0.25f;

        private IAdditiveTransformHandler m_Handler = null;
        private IAimController m_AimController = null;
        [SerializeField]
        Transform Aimer;

        private Vector3 m_PreviousYawForward = Vector3.forward;
		private Vector3 m_EulerAngles = Vector3.zero;
		private Vector3 m_AngleDelta = Vector3.zero;
        private float m_PreviousPitch = 0f;

        [ShowInInspector]
        private float m_CurrentUserStrength = 0f;
        private float m_TargetUserStrength = 1f;
        private bool m_SkipReset = false;

        public Quaternion rotation
		{
            get;
            private set;
		}

		public Vector3 position
		{
			get { return Vector3.zero; }
        }

        public float strength
        {
            get { return m_TargetUserStrength; }
            set { m_TargetUserStrength = value; }
        }

        public bool bypassPositionMultiplier
        {
            get { return true; }
        }

        public bool bypassRotationMultiplier
        {
            get { return true; }
        }

        protected void Awake ()
		{
			m_Handler = GetComponent<IAdditiveTransformHandler>();
		}

        protected void OnEnable ()
        {
            if (m_Handler != null)
                m_Handler.ApplyAdditiveEffect(this);

            if (Aimer.NotNull())
            {

                if (!m_SkipReset)
                {
                    m_EulerAngles = Vector3.zero;
                    m_AngleDelta = Vector3.zero;

                    // Calculate forward
                    m_PreviousYawForward = Aimer.forward; ;
                    m_PreviousPitch = Aimer.localEulerAngles.x;
                }
                else
                    m_SkipReset = false;


                return;
            }

            m_AimController = GetComponentInParent<IAimController>();


            if (m_AimController != null)
            {
                if (!m_SkipReset)
                {
                    m_EulerAngles = Vector3.zero;
                    m_AngleDelta = Vector3.zero;

                    // Calculate forward
                    m_PreviousYawForward = m_AimController.heading;
                    m_PreviousPitch = m_AimController.pitch;
                }
                else
                    m_SkipReset = false;
            }
        }


        protected void OnDisable ()
        {
            if (m_Handler != null)
                m_Handler.RemoveAdditiveEffect (this);
		}

		public void UpdateTransform ()
        {
            // Interpolate user strength
            m_CurrentUserStrength = Mathf.Lerp(m_CurrentUserStrength, m_TargetUserStrength, ConstCache.deltaTime * 5f);

            // Get horizontal angle diff
            Vector3 yawForward = Aimer?Aimer.forward: m_AimController.heading;
            float hDiff = Vector3.SignedAngle(m_PreviousYawForward, yawForward, Aimer?Aimer.up: m_AimController.yawUp);
            m_PreviousYawForward = yawForward;

            // Get vertical angle diff
            float pitch = Aimer?Aimer.localEulerAngles.x: m_AimController.pitch;
            float vDiff = pitch - m_PreviousPitch;
            m_PreviousPitch = pitch;

            // Get multiplier from pitch
            float cosPitch = Mathf.Cos(pitch * Mathf.Deg2Rad);
            cosPitch = Mathf.Lerp(0.25f, 1f, cosPitch);

            float hInputRotationScale = Mathf.Lerp(0.01f, 0.1f, m_Sensitivity);
            float vInputRotationScale = Mathf.Lerp(0.02f, 0.2f, m_Sensitivity);

            // Damp the Input rotation
            m_EulerAngles = Vector3.SmoothDamp(
                m_EulerAngles,
                new Vector3(
                    hDiff * cosPitch * hInputRotationScale,
                    vDiff * cosPitch * vInputRotationScale,
                    0f),
                ref m_AngleDelta,
                m_DampingTime
            );

            // Use the damped Input rotation to get a logarithmic position offset
            float hOutScale = 7.5f * m_HorizontalMultiplier * m_CurrentUserStrength;
            float vOutScale = 7.5f * m_VerticalMultiplier * m_CurrentUserStrength;
            rotation = Quaternion.Euler(
                -Mathf.Log(Mathf.Abs(m_EulerAngles.y) + 1) * Mathf.Sign(m_EulerAngles.y) * vOutScale,
                Mathf.Log(Mathf.Abs(m_EulerAngles.x) + 1) * Mathf.Sign(m_EulerAngles.x) * hOutScale,
                0f
                );
        }
    }
}
