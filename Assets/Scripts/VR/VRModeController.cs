using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using ShouldYouShoot.Data;

namespace ShouldYouShoot.VR
{
    /// <summary>
    /// Manages the Virtual Reality scene.
    /// Uses the XR Interaction Toolkit to set up the VR rig and spawn historical
    /// character prefabs into the immersive 3D environment.
    /// </summary>
    public class VRModeController : MonoBehaviour
    {
        // ── Inspector References ───────────────────────────────────────────────
        [Header("VR Rig")]
        [Tooltip("XR Origin (or XR Rig) root object — the player's VR representation.")]
        [SerializeField] private GameObject xrOrigin;

        [Header("Character Spawn")]
        [Tooltip("Where in the virtual world the character will appear.")]
        [SerializeField] private Transform characterSpawnPoint;

        [Tooltip("Distance in front of the player if no spawn point is assigned.")]
        [SerializeField] private float defaultSpawnDistance = 3f;

        [Header("Environment")]
        [Tooltip("Optional skybox / environment root that can be swapped per scenario.")]
        [SerializeField] private GameObject defaultEnvironment;

        // ── State ──────────────────────────────────────────────────────────────
        private GameObject _spawnedCharacter;

        // ── Unity Lifecycle ────────────────────────────────────────────────────
        private void OnEnable()
        {
            if (xrOrigin != null)
                xrOrigin.SetActive(true);
            if (defaultEnvironment != null)
                defaultEnvironment.SetActive(true);
        }

        private void OnDisable()
        {
            DestroySpawnedCharacter();
            if (defaultEnvironment != null)
                defaultEnvironment.SetActive(false);
        }

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>
        /// Load and place a character prefab at the designated spawn point in the VR scene.
        /// </summary>
        public void SpawnCharacter(HistoricalCharacter character)
        {
            DestroySpawnedCharacter();

            GameObject prefab = Resources.Load<GameObject>(character.PrefabResourceName);
            if (prefab == null)
            {
                Debug.LogWarning($"[VRModeController] Prefab '{character.PrefabResourceName}' not found. Using placeholder.");
                prefab = CreatePlaceholderPrefab(character.Name);
            }

            Vector3 spawnPosition = GetSpawnPosition();
            _spawnedCharacter = Instantiate(prefab, spawnPosition, Quaternion.identity);
            _spawnedCharacter.layer = LayerMask.NameToLayer("Character");

            FacePlayer(_spawnedCharacter);
        }

        // ── Private Helpers ────────────────────────────────────────────────────

        private Vector3 GetSpawnPosition()
        {
            if (characterSpawnPoint != null)
                return characterSpawnPoint.position;

            Camera head = Camera.main;
            if (head != null)
                return head.transform.position + head.transform.forward * defaultSpawnDistance;

            return Vector3.forward * defaultSpawnDistance;
        }

        private void FacePlayer(GameObject target)
        {
            Camera head = Camera.main;
            if (head == null) return;

            Vector3 direction = head.transform.position - target.transform.position;
            direction.y = 0f;
            if (direction != Vector3.zero)
                target.transform.rotation = Quaternion.LookRotation(-direction);
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
