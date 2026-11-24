using System.Collections.Generic;
using NUnit.Framework;
using RichHaptic.Data;
using VerticalShooter.Gameplay;

namespace VerticalShooter.Tests
{
    public class EnemyHapticFeedbackTests
    {
        [Test]
        public void HitAndKillUseDistinctShortImpactPresets()
        {
            var pulses = new List<PresetType>();
            var feedback = new EnemyHapticFeedback(pulses.Add);
            feedback.OnEnemyHit(false);
            feedback.OnEnemyHit(true);
            Assert.That(pulses, Is.EqualTo(new[] { PresetType.LightImpact, PresetType.MediumImpact }));
        }
    }
}
