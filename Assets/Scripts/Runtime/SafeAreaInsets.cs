using UnityEngine;

namespace VerticalShooter.UI
{
    public static class SafeAreaInsets
    {
        public static Vector4 Calculate(Rect safeArea, Vector2 screenSize, Vector2 panelSize)
        {
            float scaleX = panelSize.x / screenSize.x;
            float scaleY = panelSize.y / screenSize.y;
            return new Vector4(
                safeArea.xMin * scaleX,
                (screenSize.y - safeArea.yMax) * scaleY,
                (screenSize.x - safeArea.xMax) * scaleX,
                safeArea.yMin * scaleY);
        }
    }
}
