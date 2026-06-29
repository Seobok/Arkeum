using Arkeum.Production.Gameplay.Run;

namespace Arkeum.Production.Presentation.Audio
{
    public sealed class AudioCueService
    {
        public void PlayHubBgm()
        {
            AudioManager.Instance.PlayBgm("Hub");
        }

        public void PlayRunBgm()
        {
            AudioManager.Instance.PlayBgm("Run");
        }

        public void PlayRunActionFeedback(RunActionFeedback feedback)
        {
            if ((feedback & RunActionFeedback.PlayerMoved) != 0)
            {
                PlaySfx("PlayerMove");
            }

            if ((feedback & RunActionFeedback.PlayerAttacked) != 0)
            {
                PlaySfx("PlayerAttack");
            }

            if ((feedback & RunActionFeedback.PlayerTeleported) != 0)
            {
                PlaySfx("Teleport");
            }
        }

        public void PlayPlayerMove()
        {
            PlaySfx("PlayerMove");
        }

        public void PlayPlayerHit()
        {
            PlaySfx("PlayerHit");
        }

        private static void PlaySfx(string id)
        {
            AudioManager.Instance.PlaySfx(id);
        }
    }
}
