using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

internal class VersionTests
{
    static string GetHelpURLVersion<T>(string attributeName)
    {
        var customAttributes = typeof(T).GetCustomAttributes(false);
        Assert.NotNull(customAttributes);
        var urlAttribute = customAttributes.FirstOrDefault(attribute => attribute.GetType().Name == attributeName);
        Assert.NotNull(urlAttribute);
        var propertyInfo = GetProperty(urlAttribute.GetType(), "version", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(propertyInfo);
        var version = propertyInfo.GetValue(null) as string;
        Assert.NotNull(version);
        return version;

        PropertyInfo GetProperty (Type type, string propertyName, BindingFlags bindingFlags)
        {
            var prop = type.GetProperty(propertyName, bindingFlags);
            if (prop != null)
                return prop;
            else if (type.BaseType != null)
                return GetProperty(type.BaseType, propertyName, bindingFlags);
            return null;
        }
    }

    struct Version
    {
        public int major;
        public int minor;
        public int patch;

        public Version(string version)
        {
            major = minor = patch = 0;
            var split = version.Split('.');
            if (split.Length > 0) int.TryParse(split[0], out major);
            if (split.Length > 1) int.TryParse(split[1], out minor);
            if (split.Length > 2) int.TryParse(split[2], out patch);
        }

        public override string ToString()
        {
            return $"{major}.{minor}.{patch}";
        }

        public override bool Equals(object obj)
        {
            if (obj is Version other)
                return major == other.major && minor == other.minor;
            return false;
        }
    }

    [Test]
    public void CheckVersionParity()
    {
        var baseVersion = GetHelpURLVersion<RectTransform>("UIModuleHelpURL");
        Assert.NotNull(baseVersion);
        var uguiVersion = GetHelpURLVersion<Button>("UGUIHelpURL");
        Assert.NotNull(uguiVersion);
        var tmpVersion = GetHelpURLVersion<TextMeshProUGUI>("TMPHelpURL");
        Assert.NotNull(tmpVersion);
        var baseVersionNumber = new Version(baseVersion);
        var uguiVersionNumber = new Version(uguiVersion);
        var tmpVersionNumber = new Version(tmpVersion);
        Assert.That(baseVersionNumber, Is.EqualTo(uguiVersionNumber).And.EqualTo(tmpVersionNumber));

        var uguiPackageInfo = PackageInfo.FindForPackageName("com.unity.ugui");
        Assert.NotNull(uguiPackageInfo);
        var packageVersionNumber = new Version(uguiPackageInfo.version);
        Assert.AreEqual(baseVersionNumber, packageVersionNumber);
    }
}

