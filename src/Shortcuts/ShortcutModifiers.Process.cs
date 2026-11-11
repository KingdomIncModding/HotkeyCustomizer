using UnityEngine.InputSystem;

namespace KingdomHotkeyCustomizer
{
    public sealed partial class ShortcutModifiers : IInputInteraction
    {
        public void Process(ref InputInteractionContext context)
        {
            var keyboard = context.control.device as Keyboard ?? Keyboard.current;
            bool matches = keyboard != null && (ctrl != 0) == keyboard.ctrlKey.isPressed
                && (alt != 0) == keyboard.altKey.isPressed && (shift != 0) == keyboard.shiftKey.isPressed
                && (meta != 0) == (keyboard.leftMetaKey.isPressed || keyboard.rightMetaKey.isPressed);
            if (matches && context.ControlIsActuated())
            {
                if (context.phase == InputActionPhase.Waiting) context.Started();
                if (context.phase == InputActionPhase.Started) context.PerformedAndStayPerformed();
            }
            else if (context.phase != InputActionPhase.Waiting) context.Canceled();
        }
    }
}
