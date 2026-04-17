using System;
using Arkeum.Production.Gameplay.Actors;

namespace Arkeum.Production.Gameplay.Run
{
    // 이번 한 판의 현재 상황을 모아둔 상태 객체
    [Serializable]
    public sealed class RunState
    {
        public int RunIndex;                    // 몇번째 런인지
        public int TurnCount;                   // 행동 횟수
        public int DepthReached;                // 도달한 최고 깊이
        public int BloodShards;                 // 소모 자원
        public int BandageCount;                // 회복 아이템 수량
        public int DraughtCount;                // 회복 아이템 수량
        public int AttackBonus;                 // 공격력 보정값
        public int GleamReward;                 // 종료 후 얻는 영구 보상
        public bool TemporaryWeaponEquipped;    // 이벤트 진행 플래그?
        public bool ReliquaryClaimed;           // 이벤트 진행 플래그?
        public bool TemporaryWeaponCollected;   // 이벤트 진행 플래그?
        public int DraughtStock;                // 상점 재고
        public RunEndReason EndReason;          // 런이 끝난 이유
        public ActorEntity Player;              // 실제 플레이어 액터

        public int EffectiveAttack => 3 + AttackBonus; // 공격력 보정값
    }
}
