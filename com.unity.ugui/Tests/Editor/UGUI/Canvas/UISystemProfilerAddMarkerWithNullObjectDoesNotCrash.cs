using UnityEngine;
using NUnit.Framework;

namespace Tests
{
    internal class UISystemProfilerAddMarkerWithNullObjectDoesNotCrash
    {
        [Test]
        public void AddMarkerShouldNotCrashWithNullObject()
        {
            UISystemProfilerApi.AddMarker("Test", null);
        }
    }
}
