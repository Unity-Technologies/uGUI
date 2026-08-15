using System;
using System.Collections.Generic;

namespace UnityEngine.UI.Tweening
{
    internal interface ITweenValue
    {
        void TweenValue(float floatPercentage);
        bool ignoreTimeScale { get; }
        float duration { get; }
        bool ValidTarget();
    }

    internal struct ColorTween : ITweenValue
    {
        public enum ColorTweenMode
        {
            All,
            RGB,
            Alpha
        }

        private Action<Color> m_Target;
        private Color m_StartColor;
        private Color m_TargetColor;
        private ColorTweenMode m_TweenMode;

        private float m_Duration;
        private bool m_IgnoreTimeScale;

        public Color startColor
        {
            get { return m_StartColor; }
            set { m_StartColor = value; }
        }

        public Color targetColor
        {
            get { return m_TargetColor; }
            set { m_TargetColor = value; }
        }

        public ColorTweenMode tweenMode
        {
            get { return m_TweenMode; }
            set { m_TweenMode = value; }
        }

        public float duration
        {
            get { return m_Duration; }
            set { m_Duration = value; }
        }

        public bool ignoreTimeScale
        {
            get { return m_IgnoreTimeScale; }
            set { m_IgnoreTimeScale = value; }
        }

        public void TweenValue(float floatPercentage)
        {
            if (!ValidTarget())
                return;

            Color newColor;
            // Avoid lerping channels that will be discarded or overwritten.
            if (m_TweenMode == ColorTweenMode.Alpha)
            {
                newColor = m_StartColor;
                newColor.a = Mathf.LerpUnclamped(m_StartColor.a, m_TargetColor.a, floatPercentage);
            }
            else
            {
                // All and RGB both need the full RGB lerp; RGB just pins alpha to start.
                newColor = Color.LerpUnclamped(m_StartColor, m_TargetColor, floatPercentage);
                if (m_TweenMode == ColorTweenMode.RGB)
                {
                    newColor.a = m_StartColor.a;
                }
            }
            m_Target.Invoke(newColor);
        }

        public void SetChangedCallback(Action<Color> callback)
        {
            m_Target = callback;
        }

        public bool ValidTarget()
        {
            return m_Target != null;
        }
    }

    internal struct FloatTween : ITweenValue
    {
        private Action<float> m_Target;
        private float m_StartValue;
        private float m_TargetValue;

        private float m_Duration;
        private bool m_IgnoreTimeScale;

        public float startValue
        {
            get { return m_StartValue; }
            set { m_StartValue = value; }
        }

        public float targetValue
        {
            get { return m_TargetValue; }
            set { m_TargetValue = value; }
        }

        public float duration
        {
            get { return m_Duration; }
            set { m_Duration = value; }
        }

        public bool ignoreTimeScale
        {
            get { return m_IgnoreTimeScale; }
            set { m_IgnoreTimeScale = value; }
        }

        public void TweenValue(float floatPercentage)
        {
            if (!ValidTarget())
                return;

            // floatPercentage is already clamped by the caller; use LerpUnclamped to skip the internal Clamp01.
            var newValue = Mathf.LerpUnclamped(m_StartValue, m_TargetValue, floatPercentage);
            m_Target.Invoke(newValue);
        }

        public void SetChangedCallback(Action<float> callback)
        {
            m_Target = callback;
        }

        public bool ValidTarget()
        {
            return m_Target != null;
        }


    }

    internal interface ITweenRunner
    {
        bool Tick(float deltaTime, float unscaledDeltaTime);
    }

    internal class TweenManager
    {
        private static TweenManager s_Instance;

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void ResetStaticsOnLoad()
        {
            if (s_Instance != null)
            {
                Canvas.willRenderCanvases -= s_Instance.PerformUpdate;
                s_Instance = default;
            }
        }
#endif

        private readonly List<ITweenRunner> m_ActiveTweens = new List<ITweenRunner>();
        private readonly List<ITweenRunner> m_PendingRegistrations = new List<ITweenRunner>();
        private readonly List<ITweenRunner> m_PendingUnregistrations = new List<ITweenRunner>();
        private bool m_PerformingUpdate;

        private TweenManager()
        {
            Canvas.willRenderCanvases += PerformUpdate;
        }

