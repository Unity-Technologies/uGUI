using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Utils;

internal class SceneWithNestedLayoutElementsLoadScript : MonoBehaviour
{
    public bool isStartCalled { get; private set; }

    protected void Start()
    {
        isStartCalled = true;
    }
}
