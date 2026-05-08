namespace Arkeum.Production.Gameplay.Timing
{
    public readonly struct TimingAttackResult
    {
        public TimingAttackResult(bool attempted, TimingResultGrade grade, float damageMultiplier, int flatDamageBonus)
        {
            Attempted = attempted;
            Grade = grade;
            DamageMultiplier = damageMultiplier;
            FlatDamageBonus = flatDamageBonus;
        }

        public bool Attempted { get; }
        public TimingResultGrade Grade { get; }
        public float DamageMultiplier { get; }
        public int FlatDamageBonus { get; }

        public static TimingAttackResult None => new TimingAttackResult(false, TimingResultGrade.None, 1f, 0);
    }
}
