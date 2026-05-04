namespace Arkeum.Production.Gameplay.Progression
{
    public sealed class QuestService
    {
        public void MarkPrototypeClear(SaveProfile profile)
        {
            if (profile == null)
            {
                return;
            }

            profile.HighestFloor = profile.HighestFloor < 1 ? 1 : profile.HighestFloor;
            profile.Mq01Completed = true;
            if (!profile.CompletedQuestIds.Contains("MQ-01"))
            {
                profile.CompletedQuestIds.Add("MQ-01");
            }
        }
    }
}
