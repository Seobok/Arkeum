using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Arkeum.Production.Infrastructure.Input
{
    // Input facade for gameplay actions.
    public sealed class InputReader
    {
        private const string PlayerMapName = "Player";
        private const string MoveActionName = "Move";
        private const string TimingToggleActionName = "Next";
        private const string ConfirmActionName = "Attack";

        private readonly InputActionMap playerActions;
        private readonly InputAction moveAction;
        private readonly InputAction timingToggleAction;
        private readonly InputAction confirmAction;
        private Vector2Int queuedMoveDirection;
        private bool hasQueuedMove;
        private bool timingToggleQueued;
        private bool timingActionQueued;

        public InputReader(InputActionAsset inputActions)
        {
            if (inputActions == null)
            {
                Debug.LogWarning("InputReader requires an InputActionAsset.");
                return;
            }

            playerActions = inputActions.FindActionMap(PlayerMapName);
            if (playerActions == null)
            {
                Debug.LogWarning($"Input action map '{PlayerMapName}' was not found.");
                return;
            }

            moveAction = playerActions.FindAction(MoveActionName);
            timingToggleAction = playerActions.FindAction(TimingToggleActionName);
            confirmAction = playerActions.FindAction(ConfirmActionName);
            playerActions.Enable();
        }

        public bool TryGetMoveDirection(out Vector2Int direction)
        {
            if (hasQueuedMove)
            {
                direction = queuedMoveDirection;
                queuedMoveDirection = Vector2Int.zero;
                hasQueuedMove = false;
                return direction != Vector2Int.zero;
            }

            direction = Vector2Int.zero;
            if (moveAction == null || !moveAction.WasPressedThisFrame())
            {
                return false;
            }

            Vector2 move = moveAction.ReadValue<Vector2>();
            if (move == Vector2.zero)
            {
                return false;
            }

            if (Mathf.Abs(move.y) >= Mathf.Abs(move.x))
            {
                direction = move.y > 0f ? Vector2Int.up : Vector2Int.down;
                return true;
            }

            direction = move.x > 0f ? Vector2Int.right : Vector2Int.left;
            return true;
        }

        public bool WasTimingTogglePressed()
        {
            if (timingToggleQueued)
            {
                timingToggleQueued = false;
                return true;
            }

            return timingToggleAction != null && timingToggleAction.WasPressedThisFrame();
        }

        public bool WasTimingActionPressed()
        {
            if (timingActionQueued)
            {
                timingActionQueued = false;
                return true;
            }

            bool confirmPressed = WasConfirmPressed();
            if (confirmPressed && WasMousePressedOverUi())
            {
                confirmPressed = false;
            }

            return confirmPressed ||
                (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame);
        }

        private static bool WasMousePressedOverUi()
        {
            return Mouse.current != null &&
                Mouse.current.leftButton.wasPressedThisFrame &&
                EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject();
        }

        public void QueueMove(Vector2Int direction)
        {
            if (direction == Vector2Int.zero)
            {
                return;
            }

            queuedMoveDirection = direction;
            hasQueuedMove = true;
        }

        public void QueueTimingToggle()
        {
            timingToggleQueued = true;
        }

        public void QueueTimingAction()
        {
            timingActionQueued = true;
        }

        public bool WasConfirmPressed()
        {
            return confirmAction != null && confirmAction.WasPressedThisFrame();
        }
    }
}
