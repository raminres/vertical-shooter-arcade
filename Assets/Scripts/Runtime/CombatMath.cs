using UnityEngine;

namespace VerticalShooter.Gameplay
{
    public static class CombatMath
    {
        public static bool TryHitTime(Vector2 projectileStart, Vector2 projectileEnd,
            Vector2 targetStart, Vector2 targetEnd, float radius, out float time)
        {
            Vector2 offset = projectileStart - targetStart;
            Vector2 movement = (projectileEnd - projectileStart) - (targetEnd - targetStart);
            float c = Vector2.Dot(offset, offset) - radius * radius;
            if (c <= 0f) { time = 0f; return true; }
            float a = Vector2.Dot(movement, movement);
            if (a <= Mathf.Epsilon) { time = 0f; return false; }
            float b = Vector2.Dot(offset, movement);
            float discriminant = b * b - a * c;
            if (discriminant < 0f) { time = 0f; return false; }
            time = (-b - Mathf.Sqrt(discriminant)) / a;
            return time >= 0f && time <= 1f;
        }
    }
}
