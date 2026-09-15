using ProjectXenocide.Model.Geoscape.Vehicles;

using Xunit;

namespace Xenocide.Test.MonoGame
{
    /// <summary>
    /// Unit tests for the pure aeroscape model helpers (no game objects needed).
    /// </summary>
    public class AeroscapeModelTests
    {
        [Fact]
        public void CooldownMultiplier_AggressiveFiresFasterThanStandardFasterThanCautious()
        {
            double aggressive = AeroscapeState.GetCooldownMultiplier(TacticalMode.Aggressive);
            double standard = AeroscapeState.GetCooldownMultiplier(TacticalMode.Standard);
            double cautious = AeroscapeState.GetCooldownMultiplier(TacticalMode.Cautious);

            Assert.True(aggressive < standard,
                "Aggressive should have a shorter cooldown than Standard.");
            Assert.True(standard < cautious,
                "Standard should have a shorter cooldown than Cautious.");
        }

        [Fact]
        public void CooldownMultiplier_IsPositiveForEveryMode()
        {
            foreach (TacticalMode mode in System.Enum.GetValues<TacticalMode>())
            {
                Assert.True(AeroscapeState.GetCooldownMultiplier(mode) > 0,
                    $"Mode {mode} should have a positive cooldown multiplier.");
            }
        }
    }
}
