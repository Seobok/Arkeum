using Arkeum.Production.Gameplay.Timing;
using UnityEngine;

namespace Arkeum.Production.Presentation.UI
{
    public abstract class TimingChallengePresenterBase : MonoBehaviour
    {
        protected TimingSession Session { get; private set; }

        // 타이밍 팝업 호출
        public void Show(TimingSession session)
        {
            Session = session;
            gameObject.SetActive(session != null);
            if (session != null)
            {
                OnShow(session);
                Refresh(session);
            }
        }

        // 타이밍 팝업 호출 중 프레임 단위로 호출 (LateUpdate)
        public void Refresh(TimingSession session)
        {
            if (session == null)
            {
                return;
            }

            // Concrete presenters own the view details for their timing rule.
            OnRefresh(session);
        }

        public void Hide()
        {
            Session = null;
            OnHide();
            gameObject.SetActive(false);
        }

        protected virtual void OnShow(TimingSession session)
        {
        }

        protected abstract void OnRefresh(TimingSession session);

        protected virtual void OnHide()
        {
        }
    }
}
