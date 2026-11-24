using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace VerticalShooter.Tests
{
    public class SpecialWeaponMaterialTests
    {
        [TestCase("Assets/Prefabs/Projectiles/PF_SpecialLaser.prefab", "Assets/Art/Materials/MT_SpecialWeapon.mat", true)]
        [TestCase("Assets/Prefabs/Projectiles/PF_SpecialProjectile.prefab", "Assets/Art/Materials/MT_SpecialProjectile.mat", false)]
        public void SpecialPrefabUsesSupportedUrpMaterialWithDistinctNonErrorColor(string prefabPath, string materialPath, bool laser)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Assert.That(prefab, Is.Not.Null);
            var renderer = prefab.GetComponent<Renderer>();
            Assert.That(renderer, Is.Not.Null);
            var material = renderer.sharedMaterial;
            Assert.That(material, Is.Not.Null);
            Assert.That(AssetDatabase.GetAssetPath(material), Is.EqualTo(materialPath));
            Assert.That(material.shader, Is.Not.Null);
            Assert.That(material.shader.name, Is.EqualTo("Universal Render Pipeline/Unlit"));
            Assert.That(material.shader.isSupported, Is.True);
            var color = material.GetColor("_BaseColor");
            Assert.That(color.g, Is.GreaterThan(color.r));
            Assert.That(laser ? color.b > color.r : color.g > color.b, Is.True);
        }
    }
}
