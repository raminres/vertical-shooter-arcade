using NUnit.Framework;
using UnityEngine;
using VerticalShooter.Gameplay;

namespace VerticalShooter.Tests
{
    public class CombatMathTests
    {
        [Test]
        public void SweepDetectsFastProjectileCrossingAMovingTarget()
        {
            Assert.That(CombatMath.TryHitTime(new Vector2(0, -3), new Vector2(0, 3),
                new Vector2(0, 2), new Vector2(0, -2), .5f, out float time), Is.True);
            Assert.That(time, Is.InRange(0f, 1f));
        }

        [Test]
        public void SweepMissesTargetsOutsideTheCombinedRadius()
        {
            Assert.That(CombatMath.TryHitTime(new Vector2(0, -3), new Vector2(0, 3),
                new Vector2(1, 0), new Vector2(1, 0), .5f, out _), Is.False);
        }

        [Test]
        public void SweepDetectsInitialOverlapAndStationaryMiss()
        {
            Assert.That(CombatMath.TryHitTime(Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, .5f, out float time), Is.True);
            Assert.That(time, Is.Zero);
            Assert.That(CombatMath.TryHitTime(Vector2.zero, Vector2.zero, Vector2.one, Vector2.one, .5f, out _), Is.False);
        }
    }
}
