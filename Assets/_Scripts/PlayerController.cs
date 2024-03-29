using System.Collections;
using _Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace TempleRun.Player
{
    [RequireComponent(typeof(CharacterController), typeof(PlayerInput))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float initialPlayerSpeed = 11f;
        [SerializeField] private float maximumPlayerSpeed = 25f;
        [SerializeField] private float playerSpeedIncreaseRate = .1f;
        [SerializeField] private float jumpHeight = 1.0f;
        [SerializeField] private float initialGravityValue = -9.81f;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private LayerMask turnLayer;
        [SerializeField] private LayerMask obstacleLayer;
        [SerializeField] private Animator animator;
        [SerializeField] private AnimationClip slideAnimationClip;
        [SerializeField] private float playerSpeed;
        [SerializeField] private float scoreMultiplier = 10;
        private float gravity;
        private Vector3 movementDirection = Vector3.forward;
        private Vector3 playerVelocity;

        private PlayerInput playerInput;
        private InputAction turnAction;
        private InputAction jumpAction;
        private InputAction slideAction;

        private CharacterController controller;

        private int slidingAnimationId;
        private bool sliding;
        private float score;
        private int coins;

        [SerializeField] private UnityEvent<Vector3> turnEvent;
        [SerializeField] private UnityEvent<int> gameOverEvent;
        [SerializeField] private UnityEvent<int> scoreUpdateEvent;
        [SerializeField] private UnityEvent<int> coinCollectEvent;

        [SerializeField] private TextMeshProUGUI countdownText;

        private bool isJumping;
        private bool superPowerActive;
        private const float superPowerDuration = 4f;
        private float superPowerTimer;

        private bool isPoisoned;
        private const float poisonDuration = 5f;
        private float poisonTimer;
        
        public EnvironmentController environmentController;

        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
            controller = GetComponent<CharacterController>();
            slidingAnimationId = Animator.StringToHash("Sliding");

            turnAction = playerInput.actions["Turn"];
            jumpAction = playerInput.actions["Jump"];
            slideAction = playerInput.actions["Slide"];
        }

        private void OnEnable()
        {
            turnAction.performed += PlayerTurn;
            slideAction.performed += PlayerSlide;
            jumpAction.performed += PlayerJump;
        }

        private void OnDisable()
        {
            if (turnAction != null)
            {
                turnAction.performed -= PlayerTurn;
            }

            if (slideAction != null)
            {
                slideAction.performed -= PlayerSlide;
            }

            if (jumpAction != null)
            {
                jumpAction.performed -= PlayerJump;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.name.Contains("PirateCoin"))
            {
                CollectCoin();
                Destroy(other.gameObject);
            }

            if (other.gameObject.CompareTag("SuperPower"))
            {
                ActivateSuperPower();
                Destroy(other.gameObject);
            }
            
            if (other.gameObject.CompareTag("Poison"))
            {
                ActivatePoisonEffect();
                environmentController.ActivatePoisonEffect();
                Destroy(other.gameObject);
            }
        }

        private void ActivatePoisonEffect()
        {
            isPoisoned = true;
            poisonTimer = poisonDuration;
            // Activate fog or UI overlay to impair vision
        }
        
        private void ActivateSuperPower()
        {
            superPowerActive = true;
            superPowerTimer = superPowerDuration;
            playerSpeed /= 2;
        }


        private void CollectCoin()
        {
            coins++;
            coinCollectEvent.Invoke(coins);
        }


        private void PlayerJump(InputAction.CallbackContext context)
        {
            if (IsGrounded() && !isJumping)
            {
                playerVelocity.y += Mathf.Sqrt(jumpHeight * gravity * -3f);
                controller.Move(playerVelocity * Time.deltaTime);
                isJumping = true;
            }
        }

        private void PlayerSlide(InputAction.CallbackContext context)
        {
            if (!sliding && IsGrounded())
            {
                StartCoroutine(Slide());
            }
        }

        private IEnumerator Slide()
        {
            sliding = true;

            Vector3 originalControllerCenter = controller.center;
            Vector3 newControllerCenter = originalControllerCenter;
            controller.height /= 2;

            newControllerCenter.y -= controller.height / 2;
            controller.center = newControllerCenter;
            animator.Play(slidingAnimationId);

            yield return new WaitForSeconds(slideAnimationClip.length / animator.speed);

            controller.height *= 2;
            controller.center = originalControllerCenter;
            sliding = false;
        }

        private void PlayerTurn(InputAction.CallbackContext context)
        {
            Vector3? turnPosition = CheckTurn(context.ReadValue<float>());
            if (!turnPosition.HasValue)
            {
                GameOver();
                return;
            }

            Vector3 targetDirection =
                Quaternion.AngleAxis(90 * context.ReadValue<float>(), Vector3.up) * movementDirection;

            turnEvent.Invoke(targetDirection);
            Turn(context.ReadValue<float>(), turnPosition.Value);
        }

        private void Turn(float turnValue, Vector3 turnPosition)
        {
            Vector3 tempPlayerPosition = new Vector3(turnPosition.x, transform.position.y, turnPosition.z);
            controller.enabled = false;
            transform.position = tempPlayerPosition;
            controller.enabled = true;

            Quaternion targetRotation = transform.rotation * Quaternion.Euler(0, 90 * turnValue, 0);
            transform.rotation = targetRotation;
            movementDirection = transform.forward.normalized;
        }

        private Vector3? CheckTurn(float turnValue)
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, .1f, turnLayer);

            if (hitColliders.Length != 0)
            {
                Tile tile = hitColliders[0].transform.parent.GetComponent<Tile>();
                TileType type = tile.type;
                if ((type == TileType.LEFT && turnValue == -1) ||
                    (type == TileType.RIGHT && turnValue == 1) ||
                    (type == TileType.SIDEWAYS))
                {
                    return tile.pivot.position;
                }
            }

            return null;
        }

        void Start()
        {
            playerSpeed = initialPlayerSpeed;
            gravity = initialGravityValue;
        }

        private void Update()
        {
            if (!IsGrounded(20f) && !isJumping)
            {
                GameOver();
                return;
            }

            //Score functionality
            score += scoreMultiplier * Time.deltaTime;
            scoreUpdateEvent.Invoke((int)score);
            coinCollectEvent.Invoke((int)coins);

            controller.Move(transform.forward * playerSpeed * Time.deltaTime);

            if (IsGrounded() && playerVelocity.y < 0)
            {
                playerVelocity.y = 0f;
                isJumping = false;
            }

            playerVelocity.y += gravity * Time.deltaTime;
            controller.Move(playerVelocity * Time.deltaTime);

            if (playerSpeed < maximumPlayerSpeed)
            {
                playerSpeed += Time.deltaTime * playerSpeedIncreaseRate;
                gravity = initialGravityValue - playerSpeed;

                if (animator.speed < 1.25f)
                {
                    animator.speed += (1 / playerSpeed) * Time.deltaTime;
                }
            }

            if (superPowerActive)
            {
                superPowerTimer -= Time.deltaTime;
                countdownText.text = "Speed Boost: " + superPowerTimer.ToString("F1") + "s";

                if (superPowerTimer <= 0)
                {
                    DeactivateSuperPower();
                }
            }
            
            if (isPoisoned)
            {
                poisonTimer -= Time.deltaTime;
                // Update fog or UI overlay if necessary

                if (poisonTimer <= 0)
                {
                    DeactivatePoisonEffect();
                }
            }
        }

        private bool IsGrounded(float length = .2f)
        {
            Vector3 raycastOriginFirst = transform.position;
            raycastOriginFirst.y -= controller.height / 2f;
            raycastOriginFirst.y += .1f;

            Vector3 raycastOriginSecond = raycastOriginFirst;
            raycastOriginFirst -= transform.forward * .2f;
            raycastOriginSecond += transform.forward * .2f;


            if (Physics.Raycast(raycastOriginFirst, Vector3.down, out RaycastHit hit, length, groundLayer) ||
                Physics.Raycast(raycastOriginSecond, Vector3.down, out RaycastHit hit2, length, groundLayer))
            {
                Debug.Log("Is grounded");
                return true;
            }

            Debug.Log("Is not grounded");
            return false;
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (((1 << hit.collider.gameObject.layer) & obstacleLayer) != 0)
            {
                GameOver();
            }
        }

        private void GameOver()
        {
            Debug.Log("Game over");
            gameOverEvent.Invoke((int)score);
            gameObject.SetActive(false);
        }

        private void DeactivateSuperPower()
        {
            superPowerActive = false;
            playerSpeed *= 2;
            countdownText.text = "";
        }
        
        private void DeactivatePoisonEffect()
        {
            isPoisoned = false;
            environmentController.DeactivatePoisonEffect();
        }
    }
}