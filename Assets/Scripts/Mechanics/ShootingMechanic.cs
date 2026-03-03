using UnityEngine;
using UnityEngine.InputSystem;
using ShouldYouShoot.Core;

namespace ShouldYouShoot.Mechanics
{
    /// <summary>
    /// Handles shooting input from both AR touch/tap and VR controller triggers.
    /// Determines whether the ray cast from the input hits a character target
    /// and notifies <see cref="GameManager"/> accordingly.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class ShootingMechanic : MonoBehaviour
    {
        // ── Inspector References ───────────────────────────────────────────────
        [Header("Input Actions")]
        [Tooltip("Input Action for triggering a shot (trigger button / screen tap).")]
        [SerializeField] private InputActionReference shootAction;

        [Tooltip("Input Action for the 'spare' / holster button.")]
        [SerializeField] private InputActionReference spareAction;

        [Header("Ray Cast")]
        [SerializeField] private float maxShootDistance = 10f;
        [SerializeField] private LayerMask characterLayerMask;

        [Header("VFX / Audio")]
        [SerializeField] private GameObject muzzleFlashPrefab;
        [SerializeField] private AudioClip gunshotClip;
        [SerializeField] private AudioClip spareSoundClip;

        // ── State ──────────────────────────────────────────────────────────────
        private bool _shootingEnabled;
        private Camera _camera;
        private AudioSource _audioSource;

        // ── Unity Lifecycle ────────────────────────────────────────────────────
        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
                _audioSource = gameObject.AddComponent<AudioSource>();
        }

        private void OnEnable()
        {
            if (shootAction != null)
            {
                shootAction.action.Enable();
                shootAction.action.performed += OnShootPerformed;
            }
            if (spareAction != null)
            {
                spareAction.action.Enable();
                spareAction.action.performed += OnSparePerformed;
            }
        }

        private void OnDisable()
        {
            if (shootAction != null)
                shootAction.action.performed -= OnShootPerformed;
            if (spareAction != null)
                spareAction.action.performed -= OnSparePerformed;
        }

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>Enable or disable shooting input processing.</summary>
        public void EnableShooting(bool enabled)
        {
            _shootingEnabled = enabled;
        }

        // ── Input Callbacks ────────────────────────────────────────────────────

        private void OnShootPerformed(InputAction.CallbackContext ctx)
        {
            if (!_shootingEnabled) return;

            if (RaycastHitsCharacter(out RaycastHit hit))
            {
                PlayGunshotEffects(hit.point);
                GameManager.Instance.RegisterShot();
            }
            // Tapping outside the character does not register as a decision
        }

        private void OnSparePerformed(InputAction.CallbackContext ctx)
        {
            if (!_shootingEnabled) return;
            PlaySpareSound();
            GameManager.Instance.RegisterSpare();
        }

        // ── Ray Cast ──────────────────────────────────────────────────────────

        private bool RaycastHitsCharacter(out RaycastHit hit)
        {
            Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            return Physics.Raycast(ray, out hit, maxShootDistance, characterLayerMask);
        }

        // ── Effects ───────────────────────────────────────────────────────────

        private void PlayGunshotEffects(Vector3 hitPoint)
        {
            if (muzzleFlashPrefab != null)
            {
                GameObject flash = Instantiate(muzzleFlashPrefab, transform.position, transform.rotation);
                Destroy(flash, 0.5f);
            }
            if (gunshotClip != null)
                _audioSource.PlayOneShot(gunshotClip);
        }

        private void PlaySpareSound()
        {
            if (spareSoundClip != null)
                _audioSource.PlayOneShot(spareSoundClip);
        }
    }
}
