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

            CreatePresenter(presenterPrefab);
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
            DestroyActivePresenter();

            // The host only chooses the prefab; each prefab owns its own layout and widgets.
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
