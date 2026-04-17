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

            profile.HighestDepth = profile.HighestDepth < 2 ? 2 : profile.HighestDepth;
            profile.Mq01Completed = true;
            if (!profile.CompletedQuestIds.Contains("MQ-01"))
            {
                profile.CompletedQuestIds.Add("MQ-01");
            }
        }
    }
}
