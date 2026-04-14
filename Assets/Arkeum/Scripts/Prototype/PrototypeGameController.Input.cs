using UnityEngine.InputSystem;

namespace Arkeum.Prototype
{
    public sealed partial class PrototypeGameController
    {
        private void UpdateHubInput()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (TryHandleHubMovement(keyboard))
            {
                return;
            }
        }

        private void UpdateRunInput()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (TryHandleDirectionalActionInRun(keyboard))
            {
                return;
            }

            if (keyboard.qKey.wasPressedThisFrame)
            {
                ConsumeTurn("You wait and listen.");
                return;
            }

            if (keyboard.digit1Key.wasPressedThisFrame)
            {
                UseBandage();
                return;
            }

            if (keyboard.digit2Key.wasPressedThisFrame)
            {
                UseDraught();
            }
        }

        private void UpdateRunResultInput()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (keyboard.enterKey.wasPressedThisFrame)
            {
                EnterHub("The echo of return fades, and you stand before the altar again.");
            }
        }
    }
}
