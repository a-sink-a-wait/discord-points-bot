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
            string warIdA = new WarId("playerA_playerB");
            string warIdB = new WarId("playerB_playerA");

            string warIdSwapped = ((WarId) warIdA).Swapped();

            Assert.AreNotEqual(warIdA, warIdB);
            Assert.AreEqual(warIdB, warIdSwapped);

            WarId typedWarIdA = warIdA;
            WarId typedWarIdB = warIdB;
            WarId typedWarIdSwapped = warIdSwapped;

            Assert.IsTrue(typedWarIdA.EqualsId(typedWarIdB));
            Assert.IsTrue(typedWarIdA.EqualsId(typedWarIdSwapped));
        }
    }
}