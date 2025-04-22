using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityEditor.EventSystems
{
    public static class InputModuleComponentFactory
    {
        public delegate BaseInputModule AddInputModuleComponentDelegate(GameObject gameObject);

        public static void SetInputModuleComponentOverride(AddInputModuleComponentDelegate addInputModuleComponentOverride)
        {
            m_AddInputModuleComponentOverride = addInputModuleComponentOverride;
        }

        private static AddInputModuleComponentDelegate m_AddInputModuleComponentOverride = null;

        public static BaseInputModule AddInputModule(GameObject gameObject)
        {
            return m_AddInputModuleComponentOverride != null ?
                m_AddInputModuleComponentOverride(gameObject) :
                AddStandaloneInputModuleComponent(gameObject);
        }

        private static BaseInputModule AddStandaloneInputModuleComponent(GameObject gameObject)
        {
            return ObjectFactory.AddComponent<StandaloneInputModule>(gameObject);
        }
    }
}
