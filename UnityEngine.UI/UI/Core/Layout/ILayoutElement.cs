using UnityEngine;
using System.Collections;

namespace UnityEngine.UI
{
    public interface ILayoutElement
    {
        // After this method is invoked, layout horizontal input properties should return up-to-date values.
        // Children will already have up-to-date layout horizontal inputs when this methods is called.
        void CalculateLayoutInputHorizontal();
        // After this method is invoked, layout vertical input properties should return up-to-date values.
        // Children will already have up-to-date layout vertical inputs when this methods is called.
        void CalculateLayoutInputVertical();

        // Layout horizontal inputs
        float minWidth { get; }
        float preferredWidth { get; }
        float flexibleWidth { get; }
        // Layout vertical inputs
        float minHeight { get; }
        float preferredHeight { get; }
        float flexibleHeight { get; }

        int layoutPriority { get; }
    }

    public interface ILayoutController
    {
        void SetLayoutHorizontal();
        void SetLayoutVertical();
    }

    // An ILayoutGroup component should drive the RectTransforms of its children.
    public interface ILayoutGroup : ILayoutController
    {
    }

    // An ILayoutSelfController component should drive its own RectTransform.
    public interface ILayoutSelfController : ILayoutController
    {
    }

    // An ILayoutIgnorer component is ignored by the auto-layout system.
    public interface ILayoutIgnorer
    {
        bool ignoreLayout { get; }
    }
}
