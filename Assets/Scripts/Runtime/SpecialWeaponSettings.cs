using UnityEngine;

namespace VerticalShooter.Gameplay
{
    public enum SpecialWeaponType { Laser, Spread, Missiles }

    public static class SpecialWeaponSelection
    {
        public static SpecialWeaponType Selected { get; set; } = SpecialWeaponType.Laser;
    }

    [CreateAssetMenu(menuName = "Vertical Shooter/Special Weapon Settings")]
    public sealed class SpecialWeaponSettings : ScriptableObject
    {
        [Min(1)] public int startingCharges = 2;
        [Min(.1f)] public float laserDuration = 1.5f;
        [Min(.01f)] public float laserTickInterval = .3f;
        [Min(1)] public int laserDamage = 1;
        [Min(.1f)] public float laserWidth = .5f;
        [Min(1f)] public float laserRange = 40f;
        [Min(1)] public int spreadCount = 7;
        [Range(0f, 90f)] public float spreadAngle = 60f;
        [Min(1)] public int missileCount = 4;
        [Min(.1f)] public float projectileSpeed = 10f;
        [Min(.1f)] public float missileTurnSpeed = 4f;
        [Min(.01f)] public float projectileRadius = .16f;
        [Min(1)] public int projectileDamage = 2;
    }
}
