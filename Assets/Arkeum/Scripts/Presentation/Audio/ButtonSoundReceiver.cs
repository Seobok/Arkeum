using UnityEngine;
using UnityEngine.UI;

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
            if(GetComponent<Button>().interactable)
                AudioManager.Instance.PlaySfx(id);
        }
    }
}
