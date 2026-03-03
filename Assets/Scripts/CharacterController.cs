using UnityEngine;
using ShouldYouShoot.Data;

namespace ShouldYouShoot
{
    /// <summary>
    /// Attached to a spawned historical character GameObject.
    /// Manages the character's visual state (idle, highlighted, shot, spared)
    /// and exposes the associated <see cref="HistoricalCharacter"/> data.
    /// </summary>
    public class CharacterController : MonoBehaviour
    {
        // ── State ──────────────────────────────────────────────────────────────
        public HistoricalCharacter CharacterData { get; private set; }

        [Header("Visual States")]
        [SerializeField] private Animator animator;
        [SerializeField] private Renderer characterRenderer;

        [Header("Highlight")]
        [SerializeField] private Material highlightMaterial;
        [SerializeField] private Material defaultMaterial;

        [Header("Audio")]
        [SerializeField] private AudioClip idleClip;
        [SerializeField] private AudioSource audioSource;

        private static readonly int IsHighlightedParam = Animator.StringToHash("IsHighlighted");
        private static readonly int WasShotParam       = Animator.StringToHash("WasShot");
        private static readonly int WasSparedParam     = Animator.StringToHash("WasSpared");

        // ── Unity Lifecycle ────────────────────────────────────────────────────
        private void Start()
        {
            if (audioSource != null && idleClip != null)
            {
                audioSource.clip = idleClip;
                audioSource.loop = true;
                audioSource.Play();
            }
        }

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>Initialise this character with its data payload.</summary>
        public void Initialise(HistoricalCharacter data)
        {
            CharacterData = data;
        }

        /// <summary>Highlight the character when the player aims at them.</summary>
        public void SetHighlighted(bool highlighted)
        {
            if (animator != null)
                animator.SetBool(IsHighlightedParam, highlighted);

            if (characterRenderer != null && highlightMaterial != null && defaultMaterial != null)
                characterRenderer.material = highlighted ? highlightMaterial : defaultMaterial;
        }

        /// <summary>Play the shot reaction animation.</summary>
        public void PlayShotReaction()
        {
            if (animator != null)
                animator.SetTrigger(WasShotParam);
        }

        /// <summary>Play the spared reaction animation.</summary>
        public void PlaySparedReaction()
        {
            if (animator != null)
                animator.SetTrigger(WasSparedParam);
        }
    }
}
