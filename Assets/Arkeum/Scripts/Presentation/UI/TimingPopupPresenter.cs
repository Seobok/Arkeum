using Arkeum.Production.Gameplay.Timing;
using UnityEngine;

namespace Arkeum.Production.Presentation.UI
{
    public sealed class TimingPopupPresenter : MonoBehaviour
    {
        [SerializeField] private Transform presenterRoot;

        private TimingSession session;
        private TimingChallengePresenterBase activePresenter;
        private bool missingPresenterLogged;

        public bool IsVisible => session != null;

        public void Initialize()
        {
            Hide();
        }

        // 타이밍 공격 호출
        public void Show(TimingSession timingSession)
        {
            session = timingSession;
            if (session == null)
            {
                Hide();
                return;
            }

            TimingChallengePresenterBase presenterPrefab = session.Definition != null
                ? session.Definition.PresenterPrefab
                : null;

            if (presenterPrefab == null)
            {
                if (!missingPresenterLogged)
                {
                    missingPresenterLogged = true;
                    Debug.LogWarning("[TimingPopupPresenter] Timing challenge has no presenter prefab assigned.", this);
                }

                return;
            }

            // 타이밍 팝업 생성
            CreatePresenter(presenterPrefab);

            // 타이밍 팝업 호출
            activePresenter.Show(session);
        }

        public void Hide()
        {
            session = null;
            DestroyActivePresenter();
        }

        private void LateUpdate()
        {
            Refresh();
        }

        private void Refresh()
        {
            if (session == null || activePresenter == null)
            {
                return;
            }

            activePresenter.Refresh(session);
        }

        private void CreatePresenter(TimingChallengePresenterBase presenterPrefab)
        {
            // 기존 삭제되지 않은 타이밍 팝업이 있다면 제거
            DestroyActivePresenter();

            // 타이밍 팝업 생성
            Transform root = presenterRoot != null ? presenterRoot : transform;
            activePresenter = Instantiate(presenterPrefab, root);
        }

        private void DestroyActivePresenter()
        {
            if (activePresenter == null)
            {
                return;
            }

            Destroy(activePresenter.gameObject);
            activePresenter = null;
        }
    }
}
