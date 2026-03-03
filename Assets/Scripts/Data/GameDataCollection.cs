using System;
using System.Collections.Generic;

namespace ShouldYouShoot.Data
{
    /// <summary>
    /// Root wrapper for the JSON data file that contains all historical characters
    /// and moral dilemmas used by the game.
    /// </summary>
    [Serializable]
    public class GameDataCollection
    {
        /// <summary>All playable historical character scenarios.</summary>
        public List<HistoricalCharacter> Characters;

        /// <summary>All moral dilemma scenarios, keyed to characters by CharacterId.</summary>
        public List<MoralDilemma> Dilemmas;
    }
}
