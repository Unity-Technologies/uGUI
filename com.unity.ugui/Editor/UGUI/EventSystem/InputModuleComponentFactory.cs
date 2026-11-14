using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityEditor.EventSystems
{
    /// <summary>
    /// Provides a way to add an input module to an <see cref="EventSystem"/> GameObject in the Editor.
    /// </summary>
    /// <remarks>
    /// The Editor uses this factory when it creates an <see cref="EventSystem"/>, or when you select **Add Default Input Modules** in the <see cref="EventSystem"/> Inspector.
    /// By default, the factory adds a <see cref="StandaloneInputModule"/> component.
    /// To add a different input module, such as one from another input package, register a delegate with <see cref="SetInputModuleComponentOverride"/>.
    /// </remarks>
    public static class InputModuleComponentFactory
    {
        /// <summary>
        /// Represents a method that adds an input module component to a GameObject.
        /// </summary>
        /// <param name="gameObject">GameObject to add the input module component to.</param>
        /// <returns>The input module component that the method adds.</returns>
        public delegate BaseInputModule AddInputModuleComponentDelegate(GameObject gameObject);

        /// <summary>
        /// Sets the method that adds an input module component to a GameObject.
        /// </summary>
        /// <remarks>
        /// After you set an override, <see cref="AddInputModule"/> calls the override and doesn't add a <see cref="StandaloneInputModule"/>.
        /// Pass <c>null</c> to restore the default behavior.
        /// </remarks>
        /// <param name="addInputModuleComponentOverride">Method that adds an input module component, or <c>null</c> to use the default behavior.</param>
        public static void SetInputModuleComponentOverride(AddInputModuleComponentDelegate addInputModuleComponentOverride)
        {
            m_AddInputModuleComponentOverride = addInputModuleComponentOverride;
        }

        private static AddInputModuleComponentDelegate m_AddInputModuleComponentOverride = null;

        /// <summary>
        /// The method that adds an input module component in place of the default <see cref="StandaloneInputModule"/>.
        /// </summary>
        /// <remarks>
        /// When the value is <c>null</c>, <see cref="AddInputModule"/> adds a <see cref="StandaloneInputModule"/>.
        /// To set this value, use <see cref="SetInputModuleComponentOverride"/>.
        /// </remarks>
        public static AddInputModuleComponentDelegate InputModuleComponentOverride { get { return m_AddInputModuleComponentOverride; } }

        /// <summary>
        /// Adds an input module component to a GameObject.
        /// </summary>
        /// <remarks>
        /// If an override is set with <see cref="SetInputModuleComponentOverride"/>, this method calls the override.
        /// Otherwise, it adds a <see cref="StandaloneInputModule"/> component.
        /// </remarks>
        /// <param name="gameObject">GameObject to add the input module component to.</param>
        /// <returns>The input module component that was added.</returns>
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
