using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using ShouldYouShoot.Data;

namespace ShouldYouShoot.AR
{
    /// <summary>
    /// Manages the Augmented Reality scene.
    /// Uses AR Foundation to detect planes and spawn historical character prefabs
    /// into the real-world environment via the device camera.
    /// </summary>
    [RequireComponent(typeof(ARPlaneManager))]
    [RequireComponent(typeof(ARRaycastManager))]
    public class ARModeController : MonoBehaviour
    {
        // ── Inspector References ───────────────────────────────────────────────
        [Header("AR Components")]
        [SerializeField] private ARPlaneManager arPlaneManager;
        [SerializeField] private ARRaycastManager arRaycastManager;
        [SerializeField] private Camera arCamera;

        [Header("Placement")]
        [Tooltip("Default placement distance in front of camera when no plane is detected.")]
        [SerializeField] private float defaultPlacementDistance = 1.5f;

        [Tooltip("Placement indicator shown while scanning for a surface.")]
        [SerializeField] private GameObject placementIndicatorPrefab;

        // ── State ──────────────────────────────────────────────────────────────
        private GameObject _spawnedCharacter;
        private GameObject _placementIndicator;
        private bool _placementConfirmed;
        private static readonly List<ARRaycastHit> ARHits = new List<ARRaycastHit>();

        // ── Unity Lifecycle ────────────────────────────────────────────────────
        private void Awake()
        {
            if (arCamera == null)
                arCamera = Camera.main;

            if (placementIndicatorPrefab != null)
                _placementIndicator = Instantiate(placementIndicatorPrefab);
        }

        private void OnEnable()
        {
            _placementConfirmed = false;
            SetPlacementIndicatorActive(true);
        }

        private void OnDisable()
        {
            SetPlacementIndicatorActive(false);
            DestroySpawnedCharacter();
        }

        private void Update()
        {
            if (_placementConfirmed) return;
            UpdatePlacementIndicator();
            HandleTapToPlace();
        }

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>
        /// Load and spawn a character prefab at the confirmed AR placement position.
        /// </summary>
        public void SpawnCharacter(HistoricalCharacter character)
        {
            DestroySpawnedCharacter();

            GameObject prefab = Resources.Load<GameObject>(character.PrefabResourceName);
            if (prefab == null)
            {
                Debug.LogWarning($"[ARModeController] Prefab '{character.PrefabResourceName}' not found. Using placeholder.");
                prefab = CreatePlaceholderPrefab(character.Name);
            }

            Vector3 spawnPosition = _placementIndicator != null && _placementIndicator.activeSelf
                ? _placementIndicator.transform.position
                : arCamera.transform.position + arCamera.transform.forward * defaultPlacementDistance;

            _spawnedCharacter = Instantiate(prefab, spawnPosition, Quaternion.identity);
            _spawnedCharacter.layer = LayerMask.NameToLayer("Character");

            FaceCamera(_spawnedCharacter);
            SetPlacementIndicatorActive(false);
        }

        // ── Private Helpers ────────────────────────────────────────────────────

        private void UpdatePlacementIndicator()
        {
            if (_placementIndicator == null) return;

            if (arRaycastManager.Raycast(
                    new Vector2(Screen.width / 2f, Screen.height / 2f),
                    ARHits,
                    TrackableType.PlaneWithinPolygon))
            {
                _placementIndicator.transform.SetPositionAndRotation(
                    ARHits[0].pose.position,
                    ARHits[0].pose.rotation);
                SetPlacementIndicatorActive(true);
            }
            else
            {
                SetPlacementIndicatorActive(false);
            }
        }

        private void HandleTapToPlace()
        {
#if UNITY_EDITOR
            if (Input.GetMouseButtonDown(0))
            {
                _placementConfirmed = true;
            }
#else
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                _placementConfirmed = true;
            }
#endif
        }

        private void FaceCamera(GameObject target)
        {
            if (arCamera == null) return;
            Vector3 direction = arCamera.transform.position - target.transform.position;
            direction.y = 0f;
            if (direction != Vector3.zero)
                target.transform.rotation = Quaternion.LookRotation(-direction);
        }

        private void SetPlacementIndicatorActive(bool active)
        {
            if (_placementIndicator != null)
                _placementIndicator.SetActive(active);
        }

        private void DestroySpawnedCharacter()
        {
            if (_spawnedCharacter != null)
            {
                Destroy(_spawnedCharacter);
                _spawnedCharacter = null;
            }
        }

        /// <summary>Creates a simple capsule placeholder when no prefab is assigned.</summary>
        private static GameObject CreatePlaceholderPrefab(string characterName)
        {
            GameObject placeholder = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            placeholder.name = $"Placeholder_{characterName}";
            return placeholder;
        }
    }
}
