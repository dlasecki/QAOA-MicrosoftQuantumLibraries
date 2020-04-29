using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Quantum.QAOA;
using System;
using System.Reflection;

namespace QAOATest.ClassicalOptimizationTests
{

    //TODO proper testing of private functions (the only public method is not deterministic)

    [TestClass]
    public class ClassicalOptimizationTest
    {

        [TestMethod]
        public void convertDataVectorToVectorsTest()
        {

            ProblemInstance problemInstance = new ProblemInstance(new double[] { 1, 2, 2, -1 }, new double[] { 5, 0, 0, 1, 1, 5, 0, 0, 3, 4, -2, -2, 8, 7, -2, 12 });

            ClassicalOptimization classicalOptimization = new ClassicalOptimization(2, 3, problemInstance, new Double[] { 1, 2, 3 }, new Double[] { 4, 5, 6 }, 1);

            FreeParamsVector result = classicalOptimization.convertfreeParamsVectorToVectors(new Double[] { 1, 2, 3, 4, 5, 6 });

            FreeParamsVector dataVectors = new FreeParamsVector();
            dataVectors.beta = new double[] { 1, 2, 3 };
            dataVectors.gamma = new double[] { 4, 5, 6 };

            FreeParamsVector expectedResult = dataVectors;

            CollectionAssert.AreEqual(expectedResult.beta, result.beta, "Hamiltonian beta value not calculated correctly.");
            CollectionAssert.AreEqual(expectedResult.gamma, result.gamma, "Hamiltonian gamma value not calculated correctly.");

        }

        [TestMethod]
        public void evaluateHamiltonianTest()
        {
            ProblemInstance problemInstance = new ProblemInstance(new double[] { 1, 2, 2, -1 }, new double[] { 5, 0, 0, 1, 1, 5, 0, 0, 3, 4, -2, -2, 8, 7, -2, 12 });

            ClassicalOptimization classicalOptimization = new ClassicalOptimization(2, 3, problemInstance, new Double[] { 1, 2, 3 }, new Double[] { 4, 5, 6 }, 1);

            Double result = classicalOptimization.evaluateHamiltonian("0011");

            Double expectedResult = -1;

            Assert.AreEqual(expectedResult, result, "Hamiltonian value not calculated correctly.");

        }

        [TestMethod]
        public void evaluateCostFunctionTest()
        {
            ProblemInstance problemInstance = new ProblemInstance(new double[] { 1, 1, 1, 1 }, new double[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 });

            ClassicalOptimization classicalOptimization = new ClassicalOptimization(2, 1, problemInstance, new double[] { 2 }, new double[] { 3 }, 1);


            string optimizationResult = "0101";

            Double result = classicalOptimization.evaluateCostFunction(optimizationResult, new double[] { 5, 3, 2, 1 });

            Double expectedResult = 4;

            Assert.AreEqual(expectedResult, result, "Cost function not calculated correctly.");


        }


    }
}