        public static TweenManager instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = new TweenManager();
                }
                return s_Instance;
            }
        }

        public void Register(ITweenRunner runner)
        {
            // Defer registration if we're mid-update (e.g. a tween callback starts a new tween).
            if (m_PerformingUpdate)
            {
                m_PendingRegistrations.Add(runner);
                return;
            }
            m_ActiveTweens.Add(runner);
        }

        public void Unregister(ITweenRunner runner)
        {
            // Defer unregistration if we're mid-update (e.g. a tween callback stops another tween).
            if (m_PerformingUpdate)
            {
                m_PendingUnregistrations.Add(runner);
                return;
            }
            m_ActiveTweens.Remove(runner);
        }

        private void PerformUpdate()
        {
            m_PerformingUpdate = true;
            float dt = Time.deltaTime;
            float unscaledDt = Time.unscaledDeltaTime;

            for (int i = m_ActiveTweens.Count - 1; i >= 0; i--)
            {
                if (!m_ActiveTweens[i].Tick(dt, unscaledDt))
                {
                    // Swap with the last element before removing to avoid O(n) element shifting.
                    // Safe with backward iteration: the element swapped in from Count-1 was
                    // already ticked this frame, so skipping it at position i is correct.
                    int last = m_ActiveTweens.Count - 1;
                    m_ActiveTweens[i] = m_ActiveTweens[last];
                    m_ActiveTweens.RemoveAt(last);
                }
            }

            m_PerformingUpdate = false;

            // Flush deferred operations now that iteration is done.
            for (int i = 0; i < m_PendingUnregistrations.Count; i++)
            {
                m_ActiveTweens.Remove(m_PendingUnregistrations[i]);
            }

            m_PendingUnregistrations.Clear();

            for (int i = 0; i < m_PendingRegistrations.Count; i++)
            {
                m_ActiveTweens.Add(m_PendingRegistrations[i]);
            }

            m_PendingRegistrations.Clear();
        }
    }

    internal class TweenRunner<T> : ITweenRunner where T : struct, ITweenValue
    {
        protected MonoBehaviour m_Owner;
        private T m_TweenInfo;
        private float m_ElapsedTime;
        private float m_Duration;        // Cached to avoid property access on the hot path.
        private float m_InverseDuration; // Precomputed 1/duration to replace per-tick division with multiply.
        private bool m_IgnoreTimeScale;  // Cached to avoid property access on the hot path.
        private bool m_IsRunning;

        public void Init(MonoBehaviour owner)
        {
            m_Owner = owner;
        }

        public void StartTween(T info)
        {
            if (m_Owner == null)
            {
                Debug.LogWarning("Tween owner not configured... did you forget to call Init?");
                return;
            }

            StopTween();

            if (!m_Owner.gameObject.activeInHierarchy)
            {
                info.TweenValue(1.0f);
                return;
            }

            if (!info.ValidTarget())
                return;

            m_TweenInfo = info;
            m_ElapsedTime = 0.0f;
            m_Duration = info.duration;
            m_InverseDuration = m_Duration > 0f ? 1f / m_Duration : float.MaxValue;
            m_IgnoreTimeScale = info.ignoreTimeScale;
            m_IsRunning = true;
            TweenManager.instance.Register(this);
        }

        public void StopTween()
        {
            if (m_IsRunning)
            {
                m_IsRunning = false;
                TweenManager.instance.Unregister(this);
            }
        }

        bool ITweenRunner.Tick(float deltaTime, float unscaledDeltaTime)
        {
            if (!m_IsRunning)
            {
                return false;
            }

            // owner gameObject can be deactivated or destroyed.
            if (m_Owner == null || m_Owner.gameObject == null || !m_Owner.gameObject.activeInHierarchy)
            {
                m_IsRunning = false;
                return false;
            }

            m_ElapsedTime += m_IgnoreTimeScale ? unscaledDeltaTime : deltaTime;

            // Check completion before computing percentage to avoid a redundant TweenValue call.
            if (m_ElapsedTime >= m_Duration)
            {
                m_TweenInfo.TweenValue(1.0f);
                m_IsRunning = false;
                return false;
            }

            // Use precomputed inverse to replace division with a multiply.
            // floatPercentage is guaranteed in [0, 1) here, so LerpUnclamped is safe.
            m_TweenInfo.TweenValue(m_ElapsedTime * m_InverseDuration);
            return true;
        }
    }
}
