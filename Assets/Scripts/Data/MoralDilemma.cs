using System;
using System.Collections.Generic;

namespace ShouldYouShoot.Data
{
    /// <summary>
    /// Represents a single moral dilemma scenario presented to the player.
    /// </summary>
    [Serializable]
    public class MoralDilemma
    {
        /// <summary>Unique identifier for this dilemma.</summary>
        public string Id;

        /// <summary>Reference to the historical character featured in this dilemma.</summary>
        public string CharacterId;

        /// <summary>Title displayed on screen when the dilemma begins.</summary>
        public string Title;

        /// <summary>
        /// Full scenario description — sets the scene for the player.
        /// </summary>
        public string ScenarioText;

        /// <summary>
        /// Time limit in seconds the player has to make a decision.
        /// Zero means no time limit.
        /// </summary>
        public float DecisionTimeLimit;

        /// <summary>
        /// Ordered list of hint/context lines revealed progressively if the player
        /// takes time to read before deciding.
        /// </summary>
        public List<string> ProgressiveHints;

        /// <summary>
        /// Philosophical question posed to the player (e.g. "Does preventing future evil
        /// justify ending an innocent life today?").
        /// </summary>
        public string PhilosophicalQuestion;

        /// <summary>
        /// Additional points awarded for reading all context clues before deciding.
        /// Rewards thoughtful engagement.
        /// </summary>
        public int ThoughtfulEngagementBonus;
    }
}
