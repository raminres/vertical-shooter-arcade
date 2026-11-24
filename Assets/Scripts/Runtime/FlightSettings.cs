using UnityEngine;

namespace VerticalShooter.Gameplay
{
    [CreateAssetMenu(menuName = "Vertical Shooter/Prototype Flight Settings")]
    public sealed class FlightSettings : ScriptableObject
    {
        [Min(0f)] public float scrollSpeed = 3f;
        [Min(0f)] public float playerSpeed = 6f;
        [Min(0f)] public float touchSensitivity = 1f;
        [Min(0f)] public float bankAngle = 18f;
        [Min(0.01f)] public float touchBankFullSpeed = .5f;
        [Min(0.01f)] public float bankTweenDuration = .15f;
        [Min(0f)] public float cameraMovementZoom = 1.5f;
        [Min(0.01f)] public float cameraTweenDuration = .25f;
        [Min(0.01f)] public float enemyHitTweenDuration = .12f;
        [Min(0f)] public float playerHalfWidth = 1f;
        [Min(0f)] public float playerHalfLength = 1f;
        [Min(1)] public int lightFighterHitPoints = 2;
        [Min(1)] public int projectileDamage = 1;
        [Min(0.01f)] public float fireInterval = .3f;
        [Min(0.1f)] public float projectileSpeed = 14f;
        [Min(0.01f)] public float projectileRadius = .12f;
        [Min(0.01f)] public float enemyHitRadius = .75f;
        [Min(0f)] public float enemySpeed = 4f;
        [Min(0.1f)] public float enemySpawnInterval = 0.9f;
        [Min(0.1f)] public float groupInterval = 1.5f;
    }
}
