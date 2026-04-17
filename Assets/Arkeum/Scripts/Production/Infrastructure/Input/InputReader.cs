using UnityEngine;
using UnityEngine.InputSystem;

namespace Arkeum.Production.Infrastructure.Input
{
    // 인풋 관리
    // 현재는 wasd / 상하좌우 키보드 입력
    // TODO ::
    // 추후 new Input System 으로 교체 하는 방향 고려
    // 다른 Input의 Input 입력 여부도 해당 클래스로 옮기기 고려
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
