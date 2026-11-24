using NUnit.Framework;
using UnityEngine;
using VerticalShooter.UI;

namespace VerticalShooter.Tests
{
    public class SafeAreaTests
    {
        [Test]
        public void ConvertsDevicePixelsToPanelInsets()
        {
            var insets = SafeAreaInsets.Calculate(
                new Rect(0, 102, 1179, 2277),
                new Vector2(1179, 2556),
                new Vector2(540, 1170.687f));

            Assert.That(insets.x, Is.EqualTo(0).Within(0.01f));
            Assert.That(insets.y, Is.EqualTo(81.07f).Within(0.1f));
            Assert.That(insets.z, Is.EqualTo(0).Within(0.01f));
            Assert.That(insets.w, Is.EqualTo(46.72f).Within(0.1f));
        }

        [Test]
        public void HandlesHorizontalInsetsAndScaledPanels()
        {
            var insets = SafeAreaInsets.Calculate(
                new Rect(20, 40, 960, 1820),
                new Vector2(1000, 1900),
                new Vector2(500, 950));
            Assert.That(insets, Is.EqualTo(new Vector4(10, 20, 10, 20)));
        }
    }
}
