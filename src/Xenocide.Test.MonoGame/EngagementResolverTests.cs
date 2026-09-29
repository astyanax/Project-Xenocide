using System;
using System.Collections.Generic;

using ProjectXenocide.Model.Battlescape;

using Xunit;

namespace Xenocide.Test.MonoGame
{
    /// <summary>
    /// Tests for the Strategic Engagement resolver: determinism, decisive and
    /// stalemate outcomes, and the Monte-Carlo prediction.
    /// </summary>
    public class EngagementResolverTests
    {
        private static List<CombatantProfile> Squad(bool xcorp, int count, int accuracy, int health,
            int damage, int armor, int reactions = 40)
        {
            var list = new List<CombatantProfile>();
            for (int i = 0; i < count; ++i)
            {
                list.Add(CombatantProfile.Synthetic(
                    (xcorp ? "X" : "A") + i, xcorp, accuracy, health, reactions, damage, armor));
            }
            return list;
        }

        [Fact]
        public void SameSeed_ProducesIdenticalResult()
        {
            var xcorp = Squad(true, 4, 70, 40, 40, 5);
            var aliens = Squad(false, 4, 65, 35, 35, 3);

            var a = EngagementResolver.Simulate(xcorp, aliens, new Random(1234));
            var b = EngagementResolver.Simulate(xcorp, aliens, new Random(1234));

            Assert.Equal(a.Finish, b.Finish);
            Assert.Equal(a.Rounds, b.Rounds);
            Assert.Equal(a.XCorpKia, b.XCorpKia);
            Assert.Equal(a.AlienKills, b.AlienKills);
        }

        [Fact]
        public void OverwhelmingForce_WinsDecisively()
        {
            var xcorp = Squad(true, 6, 100, 100, 100, 20);
            var aliens = Squad(false, 1, 10, 20, 5, 0);

            var result = EngagementResolver.Simulate(xcorp, aliens, new Random(7));

            Assert.Equal(BattleFinish.XCorpVictory, result.Finish);
            Assert.Equal(1, result.AlienKills);
            Assert.Equal(0, result.XCorpKia);
        }

        [Fact]
        public void HopelessForce_IsDefeated()
        {
            var xcorp = Squad(true, 1, 10, 20, 5, 0);
            var aliens = Squad(false, 6, 100, 100, 100, 20);

            var result = EngagementResolver.Simulate(xcorp, aliens, new Random(7));

            Assert.Equal(BattleFinish.AlienVictory, result.Finish);
            Assert.Equal(1, result.XCorpKia);
        }

        [Fact]
        public void NoOneCanHurtAnyone_StalematesAtRoundCap()
        {
            var xcorp = Squad(true, 1, 100, 100, 0, 100);   // 0 damage / high armor
            var aliens = Squad(false, 1, 100, 100, 0, 100);

            var result = EngagementResolver.Simulate(xcorp, aliens, new Random(3));

            Assert.Equal(BattleFinish.Aborted, result.Finish);
            Assert.Equal(EngagementResolver.MaxRounds, result.Rounds);
            Assert.Equal(0, result.XCorpKia);
        }

        [Fact]
        public void Prediction_OverwhelmingForceHasFullWinProbability()
        {
            var xcorp = Squad(true, 6, 100, 100, 100, 20);
            var aliens = Squad(false, 1, 10, 20, 5, 0);

            var prediction = EngagementResolver.Predict(xcorp, aliens, samples: 50, seed: 99);

            Assert.Equal(1.0, prediction.WinProbability);
            Assert.Equal(0.0, prediction.ExpectedXCorpKia);
        }

        [Fact]
        public void Prediction_HopelessForceHasZeroWinProbability()
        {
            var xcorp = Squad(true, 1, 10, 20, 5, 0);
            var aliens = Squad(false, 6, 100, 100, 100, 20);

            var prediction = EngagementResolver.Predict(xcorp, aliens, samples: 50, seed: 99);

            Assert.Equal(0.0, prediction.WinProbability);
        }

        [Fact]
        public void Prediction_IsDeterministicForAGivenSeed()
        {
            var xcorp = Squad(true, 4, 70, 40, 40, 5);
            var aliens = Squad(false, 4, 65, 35, 35, 3);

            var a = EngagementResolver.Predict(xcorp, aliens, samples: 40, seed: 5);
            var b = EngagementResolver.Predict(xcorp, aliens, samples: 40, seed: 5);

            Assert.Equal(a.WinProbability, b.WinProbability);
            Assert.Equal(a.ExpectedXCorpKia, b.ExpectedXCorpKia);
        }

        [Fact]
        public void UnitsThatHit_NeverExceedTheirHealthInLosses()
        {
            var xcorp = Squad(true, 3, 100, 10, 100, 0);
            var aliens = Squad(false, 3, 100, 10, 100, 0);

            var result = EngagementResolver.Simulate(xcorp, aliens, new Random(42));

            Assert.InRange(result.XCorpKia, 0, 3);
            Assert.InRange(result.AlienKills, 0, 3);
        }
    }
}
