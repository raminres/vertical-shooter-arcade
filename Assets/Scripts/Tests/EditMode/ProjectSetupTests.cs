using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;

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
