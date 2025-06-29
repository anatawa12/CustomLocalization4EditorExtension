using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace CustomLocalization4EditorExtension.Tests
{
    public class LocalizationTest
    {
        // edbcfdff07c34ee99997772283a113bf: Resources/General
        private const string GeneralResourceDirGuid = "edbcfdff07c34ee99997772283a113bf";

        [SetUp]
        public void CleanupLocaleSettings()
        {
            // Cleanup EditorPrefs and ensure no leftover settings from previous tests
            EditorPrefs.DeleteKey($"com.anatawa12.custom-localization-for-editor-extension.locale.{GeneralResourceDirGuid}");
            EditorPrefs.DeleteKey("com.anatawa12.custom-localization-for-editor-extension.locale.71cb5e55a5c54e23bda12c109bf83952");
            EditorPrefs.DeleteKey($"com.anatawa12.custom-localization-for-editor-extension.locale.{GetTestsDirPath()}/Resources/General/");
            EditorPrefs.DeleteKey($"com.anatawa12.custom-localization-for-editor-extension.locale.{GetTestsDirPath()}/Resources/General/en.po");
        }

        private void AccessTest(string path)
        {
            var locale = new Localization(path, "en");
            locale.Setup();

            Assert.AreEqual(locale.TryTr("load-successful-identifier"), "load successful");
            Assert.AreEqual(locale.CurrentLocaleCode, "en");

            locale.CurrentLocaleCode = "ja";

            Assert.AreEqual(locale.TryTr("load-successful-identifier"), "ロード成功");
            Assert.AreEqual(locale.CurrentLocaleCode, "ja");

            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void DirectoryUuidBasedAccess()
        {
            AccessTest(GeneralResourceDirGuid);
        }

        [Test]
        public void FileUuidBasedAccess()
        {
            // 71cb5e55a5c54e23bda12c109bf83952: Resources/General/en.po
            AccessTest("71cb5e55a5c54e23bda12c109bf83952");
        }

        [Test]
        public void DirectoryPathBasedAccess()
        {
            AccessTest($"{GetTestsDirPath()}/Resources/General/");
        }

        [Test]
        public void FilePathBasedAccess()
        {
            AccessTest($"{GetTestsDirPath()}/Resources/General/en.po");
        }

        [Test]
        public void DefaultLocaleNotFound()
        {
            var locale = new Localization(GeneralResourceDirGuid, "tr");
            LogAssert.NoUnexpectedReceived();
            LogAssert.Expect(LogType.Error, "locale asset for default locale(tr) not found.");
            locale.Setup();
            
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void DefaultLocaleOnlyText()
        {
            var locale = new Localization(GeneralResourceDirGuid, "en");
            Assert.AreEqual(locale.TryTr("default-locale-only"), "default locale only");
            locale.CurrentLocaleCode = "ja";
            Assert.AreEqual(locale.TryTr("default-locale-only"), "default locale only");
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void ThirdLocaleOnlyText()
        {
            var locale = new Localization(GeneralResourceDirGuid, "en");
            Assert.AreEqual(locale.TryTr("ja-only"), null);
            locale.CurrentLocaleCode = "ja";
            Assert.AreEqual(locale.TryTr("ja-only"), "日本語のみ");
            LogAssert.NoUnexpectedReceived();
        }

        private string GetTestsDirPath()
        {
            return AssetDatabase.GUIDToAssetPath("349e1bb894f54bb38180b04c54195c3f");
        }
    }
}
