using System.Collections;
using NUnit.Framework;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace VerticalShooter.Tests
{
    public class StartupSceneTests
    {
        [UnityTest]
        public IEnumerator FirstBuildSceneLoads()
        {
            Assert.That(SceneManager.sceneCountInBuildSettings, Is.GreaterThan(0));

            yield return SceneManager.LoadSceneAsync(0, LoadSceneMode.Single);
            yield return null;

            Assert.That(SceneManager.GetActiveScene().buildIndex, Is.Zero);
        }
    }
}
