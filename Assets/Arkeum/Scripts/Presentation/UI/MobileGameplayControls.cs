using Arkeum.Production.Core;
using Arkeum.Production.Infrastructure.Input;
using UnityEngine;
using UnityEngine.UI;

namespace Arkeum.Production.Presentation.UI
{
    [DisallowMultipleComponent]
    public sealed class MobileGameplayControls : MonoBehaviour
    {
        private const string TimingButtonName = "Timing-Button";
        private const string UpButtonName = "Up-Button";
        private const string DownButtonName = "Down-Button";
        private const string LeftButtonName = "Left-Button";
        private const string RightButtonName = "Right-Button";

        [SerializeField] private Button timingButton;
        [SerializeField] private Button upButton;
        [SerializeField] private Button downButton;
        [SerializeField] private Button leftButton;
        [SerializeField] private Button rightButton;

        private GameDirector gameDirector;
        private InputReader inputReader;
        private bool initialized;

        public void Initialize(GameDirector director, InputReader reader)
        {
            if (initialized)
            {
                return;
            }

            gameDirector = director;
            inputReader = reader;
            FindButtons();

            Bind(timingButton, HandleTimingButton);
            Bind(upButton, MoveUp);
            Bind(downButton, MoveDown);
            Bind(leftButton, MoveLeft);
            Bind(rightButton, MoveRight);
            EnsureTimingButtonReceivesRaycasts();

            initialized = true;
            ValidateButtons();
            RefreshInteractableState();
        }

        private void Update()
        {
            if (initialized)
            {
                RefreshInteractableState();
            }
        }

        private void OnDestroy()
        {
            Unbind(timingButton, HandleTimingButton);
            Unbind(upButton, MoveUp);
            Unbind(downButton, MoveDown);
            Unbind(leftButton, MoveLeft);
            Unbind(rightButton, MoveRight);
        }

        public void MoveUp()
        {
            QueueMove(Vector2Int.up);
        }

        public void MoveDown()
        {
            QueueMove(Vector2Int.down);
        }

        public void MoveLeft()
        {
            QueueMove(Vector2Int.left);
        }

        public void MoveRight()
        {
            QueueMove(Vector2Int.right);
        }

        public void HandleTimingButton()
        {
            if (inputReader == null || gameDirector == null)
            {
                return;
            }

            if (gameDirector.CurrentState == GameState.TimingChallenge)
            {
                inputReader.QueueTimingAction();
                return;
            }

            if (gameDirector.CurrentState == GameState.InRun)
            {
                inputReader.QueueTimingToggle();
            }
        }

        private void QueueMove(Vector2Int direction)
        {
            if (inputReader == null || gameDirector == null)
            {
                return;
            }

            if (gameDirector.CurrentState == GameState.TimingChallenge)
            {
                inputReader.QueueTimingAction();
                return;
            }

            if (gameDirector.CurrentState == GameState.Hub || gameDirector.CurrentState == GameState.InRun)
            {
                inputReader.QueueMove(direction);
            }
        }

        private void FindButtons()
        {
            Button[] buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < buttons.Length; i++)
            {
                Button button = buttons[i];
                if (button == null)
                {
                    continue;
                }

                switch (button.name.Trim())
                {
                    case TimingButtonName:
                        timingButton ??= button;
                        break;
                    case UpButtonName:
                        upButton ??= button;
                        break;
                    case DownButtonName:
                        downButton ??= button;
                        break;
                    case LeftButtonName:
                        leftButton ??= button;
                        break;
                    case RightButtonName:
                        rightButton ??= button;
                        break;
                }
            }
        }

        private void EnsureTimingButtonReceivesRaycasts()
        {
            if (timingButton?.targetGraphic == null || timingButton.targetGraphic.enabled)
            {
                return;
            }

            Graphic graphic = timingButton.targetGraphic;
            Color color = graphic.color;
            color.a = 0f;
            graphic.color = color;
            graphic.raycastTarget = true;
            graphic.enabled = true;
            timingButton.transition = Selectable.Transition.None;
        }

        private void RefreshInteractableState()
        {
            if (gameDirector == null)
            {
                return;
            }

            bool canMove = gameDirector.CurrentState == GameState.Hub ||
                gameDirector.CurrentState == GameState.InRun ||
                gameDirector.CurrentState == GameState.TimingChallenge;
            bool canUseTiming = gameDirector.CurrentState == GameState.InRun || gameDirector.CurrentState == GameState.TimingChallenge;
            SetInteractable(upButton, canMove);
            SetInteractable(downButton, canMove);
            SetInteractable(leftButton, canMove);
            SetInteractable(rightButton, canMove);
            SetInteractable(timingButton, canUseTiming);
        }

        private void ValidateButtons()
        {
            if (timingButton == null || upButton == null || downButton == null || leftButton == null || rightButton == null)
            {
                Debug.LogWarning(
                    "[MobileGameplayControls] Expected Timing-Button, Up-Button, Down-Button, Left-Button, and Right-Button in GameScene.",
                    this);
            }
        }

        private static void Bind(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button != null)
            {
                button.onClick.AddListener(action);
            }
        }

        private static void Unbind(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button != null)
            {
                button.onClick.RemoveListener(action);
            }
        }

        private static void SetInteractable(Button button, bool interactable)
        {
            if (button != null && button.interactable != interactable)
            {
                button.interactable = interactable;
            }
        }
    }
}
