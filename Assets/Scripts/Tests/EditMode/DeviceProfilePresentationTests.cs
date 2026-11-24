using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace VerticalShooter.Tests
{
    public class DeviceProfilePresentationTests
    {
        [Serializable] private class Profile { public ScreenDefinition[] screens; }
        [Serializable] private class ScreenDefinition { public int width; public int height; public Presentation presentation; public Orientation[] orientations; }
        [Serializable] private class Presentation { public string overlayPath; public float cornerRadius; }
        [Serializable] private class Orientation { public Rect safeArea; }

        [TestCase("iPhone 15.device", 1179, 2556, 165f, 102f, 2277f)]
        [TestCase("iPad mini 6.device", 1488, 2266, 43f, 40f, 2178f)]
        public void DeviceOverlayHasRoundedCornersAndPreservesSafeArea(string file, int width, int height, float radius, float safeY, float safeHeight)
        {
            var folder = "Assets/Settings/DeviceSimulator";
            var profile = JsonUtility.FromJson<Profile>(File.ReadAllText(Path.Combine(folder, file)));
            var screen = profile.screens[0];
            Assert.That(screen.width, Is.EqualTo(width));
            Assert.That(screen.height, Is.EqualTo(height));
            Assert.That(screen.presentation.cornerRadius, Is.EqualTo(radius));
            Assert.That(screen.orientations[0].safeArea.y, Is.EqualTo(safeY));
            Assert.That(screen.orientations[0].safeArea.height, Is.EqualTo(safeHeight));
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                Assert.That(ImageConversion.LoadImage(texture, File.ReadAllBytes(Path.Combine(folder, screen.presentation.overlayPath))), Is.True);
                Assert.That(texture.width, Is.EqualTo(width));
                Assert.That(texture.height, Is.EqualTo(height));
                Assert.That(texture.GetPixel(0, 0).a, Is.GreaterThan(.99f));
                Assert.That(texture.GetPixel(width - 1, height - 1).a, Is.GreaterThan(.99f));
                Assert.That(texture.GetPixel(width / 2, height / 2).a, Is.Zero);
            }
            finally { UnityEngine.Object.DestroyImmediate(texture); }
        }
    }
}
