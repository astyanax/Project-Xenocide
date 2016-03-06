using System;
using System.Collections.Generic;
using System.Text;

namespace Xenocide.Model.Geoscape.Research
{
    /// <summary>
    /// Interface for different stuff that can be granted by a ResearchTopic once it has been researched.
    /// </summary>
    public interface IResearchGrant
    {
        /// <summary>
        /// Change the GameState so it reflects the availability of this grant
        /// </summary>
        /// <param name="gameState">The GameState to modify</param>
        /// <returns>return true if grant really changed the GameState, false if it was already available before the call.</returns>
        bool Apply(GameState gameState);
    }

    /// <summary>
    /// ResearchGrant implementation that enables a particular XNET-Topic
    /// </summary>
    [Serializable]
    public class ResearchXNETGrant : IResearchGrant
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">The symbolic name of the XNETEntry to activate</param>
        public ResearchXNETGrant(string id)
        {
            this.id = id;
        }

        /// <summary>
        /// Activate the respective XNETEntry.
        /// </summary>
        /// <param name="gameState"></param>
        /// <returns></returns>
        public bool Apply(GameState gameState)
        {
            if (!gameState.XNetState[id])
            {
                gameState.XNetState.ActivateEntry(id);
                return true;
            }
            else
            {
                return false;
            }
        }

        private string id;
    }
}
