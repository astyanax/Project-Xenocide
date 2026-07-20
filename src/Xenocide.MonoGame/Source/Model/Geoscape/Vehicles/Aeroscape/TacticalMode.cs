#region Copyright
/*
--------------------------------------------------------------------------------
This source file is part of Xenocide
  by  Project Xenocide Team

For the latest info on Xenocide, see http://www.projectxenocide.com/

This work is licensed under the Creative Commons
Attribution-NonCommercial-ShareAlike 2.5 License.

To view a copy of this license, visit
http://creativecommons.org/licenses/by-nc-sa/2.5/
or send a letter to Creative Commons, 543 Howard Street, 5th Floor,
San Francisco, California, 94105, USA.
--------------------------------------------------------------------------------
*/

/*
* @file TacticalMode.cs
* @date Created: 2026/07/20
* @author File creator: Xenocide Agent
* @author Credits: none
*/
#endregion

namespace ProjectXenocide.Model.Geoscape.Vehicles
{
    /// <summary>
    /// Tactical engagement modes for aeroscape dogfighting.
    /// Controls distance management and weapon firing behavior.
    /// </summary>
    public enum TacticalMode
    {
        /// <summary>
        /// Maximum distance (80km). Observe only, no weapons fire.
        /// Safe but cannot damage the UFO.
        /// </summary>
        Standoff,

        /// <summary>
        /// Close to maximum weapon range. Slower fire rate (1.5x cooldown).
        /// Safer engagement at range.
        /// </summary>
        Cautious,

        /// <summary>
        /// Close to minimum weapon range. Normal fire rate.
        /// Balanced risk/reward.
        /// </summary>
        Standard,

        /// <summary>
        /// Very close range (1km). Faster fire rate (0.75x cooldown).
        /// Maximum damage output but highest risk.
        /// </summary>
        Aggressive,

        /// <summary>
        /// Begin retreating. Increases distance to standoff, then ends fight.
        /// Used to disengage from unfavorable engagements.
        /// </summary>
        Disengage,
    }
}
