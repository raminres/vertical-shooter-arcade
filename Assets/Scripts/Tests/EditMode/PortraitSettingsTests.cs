using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace VerticalShooter.Tests
{
    public class PortraitSettingsTests
    {
        [Test]
        public void MobileStartsInPortrait()
        {
            Assert.That(PlayerSettings.defaultInterfaceOrientation, Is.EqualTo(UIOrientation.Portrait));
        }

        [Test]
        public void DesktopStartsInAFixedPortraitWindow()
        {
            Assert.That(PlayerSettings.fullScreenMode, Is.EqualTo(FullScreenMode.Windowed));
            Assert.That(PlayerSettings.defaultScreenWidth, Is.LessThan(PlayerSettings.defaultScreenHeight));
            Assert.That(PlayerSettings.resizableWindow, Is.False);
            Assert.That(PlayerSettings.allowFullscreenSwitch, Is.False);
        }
    }
}
