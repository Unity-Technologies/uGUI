using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEditor;
using NUnit.Framework;

internal class DropdownOptionsListDrawer : WrapperWindowFixture
{
    internal class Fixture : MonoBehaviour
    {
        public Dropdown.OptionDataList options = new Dropdown.OptionDataList();
    }

    [UnityTest]
    public IEnumerator PropertyDrawerDoesNotThrowExceptionWhenObjectIsDisposed()
    {
        var go = new GameObject();
        var component = go.AddComponent<Fixture>();
        var so = new SerializedObject(component);
        var win = GetWindow((wnd) => {
            Assert.DoesNotThrow(() => EditorGUILayout.PropertyField(so.FindProperty("options")));
            so.Dispose();
            so = new SerializedObject(component);
            Assert.DoesNotThrow(() => EditorGUILayout.PropertyField(so.FindProperty("options")));
            return true;
        });

        while (win.TestCompleted == false)
        {
            yield return null;
        }
    }
}
