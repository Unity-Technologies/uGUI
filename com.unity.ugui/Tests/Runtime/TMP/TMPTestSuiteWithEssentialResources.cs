using NUnit.Framework;
using UnityEngine.TestTools;

namespace TMPro
{
    /// <summary>
    /// Base fixture for TMP tests that need the TMP Essential Resources.
    /// </summary>
    [PrebuildSetup(typeof(TMPEssentialResourcesImporter))]
    internal abstract class TMPTestSuiteWithEssentialResources
    {
        /// <summary>
        /// Checks that the resources arrived before any test in the fixture runs. Override to add
        /// fixture-specific setup, ensuring to call base.OneTimeSetup().
        /// </summary>
        [OneTimeSetUp]
        public virtual void OneTimeSetup()
        {
            Assert.IsNotNull(TMP_Settings.instance,
                $"TMP_Settings could not be loaded, so {nameof(TMPEssentialResourcesImporter)} did not make the TMP Essential Resources available to this fixture.");
            Assert.IsNotNull(TMP_Settings.defaultFontAsset,
                $"TMP_Settings has no default font asset, so {nameof(TMPEssentialResourcesImporter)} did not make the TMP Essential Resources available to this fixture.");
        }
    }
}
