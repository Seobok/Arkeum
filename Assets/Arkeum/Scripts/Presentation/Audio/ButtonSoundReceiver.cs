using UnityEngine;

namespace Arkeum.Production.Presentation.Audio
{
    public class ButtonSoundReceiver : MonoBehaviour
    {
        public void PlayBgm(string id)
        {
            AudioManager.Instance.PlayBgm(id);
        }

        public void PlaySfx(string id)
        {
            AudioManager.Instance.PlaySfx(id);
        }
    }
}
