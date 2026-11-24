using System;
using System.IO;
using NUnit.Framework;
using VerticalShooter.Editor;

namespace VerticalShooter.Tests
{
    public class IosBuildNumbersTests
    {
        [Test]
        public void IncrementsPatchAndBuildNumber()
        {
            Assert.That(IosBuildNumbers.NextVersion("0.1.9"), Is.EqualTo("0.1.10"));
            Assert.That(IosBuildNumbers.NextBuildNumber("0"), Is.EqualTo("1"));
        }

        [TestCase("1.2")]
        [TestCase("1.2.beta")]
        [TestCase("1.2.2147483647")]
        public void RejectsInvalidVersions(string version)
        {
            Assert.Throws<FormatException>(() => IosBuildNumbers.NextVersion(version));
        }

        [TestCase("")]
        [TestCase("-1")]
        [TestCase("1.0")]
        public void RejectsInvalidBuildNumbers(string buildNumber)
        {
            Assert.Throws<FormatException>(() => IosBuildNumbers.NextBuildNumber(buildNumber));
        }

        [Test]
        public void ReservesAvailableFoldersWithoutOverwriting()
        {
            string root = Path.Combine(Path.GetTempPath(), "vertical-shooter-test-" + Guid.NewGuid());
            try
            {
                Assert.That(Path.GetFileName(IosBuildNumbers.ReserveFolder(root)), Is.EqualTo("_build0"));
                Assert.That(Path.GetFileName(IosBuildNumbers.ReserveFolder(root)), Is.EqualTo("_build1"));
                Assert.That(Directory.Exists(Path.Combine(root, "_build0")), Is.True);
                Directory.Move(Path.Combine(root, "_build1"), Path.Combine(root, "_build5"));
                Assert.That(Path.GetFileName(IosBuildNumbers.ReserveFolder(root)), Is.EqualTo("_build6"));
            }
            finally
            {
                if (Directory.Exists(root))
                    Directory.Delete(root, true);
            }
        }
    }
}
