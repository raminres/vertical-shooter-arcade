using DG.Tweening;
using NUnit.Framework;
using RichHaptic;
using RichHaptic.Data;

namespace VerticalShooter.Tests
{
    public class TweenAndHapticPackageTests
    {
        [Test]
        public void DotweenEasingCanBeUsedFromAnAssemblyDefinition()
        {
            float value = DOVirtual.EasedValue(0f, 10f, 0.5f, Ease.Linear);
            Assert.That(value, Is.EqualTo(5f).Within(0.001f));
        }

        [Test]
        public void HapticPresetIsSafeInTheEditor()
        {
            Assert.DoesNotThrow(() => Haptics.PlayPreset(PresetType.LightImpact));
        }
    }
}
