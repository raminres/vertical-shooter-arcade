using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace VerticalShooter.Tests
{
    public class AssetNamingTests
    {
        private static bool IsProjectAsset(string path) =>
            path.StartsWith("Assets/") && !path.StartsWith("Assets/Plugins/") &&
            !path.StartsWith("Assets/TutorialInfo/");

        [TestCase("Prefab", ".prefab", "PF_")]
        [TestCase("Texture2D", "", "TX_")]
        [TestCase("AudioClip", "", "AU_")]
        public void ProjectAssetsHaveExpectedPrefix(string type, string extension, string prefix)
        {
            var paths = AssetDatabase.FindAssets("t:" + type, new[] { "Assets" })
                .Select(AssetDatabase.GUIDToAssetPath).Distinct().Where(IsProjectAsset)
                .Where(path => extension.Length == 0 || path.EndsWith(extension))
                .Where(path => type != "Texture2D" || AssetImporter.GetAtPath(path) is TextureImporter)
                .Where(path => type != "AudioClip" || AssetImporter.GetAtPath(path) is AudioImporter);
            foreach (var path in paths)
                Assert.That(Path.GetFileNameWithoutExtension(path), Does.StartWith(prefix), path);
        }

        [Test]
        public void ProjectMaterialsUseTheMaterialPrefix()
        {
            var paths = AssetDatabase.FindAssets("t:Material", new[] { "Assets" })
                .Select(AssetDatabase.GUIDToAssetPath).Distinct()
                .Where(IsProjectAsset).Where(path => path.EndsWith(".mat"));
            foreach (var path in paths)
            {
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                var name = Path.GetFileNameWithoutExtension(path);
                Assert.That(name.StartsWith("MT_") || name.StartsWith("MI_"), Is.True, path);
                if (material.parent != null)
                    Assert.That(name, Does.StartWith("MI_"), path);
            }
        }

        [Test]
        public void ProjectModelsAndStandaloneMeshesUseMeshPrefixes()
        {
            foreach (var path in Directory.GetFiles("Assets", "*", SearchOption.AllDirectories).Select(p => p.Replace('\\', '/')).Where(IsProjectAsset))
            {
                if (!(AssetImporter.GetAtPath(path) is ModelImporter)) continue;
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (model == null) continue;
                string prefix = model.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length > 0 ? "SK_" : "SM_";
                Assert.That(Path.GetFileNameWithoutExtension(path), Does.StartWith(prefix), path);
            }
            var meshes = AssetDatabase.FindAssets("t:Mesh", new[] { "Assets" }).Select(AssetDatabase.GUIDToAssetPath)
                .Distinct().Where(IsProjectAsset).Where(path => path.EndsWith(".asset"))
                .Where(path => AssetDatabase.LoadMainAssetAtPath(path) is Mesh);
            foreach (var path in meshes)
            {
                var name = Path.GetFileNameWithoutExtension(path);
                Assert.That(name.StartsWith("SM_") || name.StartsWith("SK_"), Is.True, path);
            }
        }
    }
}
