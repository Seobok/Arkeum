using UnityEngine;
using UnityEngine.SceneManagement;

namespace Arkeum.Prototype
{
    public static class PrototypeBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsurePrototypeController()
        {
            if (Object.FindFirstObjectByType<PrototypeGameController>() != null)
            {
                return;
            }

            Scene activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid())
            {
                return;
            }

            GameObject root = new GameObject("ArkeumPrototype");
            root.AddComponent<PrototypeGameController>();
        }
    }
}
