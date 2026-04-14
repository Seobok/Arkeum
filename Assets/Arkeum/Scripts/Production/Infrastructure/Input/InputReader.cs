using UnityEngine;
using UnityEngine.InputSystem;

namespace Arkeum.Production.Infrastructure.Input
{
    public sealed class InputReader
    {
        public bool TryGetMoveDirection(Keyboard keyboard, out Vector2Int direction)
        {
            direction = Vector2Int.zero;
            if (keyboard == null)
            {
                return false;
            }

            if (keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame)
            {
                direction = Vector2Int.up;
                return true;
            }

            if (keyboard.sKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame)
            {
                direction = Vector2Int.down;
                return true;
            }

            if (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame)
            {
                direction = Vector2Int.left;
                return true;
            }

            if (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)
            {
                direction = Vector2Int.right;
                return true;
            }

            return false;
        }
    }
}
