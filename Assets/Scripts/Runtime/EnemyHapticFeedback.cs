using System;
using RichHaptic.Data;

namespace VerticalShooter.Gameplay
{
    public sealed class EnemyHapticFeedback
    {
        private readonly Action<PresetType> play;

        public EnemyHapticFeedback(Action<PresetType> play) => this.play = play;

        public void OnEnemyHit(bool defeated) => play(defeated ? PresetType.MediumImpact : PresetType.LightImpact);
    }
}
