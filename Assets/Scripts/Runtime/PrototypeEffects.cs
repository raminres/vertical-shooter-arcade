using UnityEngine;

namespace VerticalShooter.Gameplay
{
    public sealed class PrototypeEffects : MonoBehaviour
    {
        [SerializeField] private ParticleSystem[] enemyExplosions;
        [SerializeField] private ParticleSystem[] playerHits;

        public bool PlayEnemyExplosion(Vector3 position) => PlayAvailable(enemyExplosions, position);
        public bool PlayPlayerHit(Vector3 position) => PlayAvailable(playerHits, position);

        private static bool PlayAvailable(ParticleSystem[] pool, Vector3 position)
        {
            foreach (var effect in pool)
            {
                if (effect.isPlaying || effect.IsAlive(true)) continue;
                effect.transform.position = position;
                effect.Play(true);
                return true;
            }
            // A busy pool skips the effect; gameplay never waits for visuals.
            return false;
        }
    }
}
