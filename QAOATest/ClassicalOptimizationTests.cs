using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Quantum.QAOA;
namespace QAOATest
{
    [TestClass]
    public class ClassicalOptimizationTest
    {
        [TestMethod]
        public void modeOfABoolListTest()
        {
            bool[] boolsArray1 = { false, false, true };
            bool[] boolsArray2 = { false, false, true };
            bool[] boolsArray3 = { false, false, false };
            bool[] boolsArray4 = { false, true, true };

            List<bool[]> listOfBools = new List<bool[]>();
            listOfBools.Add(boolsArray1);
            listOfBools.Add(boolsArray2);
            listOfBools.Add(boolsArray3);
            listOfBools.Add(boolsArray4);

            string expectedResult = "001";

            string result = ClassicalOptimization.modeOfABoolList(listOfBools);

            Assert.AreEqual(expectedResult, result, "Mode bool string not found correctly.");


        }
    }
}
