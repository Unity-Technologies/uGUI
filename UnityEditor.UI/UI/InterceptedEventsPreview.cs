using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityEditor.Events
{
    [CustomPreview(typeof(GameObject))]
    class InterceptedEventsPreview : ObjectPreview
    {
        protected class ComponentInterceptedEvents
        {
            public GUIContent componentName;
            public GUIContent[] interceptedEvents;
        }

        class Styles
        {
            public GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
            public GUIStyle componentName = new GUIStyle(EditorStyles.boldLabel);

            public Styles()
            {
                Color fontColor = new Color(0.7f, 0.7f, 0.7f);
                labelStyle.padding.right += 20;
                labelStyle.normal.textColor    = fontColor;
                labelStyle.active.textColor    = fontColor;
                labelStyle.focused.textColor   = fontColor;
                labelStyle.hover.textColor     = fontColor;
                labelStyle.onNormal.textColor  = fontColor;
                labelStyle.onActive.textColor  = fontColor;
                labelStyle.onFocused.textColor = fontColor;
                labelStyle.onHover.textColor   = fontColor;

                componentName.normal.textColor = fontColor;
                componentName.active.textColor = fontColor;
                componentName.focused.textColor = fontColor;
                componentName.hover.textColor = fontColor;
                componentName.onNormal.textColor = fontColor;
                componentName.onActive.textColor = fontColor;
                componentName.onFocused.textColor = fontColor;
                componentName.onHover.textColor = fontColor;
            }
        }

        private Dictionary<GameObject, ComponentInterceptedEvents[]> m_TargetEvents;
        private bool m_InterceptsAnyEvent = false;
        private GUIContent m_Title;
        private Styles m_Styles = new Styles();

        public override void Initialize(UnityEngine.Object[] targets)
        {
            base.Initialize(targets);
            m_TargetEvents = new Dictionary<GameObject, ComponentInterceptedEvents[]>(targets.Count());
            m_InterceptsAnyEvent = false;
            foreach (var t in targets)
            {
                GameObject go = t as GameObject;
                ComponentInterceptedEvents[] interceptedEvents = GetEventsInfo(go);
                m_TargetEvents.Add(go, interceptedEvents);
                if (interceptedEvents.Any())
                    m_InterceptsAnyEvent = true;
            }
        }

        public override GUIContent GetPreviewTitle()
        {
            if (m_Title == null)
            {
                m_Title = new GUIContent("Intercepted Events");
            }
            return m_Title;
        }

        public override bool HasPreviewGUI()
        {
            return m_TargetEvents != null && m_InterceptsAnyEvent;
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            if (m_Styles == null)
                m_Styles = new Styles();

            Vector2 maxEventLabelSize = Vector2.zero;
            int totalInterceptedEvents = 0;

            ComponentInterceptedEvents[] componentIncerceptedEvents = m_TargetEvents[target as GameObject];

            // Find out the maximum size needed for any given label.
            foreach (ComponentInterceptedEvents componentInterceptedEvents in componentIncerceptedEvents)
            {
                foreach (GUIContent eventContent in componentInterceptedEvents.interceptedEvents)
                {
                    ++totalInterceptedEvents;
                    Vector2 labelSize = m_Styles.labelStyle.CalcSize(eventContent);
                    if (maxEventLabelSize.x < labelSize.x)
                    {
                        maxEventLabelSize.x = labelSize.x;
                    }
                    if (maxEventLabelSize.y < labelSize.y)
                    {
                        maxEventLabelSize.y = labelSize.y;
                    }
                }
            }

            // Apply padding
            RectOffset previewPadding = new RectOffset(-5, -5, -5, -5);
            r = previewPadding.Add(r);

            // Figure out how many rows and columns we can/should have
            int columns = Mathf.Max(Mathf.FloorToInt(r.width / maxEventLabelSize.x), 1);
            int rows = Mathf.Max(totalInterceptedEvents / columns, 1) + componentIncerceptedEvents.Length;

            // Centering
            float initialX = r.x + Mathf.Max(0, (r.width - (maxEventLabelSize.x * columns)) / 2);
            float initialY = r.y + Mathf.Max(0, (r.height - (maxEventLabelSize.y * rows)) / 2);

            Rect labelRect = new Rect(initialX, initialY, maxEventLabelSize.x, maxEventLabelSize.y);
            int currentColumn = 0;
            foreach (ComponentInterceptedEvents componentInterceptedEvents in componentIncerceptedEvents)
            {
                GUI.Label(labelRect, componentInterceptedEvents.componentName, m_Styles.componentName);
                labelRect.y += labelRect.height;
                labelRect.x = initialX;
                foreach (GUIContent eventContent in componentInterceptedEvents.interceptedEvents)
                {
                    GUI.Label(labelRect, eventContent, m_Styles.labelStyle);
                    if (currentColumn < columns - 1)
                    {
                        labelRect.x += labelRect.width;
                    }
                    else
                    {
                        labelRect.y += labelRect.height;
                        labelRect.x = initialX;
                    }

                    currentColumn = (currentColumn + 1) % columns;
                }

                if (labelRect.x != initialX)
                {
                    labelRect.y += labelRect.height;
                    labelRect.x = initialX;
                }
            }
        }

        protected static ComponentInterceptedEvents[] GetEventsInfo(GameObject gameObject)
        {
            // TODO: could this becached somewhere?
            List<Type> interfaces = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (!type.IsInterface)
                        continue;

                    if (!typeof(IEventSystemHandler).IsAssignableFrom(type))
                        continue;

                    interfaces.Add(type);
                }
            }

            List<ComponentInterceptedEvents> componentEvents = new List<ComponentInterceptedEvents>();
            List<GUIContent> events = new List<GUIContent>();
            MonoBehaviour[] mbs = gameObject.GetComponents<MonoBehaviour>();

            for (int i = 0, imax = mbs.Length; i < imax; ++i)
            {
                MonoBehaviour mb = mbs[i];
                if (mb == null)
                    continue;

                Type type = mb.GetType();


                if (typeof(IEventSystemHandler).IsAssignableFrom(type))
                {
                    foreach (var eventInterface in interfaces)
                    {
                        if (!eventInterface.IsAssignableFrom(type))
                            continue;

                        MethodInfo[] methodInfos = eventInterface.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        foreach (MethodInfo methodInfo in methodInfos)
                        {
                            events.Add(new GUIContent(methodInfo.Name));
                        }
                    }
                }

                if (events.Count > 0)
                {
                    ComponentInterceptedEvents componentEvent = new ComponentInterceptedEvents();
                    componentEvent.componentName = new GUIContent(type.Name);
                    componentEvent.interceptedEvents = events.OrderBy(e => e.text).ToArray();
                    componentEvents.Add(componentEvent);

                    events.Clear();
                }
            }

            return componentEvents.ToArray();
        }
    }
}
