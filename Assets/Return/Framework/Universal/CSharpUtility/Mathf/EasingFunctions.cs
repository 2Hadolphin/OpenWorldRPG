using UnityEngine;

namespace NeoFPS
{
    public static class EasingFunctions
    {
        private const float k_Pi = 3.1415926535897932384626433832795f;
        private const float k_FourPi = 12.566370614359172953850573533118f;
        private const float k_C1 = 1.70158f;
        private const float k_C3 = 2.70158f;

        // Quadratic easing

        public static float EaseInQuadratic(float x)
        {
            x = Mathf.Clamp01(x);
            return x * x;
        }

        public static float EaseOutQuadratic(float x)
        {
            float y = 1f - Mathf.Clamp01(x);
            return 1f - (y * y);
        }

        public static float EaseInOutQuadratic(float x)
        {
            x = Mathf.Clamp01(x);
            if (x < 0.5f)
            {
                return 2f * x * x;
            }
            else
            {
                float y = -2f * x + 2;
                return 1f - (y * y) * 0.5f;
            }
        }
        
        public static float EaseInQuadraticUnclamped(float x)
        {
            return x * x;
        }

        public static float EaseOutQuadraticUnclamped(float x)
        {
            float y = 1f - x;
            return 1f - (y * y);
        }

        public static float EaseInOutQuadraticUnclamped(float x)
        {
            if (x < 0.5f)
            {
                return 2f * x * x;
            }
            else
            {
                float y = -2f * x + 2;
                return 1f - (y * y) * 0.5f;
            }
        }

        // Cubic easing

        public static float EaseInCubic(float x)
        {
            x = Mathf.Clamp01(x);
            return x * x * x;
        }

        public static float EaseOutCubic(float x)
        {
            float y = 1f - Mathf.Clamp01(x);
            return 1f - (y * y * y);
        }

        public static float EaseInOutCubic(float x)
        {
            x = Mathf.Clamp01(x);
            if (x < 0.5f)
            {
                return 4f * x * x * x;
            }
            else
            {
                float y = -2f * x + 2;
                return 1f - (y * y * y) * 0.5f;
            }
        }

        public static float EaseInCubicUnclamped(float x)
        {
            return x * x * x;
        }

        public static float EaseOutCubicUnclamped(float x)
        {
            float y = 1f - x;
            return 1f - (y * y * y);
        }

        public static float EaseInOutCubicUnclamped(float x)
        {
            if (x < 0.5f)
            {
                return 4f * x * x * x;
            }
            else
            {
                float y = -2f * x + 2;
                return 1f - (y * y * y) * 0.5f;
            }
        }

        // Quartic easing

        public static float EaseInQuartic(float x)
        {
            x = Mathf.Clamp01(x);
            return x * x * x * x;
        }

        public static float EaseOutQuartic(float x)
        {
            float y = 1f - Mathf.Clamp01(x);
            return 1f - (y * y * y * y);
        }

        public static float EaseInOutQuartic(float x)
        {
            x = Mathf.Clamp01(x);
            if (x < 0.5f)
            {
                return 8f * x * x * x * x;
            }
            else
            {
                float y = -2f * x + 2;
                return 1f - (y * y * y * y) * 0.5f;
            }
        }

        public static float EaseInQuarticUnclamped(float x)
        {
            return x * x * x * x;
        }

        public static float EaseOutQuarticUnclamped(float x)
        {
            float y = 1f - x;
            return 1f - (y * y * y * y);
        }

        public static float EaseInOutQuarticUnclamped(float x)
        {
            if (x < 0.5f)
            {
                return 8f * x * x * x * x;
            }
            else
            {
                float y = -2f * x + 2;
                return 1f - (y * y * y * y) * 0.5f;
            }
        }

        // Overshoot

        public static float EaseInOvershoot(float x)
        {
            float y = 1f - Mathf.Clamp01(x);
            return 1f - k_C3 * y * y * y + k_C1 * y * y;
        }

        public static float EaseOutOvershoot(float x)
        {
            x = Mathf.Clamp01(x);
            return k_C3 * x * x * x - k_C1 * x * x;
        }

        public static float EaseInOvershootUnclamped(float x)
        {
            float y = 1f - x;
            return 1f - k_C3 * y * y * y + k_C1 * y * y;
        }

        public static float EaseOutOvershootUnclamped(float x)
        {
            return k_C3 * x * x * x - k_C1 * x * x;
        }

        // Spring

        public static float EaseInSpring(float x, float damping = 0f)
        {
            x = Mathf.Clamp01(x);
            float y = x * x * k_FourPi - k_Pi;
            float pow = Mathf.Pow(2f, Mathf.Lerp(-6.75f, -10f, damping) * x);
            return 1f + pow * Mathf.Cos(y);
        }

        public static float EaseOutSpring(float x, float damping = 0f)
        {
            return 1f - EaseInSpring(1f - x, damping);
        }

        public static float EaseInSpringUnclamped(float x, float damping = 0f)
        {
            float y = x * x * k_FourPi - k_Pi;
            float pow = Mathf.Pow(2f, Mathf.Lerp(-6.75f, -10f, damping) * x);
            return 1f + pow * Mathf.Cos(y);
        }

        public static float EaseOutSpringUnclamped(float x, float damping = 0f)
        {
            return 1f - EaseInSpringUnclamped(1f - x, damping);
        }

        // Bounce 

        public static float EaseInBounce(float x, float damping = 0f)
        {
            x = Mathf.Clamp01(x);
            float y = x * x * k_FourPi;
            float pow = Mathf.Pow(2f, Mathf.Lerp(-6.75f, -10f, damping) * x);
            return 1f - pow * Mathf.Abs(Mathf.Cos(y));
        }

        public static float EaseOutBounce(float x, float damping = 0f)
        {
            return 1f - EaseInBounce(1f - x, damping);
        }

        public static float EaseInBounceUnclamped(float x, float damping = 0f)
        {
            float y = x * x * k_FourPi;
            float pow = Mathf.Pow(2f, Mathf.Lerp(-6.75f, -10f, damping) * x);
            return 1f - pow * Mathf.Abs(Mathf.Cos(y));
        }

        public static float EaseOutBounceUnclamped(float x, float damping = 0f)
        {
            return 1f - EaseInBounceUnclamped(1f - x, damping);
        }
    }
}
