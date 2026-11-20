using UnityEngine;
using UnityEngine.InputSystem;

public static class GameInput
{
    private const float Deadzone = 0.3f;
    private const float TriggerThreshold = 0.5f;

    private static bool r2WasDown;
    private static int r2Frame = -1;
    private static bool r2PressedThisFrame;

    // -1, 0 ou 1
    public static float Horizontal
    {
        get
        {
            float v = 0f;
            Keyboard k = Keyboard.current;
            if (k != null)
            {
                if (k.aKey.isPressed) v -= 1f;
                if (k.dKey.isPressed) v += 1f;
            }
            if (v != 0f) return v;

            Gamepad p = Gamepad.current;
            if (p != null)
            {
                float stick = p.leftStick.x.ReadValue();
                if (Mathf.Abs(stick) > Deadzone) return Mathf.Sign(stick);
                if (p.dpad.left.isPressed) return -1f;
                if (p.dpad.right.isPressed) return 1f;
            }
            return 0f;
        }
    }

    public static bool JumpPressed =>
        (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) ||
        (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);

    public static bool DashPressed =>
        (Keyboard.current != null && Keyboard.current.leftShiftKey.wasPressedThisFrame) ||
        R2Pressed();

    public static bool AttackPressed =>
        (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
        (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame);

    public static bool InteractPressed =>
        (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) ||
        (Gamepad.current != null && Gamepad.current.buttonNorth.wasPressedThisFrame);

    private static bool R2Pressed()
    {
        Gamepad p = Gamepad.current;
        if (p == null) return false;

        if (r2Frame != Time.frameCount)
        {
            r2Frame = Time.frameCount;
            bool down = p.rightTrigger.ReadValue() > TriggerThreshold;
            r2PressedThisFrame = down && !r2WasDown;
            r2WasDown = down;
        }
        return r2PressedThisFrame;
    }
}