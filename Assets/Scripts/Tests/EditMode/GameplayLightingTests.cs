using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace VerticalShooter.Tests
{
    public class GameplayLightingTests
    {
        [Test]
        public void GameplayHasPrefabAuthoredWarmSunAndGlobalProfile()
        {
            var lighting = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Environment/PF_GameplayLighting.prefab");
            Assert.That(lighting, Is.Not.Null);
            var sun = lighting.GetComponent<Light>();
            Assert.That(sun, Is.Not.Null);
            Assert.That(sun.type, Is.EqualTo(LightType.Directional));
            Assert.That(sun.color.r, Is.GreaterThan(sun.color.b));
            var volume = lighting.GetComponent<Volume>();
            Assert.That(volume, Is.Not.Null);
            Assert.That(volume.isGlobal, Is.True);
            Assert.That(volume.sharedProfile, Is.EqualTo(AssetDatabase.LoadAssetAtPath<VolumeProfile>("Assets/Settings/GameplayDaylightProfile.asset")));
            var dependencies = AssetDatabase.GetDependencies("Assets/Scenes/Gameplay.unity", true);
            Assert.That(dependencies, Does.Contain("Assets/Prefabs/Environment/PF_GameplayLighting.prefab"));
        }

        [TestCase("Assets/Settings/PC_RPAsset.asset")]
        [TestCase("Assets/Settings/Mobile_RPAsset.asset")]
        public void TargetRenderPipelinesSupportHdr(string path)
        {
            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(path);
            Assert.That(pipeline, Is.Not.Null);
            Assert.That(pipeline.supportsHDR, Is.True);
        }

        [Test]
        public void GameplayCameraAndProfileEnableRequestedEffects()
        {
            var rig = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Gameplay/PF_PrototypeFlightRig.prefab");
            var camera = rig.GetComponentInChildren<Camera>(true);
            var data = camera.GetComponent<UniversalAdditionalCameraData>();
            Assert.That(data, Is.Not.Null);
            Assert.That(camera.allowHDR, Is.True);
            Assert.That(data.renderPostProcessing, Is.True);
            Assert.That((data.volumeLayerMask.value & 1), Is.Not.Zero);
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>("Assets/Settings/GameplayDaylightProfile.asset");
            Assert.That(profile, Is.Not.Null);
            Assert.That(profile.TryGet<Bloom>(out var bloom), Is.True);
            Assert.That(bloom.intensity.overrideState && bloom.intensity.value > 0f, Is.True);
            Assert.That(bloom.downscale.value, Is.EqualTo(BloomDownscaleMode.Quarter));
            Assert.That(profile.TryGet<MotionBlur>(out var motion), Is.True);
            Assert.That(motion.intensity.overrideState && motion.intensity.value > 0f, Is.True);
            Assert.That(motion.mode.value, Is.EqualTo(MotionBlurMode.CameraOnly));
            Assert.That(motion.quality.value, Is.EqualTo(MotionBlurQuality.Low));
            Assert.That(motion.clamp.value, Is.LessThanOrEqualTo(.02f));
            Assert.That(profile.TryGet<ColorAdjustments>(out var colors), Is.True);
            Assert.That(colors.saturation.overrideState && colors.saturation.value > 0f, Is.True);
            Assert.That(colors.contrast.overrideState && colors.contrast.value > 0f, Is.True);
        }
    }
}
