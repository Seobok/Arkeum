using UnityEngine;
using UnityEngine.InputSystem;

namespace Arkeum.Production.Infrastructure.Input
{
    // Input facade for gameplay actions.
    public sealed class InputReader
    {
        private const string PlayerMapName = "Player";
        private const string MoveActionName = "Move";
        private const string WaitActionName = "Wait";
        private const string UseBandageActionName = "Previous";
        private const string TimingToggleActionName = "Next";
        private const string ConfirmActionName = "Attack";

        private readonly InputActionMap playerActions;
        private readonly InputAction moveAction;
        private readonly InputAction waitAction;
        private readonly InputAction useBandageAction;
        private readonly InputAction timingToggleAction;
        private readonly InputAction confirmAction;

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
            waitAction = playerActions.FindAction(WaitActionName);
            useBandageAction = playerActions.FindAction(UseBandageActionName);
            timingToggleAction = playerActions.FindAction(TimingToggleActionName);
            confirmAction = playerActions.FindAction(ConfirmActionName);
            playerActions.Enable();
        }

        public bool TryGetMoveDirection(out Vector2Int direction)
        {
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

        public bool WasWaitPressed()
        {
            return waitAction != null && waitAction.WasPressedThisFrame();
        }

        public bool WasUseBandagePressed()
        {
            return useBandageAction != null && useBandageAction.WasPressedThisFrame();
        }

        public bool WasTimingTogglePressed()
        {
            return timingToggleAction != null && timingToggleAction.WasPressedThisFrame();
        }

        public bool WasTimingActionPressed()
        {
            return WasConfirmPressed();
        }

        public bool WasConfirmPressed()
        {
            return confirmAction != null && confirmAction.WasPressedThisFrame();
        }
    }
}
