using System;
using System.Collections.Generic;

namespace ShouldYouShoot.Data
{
    /// <summary>
    /// Represents a historical figure that appears in the game as a potential target.
    /// </summary>
    [Serializable]
    public class HistoricalCharacter
    {
        /// <summary>Unique identifier for this character.</summary>
        public string Id;

        /// <summary>Full name of the historical figure.</summary>
        public string Name;

        /// <summary>
        /// Year and location of the encounter (before the character's most notorious actions).
        /// </summary>
        public string EncounterContext;

        /// <summary>
        /// Brief description of who this person is at the moment of the encounter.
        /// Shown to the player before they decide.
        /// </summary>
        public string EncounterDescription;

        /// <summary>
        /// What happens historically if the player does NOT shoot.
        /// </summary>
        public string HistoryIfSpared;

        /// <summary>
        /// Narrative outcome shown if the player decides to shoot.
        /// </summary>
        public string OutcomeIfShot;

        /// <summary>
        /// Narrative outcome shown if the player decides to spare.
        /// </summary>
        public string OutcomeIfSpared;

        /// <summary>
        /// Whether shooting this character is considered the "morally correct" choice
        /// according to the game's scoring system (i.e., the character would go on to
        /// cause great suffering).
        /// </summary>
        public bool ShootingIsCorrectChoice;

        /// <summary>
        /// Points awarded for the correct moral decision (shoot or spare).
        /// </summary>
        public int CorrectDecisionPoints;

        /// <summary>
        /// Points deducted for the incorrect moral decision.
        /// </summary>
        public int IncorrectDecisionPoints;

        /// <summary>
        /// Name of the 3D prefab resource used to represent this character.
        /// </summary>
        public string PrefabResourceName;

        /// <summary>
        /// List of historical facts shown to the player as context clues.
        /// </summary>
        public List<string> ContextClues;
    }
}
