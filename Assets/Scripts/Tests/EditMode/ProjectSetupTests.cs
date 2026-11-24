using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace VerticalShooter.Tests
{
    public class ProjectSetupTests
    {
        [Test]
        public void LocalizationTablesContainEnglishAndTurkish()
        {
            var locales = LocalizationEditorSettings.GetLocales().Select(locale => locale.Identifier.Code).ToArray();
            Assert.That(locales, Does.Contain("en"));
            Assert.That(locales, Does.Contain("tr"));
            var collection = LocalizationEditorSettings.GetStringTableCollections()
                .Single(table => table.TableCollectionName == "UI");
            Assert.That(collection.StringTables.Select(table => table.LocaleIdentifier.Code), Is.EquivalentTo(new[] { "en", "tr" }));
            Assert.That(collection.SharedData.Entries.Count, Is.GreaterThan(0));
            foreach (var key in collection.SharedData.Entries)
            foreach (var table in collection.StringTables)
                Assert.That(table.GetEntry(key.Id)?.Value, Is.Not.Null.And.Not.Empty,
                    $"Missing {table.LocaleIdentifier.Code} translation for {key.Key}");
        }

        [Test]
        public void EnglishIsSelectedBeforeSystemLanguage()
        {
            var selectors = LocalizationEditorSettings.ActiveLocalizationSettings.GetStartupLocaleSelectors();
            var englishIndex = selectors.FindIndex(selector => selector is SpecificLocaleSelector specific && specific.LocaleId.Code == "en");
            var systemIndex = selectors.FindIndex(selector => selector is SystemLocaleSelector);
            Assert.That(englishIndex, Is.GreaterThanOrEqualTo(0));
            Assert.That(systemIndex < 0 || englishIndex < systemIndex, Is.True);
        }

        [TestCase("iOS", BuildTarget.iOS)]
        [TestCase("Windows", BuildTarget.StandaloneWindows64)]
        public void BuildProfileUsesTheExpectedPlatformAndSharedSettings(string name, BuildTarget target)
        {
            var profile = AssetDatabase.LoadAssetAtPath<UnityEditor.Build.Profile.BuildProfile>(
                $"Assets/Settings/Build Profiles/{name}.asset");
            Assert.That(profile, Is.Not.Null);
            var serialized = new SerializedObject(profile);
            Assert.That(serialized.FindProperty("m_BuildTarget").intValue, Is.EqualTo((int)target));
            Assert.That(serialized.FindProperty("m_OverrideGlobalSceneList").boolValue, Is.False);
            Assert.That(serialized.FindProperty("m_PlayerSettingsYaml.m_Settings").arraySize, Is.Zero);
            Assert.That(EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path),
                Is.EqualTo(new[] { "Assets/Scenes/MainMenu.unity", "Assets/Scenes/Gameplay.unity" }));
        }

        [Test]
        public void UiIconImportUsesSpritePreset()
        {
            const string path = "Assets/UI/Icons/ImportPresetTest.png";
            Assert.That(File.Exists(path), Is.False);
            var texture = new Texture2D(2, 2);
            try
            {
                File.WriteAllBytes(path, texture.EncodeToPNG());
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                Assert.That(importer, Is.Not.Null);
                Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.Sprite));
                Assert.That(importer.mipmapEnabled, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(texture);
                AssetDatabase.DeleteAsset(path);
            }
        }
    }
}
