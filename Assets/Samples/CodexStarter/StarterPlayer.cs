using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace CodexStarter
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class StarterPlayer : MonoBehaviour
    {
        [SerializeField] private float speed = 5f;
        [SerializeField] private float jumpHeight = 1.5f;
        [SerializeField] private float gravity = -20f;
        private CharacterController controller;
        private float verticalSpeed;
        private Vector3 spawn;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            spawn = transform.position;
        }

        private void Update()
        {
            Vector2 input = Vector2.zero;
            bool jump = false;
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                input.x = (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1f : 0f)
                        - (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1f : 0f);
                input.y = (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1f : 0f)
                        - (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1f : 0f);
                jump = keyboard.spaceKey.wasPressedThisFrame;
            }
#else
            input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            jump = Input.GetKeyDown(KeyCode.Space);
#endif
            input = Vector2.ClampMagnitude(input, 1f);
            if (controller.isGrounded && verticalSpeed < 0f) verticalSpeed = -2f;
            if (controller.isGrounded && jump) verticalSpeed = Mathf.Sqrt(jumpHeight * -2f * gravity);
            verticalSpeed += gravity * Time.deltaTime;
            var movement = new Vector3(input.x * speed, verticalSpeed, input.y * speed);
            var hit = controller.Move(movement * Time.deltaTime);
            if ((hit & CollisionFlags.Above) != 0 && verticalSpeed > 0f) verticalSpeed = 0f;
            if (transform.position.y < -15f)
            {
                controller.enabled = false;
                transform.position = spawn;
                controller.enabled = true;
                verticalSpeed = 0f;
            }
        }
    }
}
