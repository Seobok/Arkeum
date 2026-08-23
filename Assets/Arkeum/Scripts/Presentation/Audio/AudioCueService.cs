using Arkeum.Production.Gameplay.Run;
using Arkeum.Production.Gameplay.Timing;

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

            if ((feedback & RunActionFeedback.EnemyDamaged) != 0)
            {
                PlaySfx("EnemyHit");
            }

            if ((feedback & RunActionFeedback.EnemyDefeated) != 0)
            {
                PlaySfx("EnemyDefeated");
            }

            if ((feedback & RunActionFeedback.ShopPurchased) != 0)
            {
                PlaySfx("ShopPurchase");
            }
            else if ((feedback & RunActionFeedback.WeaponPickedUp) != 0)
            {
                PlaySfx("WeaponEquip");
            }

            if ((feedback & RunActionFeedback.WeaponDropped) != 0)
            {
                PlaySfx("WeaponUnequip");
            }

            if ((feedback & RunActionFeedback.ActionDenied) != 0)
            {
                PlaySfx("ActionDenied");
            }

            if ((feedback & RunActionFeedback.BossEncountered) != 0)
            {
                PlaySfx("BossEncounter");
                PlaySfxDelayed("BossRoomSeal", 0.18f);
            }

            if ((feedback & RunActionFeedback.BossRoomOpened) != 0)
            {
                PlaySfx("BossRoomOpen");
            }

            if ((feedback & RunActionFeedback.FloorCleared) != 0)
            {
                PlaySfx("FloorClear");
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

        public void PlayActionDenied()
        {
            PlaySfx("ActionDenied");
        }

        public void PlayFloorDescend()
        {
            PlaySfxDelayed("FloorDescend", 0.25f);
        }

        public void PlayRunResult(RunEndReason reason)
        {
            switch (reason)
            {
                case RunEndReason.Death:
                    PlaySfxDelayed("PlayerDeath", 0.2f);
                    break;
                case RunEndReason.FloorClear:
                    PlaySfxDelayed("RunClear", 0.3f);
                    break;
            }
        }

        public void PlayTimingStart()
        {
            PlaySfx("TimingStart");
        }

        public void PlayTimingResult(TimingResultGrade grade)
        {
            switch (grade)
            {
                case TimingResultGrade.Success:
                    PlaySfx("TimingSuccess");
                    break;
                case TimingResultGrade.Failed:
                    PlaySfx("TimingFailed");
                    break;
            }
        }

        private static void PlaySfx(string id)
        {
            AudioManager.Instance.PlaySfx(id);
        }

        private static void PlaySfxDelayed(string id, float delaySeconds)
        {
            AudioManager.Instance.PlaySfxDelayed(id, delaySeconds);
        }
    }
}
