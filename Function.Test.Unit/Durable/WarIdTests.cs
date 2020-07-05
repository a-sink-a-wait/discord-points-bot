using Durable;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Function.Test.Unit.Durable
{
    [TestClass]
    public class WarIdTests
    {
        [TestMethod]
        public void WarId_IsValid_AsString()
        {
            WarId.TryParse("playerA_playerB", out string warIdA);
            WarId.TryParse("playerB_playerA", out string warIdB);

            WarId typedWarIdA = warIdA;
            WarId typedWarIdB = warIdB;

            Assert.AreNotEqual(warIdA, warIdB);
            Assert.IsTrue(typedWarIdA.EqualsId(typedWarIdB));
        }

        [TestMethod]
        public void WarId_TryParseReturnsTrue_WhenWarIdIsNotValid()
        {
            const char Separator = '_';
            const string PlayerA = "PlayerA";
            const string PlayerB = "PlayerB";

            var warId = $"{PlayerA}{Separator}{PlayerB}";
            var didParse = WarId.TryParse(warId, out var result);

            Assert.IsNotNull(result);
            Assert.IsTrue(didParse); 
        }

        [TestMethod]
        public void WarId_TryParseReturnsFalse_WhenWarIdIsNotValid()
        {
            const char Separator = ';';
            const string PlayerA = "PlayerA";
            const string PlayerB = "PlayerB";

            var warId = $"{PlayerA}{Separator}{PlayerB}";
            var didParse = WarId.TryParse(warId, out var result);

            Assert.IsNull(result);
            Assert.IsFalse(didParse);
        }
    }
}