using System.Diagnostics;

using ProjectXenocide.Model.Battlescape;
using ProjectXenocide.Model.Geoscape.Vehicles;

namespace ProjectXenocide.UI.Screens
{
    public partial class AeroscapeScreen
    {
        /// <summary>
        /// Handles all game logic for the aeroscape dogfight screen: attack resolution,
        /// turn advancement, and dogfight state management.
        /// </summary>
        /// <remarks>
        /// ARCHITECTURE: This controller owns all game state mutations for dogfight
        /// simulation. The Screen class delegates to this controller for business logic
        /// and updates GUI labels based on results.
        ///
        /// GAME MECHANICS:
        /// - Aircraft and UFO take turns attacking each other
        /// - Each attack can result in: Nothing, OpponentCrashed, OpponentDestroyed,
        ///   OpponentFled, or OutOfAmmo
        /// - Dogfight ends when any non-Nothing result occurs (except Nothing)
        /// - Real-time mode advances automatically; advance-time mode advances manually
        /// </remarks>
        private class DogfightController
        {
            private readonly Aircraft aircraft;
            private readonly Ufo ufo;
            private readonly BattleLog log;

            /// <summary>
            /// Whether the dogfight has ended.
            /// </summary>
            public bool IsDogfightOver { get; private set; }

            public DogfightController(Aircraft aircraft, Ufo ufo, BattleLog log)
            {
                this.aircraft = aircraft;
                this.ufo = ufo;
                this.log = log;
            }

            /// <summary>
            /// Advances the dogfight by one turn, processing attacks until a new log entry
            /// is generated or the dogfight ends.
            /// </summary>
            /// <returns>True if the dogfight state changed (new entry or ended)</returns>
            public bool AdvanceTurn()
            {
                if (IsDogfightOver)
                    return false;

                int logsize = log.Entries.Count;
                do
                {
                    log.UpdateTime(1.0);

                    Attack(aircraft, ufo);
                    if (!IsDogfightOver)
                    {
                        if (ufo.IsArmed)
                        {
                            Attack(ufo, aircraft);
                        }
                    }
                } while (!IsDogfightOver && log.Entries.Count == logsize);

                return true;
            }

            /// <summary>
            /// Processes a single attack from attacker to target.
            /// </summary>
            private void Attack(Craft attacker, Craft target)
            {
                AttackResult result = attacker.Attack(target, log);
                switch (result)
                {
                    case AttackResult.OpponentCrashed:
                    case AttackResult.OpponentDestroyed:
                    case AttackResult.OpponentFled:
                    case AttackResult.OutOfAmmo:
                        IsDogfightOver = true;
                        break;

                    case AttackResult.Nothing:
                        break;

                    default:
                        Debug.Assert(false);
                        break;
                }
            }

            /// <summary>
            /// Ends the dogfight and cleans up vehicle states.
            /// </summary>
            public void EndDogfight()
            {
                if (!ufo.IsDestroyed)
                    ufo.OnDogfightFinished();
                if (!aircraft.IsDestroyed)
                    aircraft.OnDogfightFinished();
            }
        }
    }
}
