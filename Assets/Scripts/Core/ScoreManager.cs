using System.Collections.Generic;
using UnityEngine;
using ShouldYouShoot.Data;

namespace ShouldYouShoot.Core
{
    /// <summary>
    /// Tracks the player's score across the session using a moral-weighted algorithm.
    /// Shooting decisions are scored based on historical context, whether the player
    /// read the dilemma carefully, and how quickly they decided.
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        // ── Constants ──────────────────────────────────────────────────────────
        private const int TimeoutPenalty         = -75;
        private const int MaxQuickDecisionBonus  = 30;   // max time bonus for fast correct decision
        private const int NoHintsReadPenalty     = -50;  // penalty for deciding without reading context
        private const float QuickDecisionWindow  = 5f;   // seconds for max time bonus

        // ── State ──────────────────────────────────────────────────────────────
        public int TotalScore { get; private set; }
        public int DilemmasResolved { get; private set; }
        public int CorrectDecisions { get; private set; }
        public int IncorrectDecisions { get; private set; }

        private List<DecisionRecord> _history = new List<DecisionRecord>();

        // ── Public API ─────────────────────────────────────────────────────────

        public void ResetScore()
        {
            TotalScore = 0;
            DilemmasResolved = 0;
            CorrectDecisions = 0;
            IncorrectDecisions = 0;
            _history.Clear();
        }

        /// <summary>
        /// Record a shoot/spare decision and compute the score delta.
        /// </summary>
        public void RecordDecision(
            HistoricalCharacter character,
            MoralDilemma dilemma,
            bool didShoot,
            bool readAllHints,
            bool readAnyHints,
            float decisionTimeSeconds)
        {
            bool isCorrect = (didShoot == character.ShootingIsCorrectChoice);

            int delta = isCorrect
                ? character.CorrectDecisionPoints
                : -character.IncorrectDecisionPoints;

            // Penalty for deciding without reading any context
            if (!readAnyHints)
                delta += NoHintsReadPenalty;

            // Thoughtful engagement bonus for reading all hints
            if (readAllHints)
                delta += dilemma.ThoughtfulEngagementBonus;

            // Quick-but-still-correct bonus
            if (isCorrect && decisionTimeSeconds <= QuickDecisionWindow)
            {
                float ratio = 1f - (decisionTimeSeconds / QuickDecisionWindow);
                delta += Mathf.RoundToInt(MaxQuickDecisionBonus * ratio);
            }

            ApplyDelta(delta, isCorrect, character, dilemma, didShoot);
        }

        /// <summary>Record a timeout (player did nothing within the time limit).</summary>
        public void RecordTimeout(HistoricalCharacter character, MoralDilemma dilemma)
        {
            ApplyDelta(TimeoutPenalty, false, character, dilemma, false);
        }

        /// <summary>Return the full decision history for the end-game summary screen.</summary>
        public IReadOnlyList<DecisionRecord> GetHistory() => _history.AsReadOnly();

        // ── Private Helpers ────────────────────────────────────────────────────

        private void ApplyDelta(
            int delta,
            bool wasCorrect,
            HistoricalCharacter character,
            MoralDilemma dilemma,
            bool didShoot)
        {
            TotalScore += delta;
            DilemmasResolved++;

            if (wasCorrect) CorrectDecisions++;
            else IncorrectDecisions++;

            _history.Add(new DecisionRecord
            {
                CharacterId   = character.Id,
                CharacterName = character.Name,
                DilemmaTitle  = dilemma.Title,
                DidShoot      = didShoot,
                WasCorrect    = wasCorrect,
                PointsDelta   = delta
            });

            Debug.Log($"[ScoreManager] {character.Name} — {(didShoot ? "SHOT" : "SPARED")} " +
                      $"| Correct: {wasCorrect} | Delta: {delta:+#;-#;0} | Total: {TotalScore}");
        }
    }

    /// <summary>Immutable record of a single player decision.</summary>
    public class DecisionRecord
    {
        public string CharacterId;
        public string CharacterName;
        public string DilemmaTitle;
        public bool DidShoot;
        public bool WasCorrect;
        public int PointsDelta;
    }
}
