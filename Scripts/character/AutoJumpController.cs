using ECM2;
using UnityEngine;

namespace character
{
    public class AutoJumpController : MonoBehaviour
    {
        [SerializeField] private ECM2.Character _character;

        [Space]
        [Tooltip("Enable or disable auto-jump. Can be toggled at runtime.")]
        [SerializeField] private bool _autoJumpEnabled = true;

        [Tooltip("Target height to jump onto (in Unity units). Set to match your block size, e.g. 1.1 for 1-block steps.")]
        [SerializeField] private float _stepHeight = 1.1f;

        [Tooltip("Forward raycast distance used to verify there is clearance above the obstacle.")]
        [SerializeField] private float _forwardCheckDistance = 1.0f;

        [Tooltip("Extra clearance above stepHeight for the passability check ray (keeps character from clipping).")]
        [SerializeField] private float _clearanceAboveStep = 0.5f;

        [Tooltip("Minimum movement speed to trigger auto-jump (avoids triggering while barely touching a wall).")]
        [SerializeField] private float _movementThreshold = 0.3f;

        [Tooltip("Cooldown between auto-jumps to prevent rapid re-triggering.")]
        [SerializeField] private float _cooldownDuration = 0.4f;

        private float _cooldown;

        public bool IsAutoJumpEnabled
        {
            get => _autoJumpEnabled;
            set => _autoJumpEnabled = value;
        }

        private void Reset()
        {
            _character = GetComponent<ECM2.Character>();
        }

        private void Awake()
        {
            if (_character == null)
                _character = GetComponent<ECM2.Character>();
        }

        private void OnEnable()
        {
            _character.Collided += OnCollided;
            _character.BeforeSimulationUpdated += Tick;
        }

        private void OnDisable()
        {
            _character.BeforeSimulationUpdated -= Tick;
            _character.Collided -= OnCollided;
        }

        private void Tick(float deltaTime)
        {
            if (_cooldown > 0f)
                _cooldown -= deltaTime;
        }

        private void OnCollided(ref CollisionResult collision)
        {
            if (!_autoJumpEnabled || _cooldown > 0f)
                return;

            // Only react to side collisions while grounded
            if (collision.hitLocation != HitLocation.Sides)
                return;

            if (!_character.IsWalking())
                return;

            // Require the character to be actively moving forward
            Vector3 movDir = _character.GetMovementDirection();
            if (movDir.sqrMagnitude < _movementThreshold * _movementThreshold)
                return;

            movDir.y = 0f;
            if (movDir.sqrMagnitude < 0.01f)
                return;
            movDir.Normalize();

            // Cast a ray forward at height (stepHeight + clearance) above feet.
            // If it hits something → the obstacle is taller than 1 block (a wall) → don't jump.
            // If it misses → there's open space above the obstacle → safe to auto-jump.
            Vector3 clearanceOrigin = transform.position + Vector3.up * (_stepHeight + _clearanceAboveStep);
            if (Physics.Raycast(clearanceOrigin, movDir, _forwardCheckDistance))
                return;

            DoAutoJump();
            _cooldown = _cooldownDuration;
        }

        private void DoAutoJump()
        {
            Vector3 worldUp = -_character.GetGravityDirection();

            // Calculate the exact vertical impulse needed to reach stepHeight.
            // Using v = sqrt(2 * g * h) — this is always less than the normal jump impulse.
            float impulse = Mathf.Sqrt(2f * _character.GetGravityMagnitude() * _stepHeight);

            _character.SetMovementMode(ECM2.Character.MovementMode.Falling);
            _character.PauseGroundConstraint();
            _character.LaunchCharacter(worldUp * impulse, overrideVerticalVelocity: true);
        }
    }
}
