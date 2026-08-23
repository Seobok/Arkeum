using Arkeum.Production.Core;
using Arkeum.Production.Gameplay.Progression;
using Arkeum.Production.Gameplay.Run;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Arkeum.Production.Presentation.UI
{
    [DisallowMultipleComponent]
    public sealed class ResultScreenPresenter : MonoBehaviour
    {
        [SerializeField] private GameObject resultCanvas;
        [SerializeField] private Transform titleRoot;
        [SerializeField] private Transform turnCountRoot;
        [SerializeField] private Transform highFloorRoot;
        [SerializeField] private Transform totalScoreRoot;
        [SerializeField] private Transform continueButtonRoot;

        private TMP_Text titleText;
        private TMP_Text turnCountText;
        private TMP_Text highFloorText;
        private TMP_Text totalScoreText;
        private Button continueButton;
        private GameDirector gameDirector;

        public void Initialize(GameDirector director)
        {
            gameDirector = director;
            ResolveComponents();

            if (continueButton != null)
            {
                continueButton.onClick.RemoveListener(ReturnToHub);
                continueButton.onClick.AddListener(ReturnToHub);
            }

            Hide();
        }

        public void Show(RunState runState, SaveProfile profile)
        {
            if (runState == null)
            {
                return;
            }

            ResolveComponents();

            if (titleText != null)
            {
                titleText.text = runState.EndReason == RunEndReason.Death ? "DEFEAT" : "CLEAR";
            }

            if (turnCountText != null)
            {
                turnCountText.text = $"Turn Count: {runState.PlayerTurnCount}";
            }

            if (highFloorText != null)
            {
                highFloorText.text = $"Current Floor: {runState.CurrentFloor}";
            }

            if (totalScoreText != null)
            {
                int totalGold = profile != null ? profile.Gold : 0;
                totalScoreText.text = $"Total Gold: {totalGold}";
            }

            if (resultCanvas != null)
            {
                resultCanvas.SetActive(true);
            }
        }

        public void Hide()
        {
            if (resultCanvas != null)
            {
                resultCanvas.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (continueButton != null)
            {
                continueButton.onClick.RemoveListener(ReturnToHub);
            }
        }

        private void ReturnToHub()
        {
            gameDirector?.EnterHub("The echo of return fades, and you stand before the altar again.");
        }

        private void ResolveComponents()
        {
            titleText = ResolveComponent(titleRoot, titleText);
            turnCountText = ResolveComponent(turnCountRoot, turnCountText);
            highFloorText = ResolveComponent(highFloorRoot, highFloorText);
            totalScoreText = ResolveComponent(totalScoreRoot, totalScoreText);
            continueButton = ResolveComponent(continueButtonRoot, continueButton);
        }

        private static T ResolveComponent<T>(Transform root, T current) where T : Component
        {
            if (current != null || root == null)
            {
                return current;
            }

            return root.GetComponent<T>() ?? root.GetComponentInChildren<T>(true);
        }
    }
}
