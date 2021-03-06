﻿using System;
using System.Collections.Generic;
using System.Linq;
using Accord.Math;
using Accord.Math.Optimization;
using Microsoft.Quantum.Simulation.Core;
using Microsoft.Quantum.Simulation.Simulators;
using QAOA.ClassicalOptimization;

namespace Quantum.QAOA

{

    public class HybridQaoa //currently support up to 2-local Hamiltonians; will be generalized later
    {
        Utils.FreeParamsVector FreeParamsVector;
        int numberOfIterations;
        int p;
        ProblemInstance problemInstance;
        Double bestHamiltonianValue;
        String bestVector;
        Double[] bestBeta;
        Double[] bestGamma;
        int numberOfRandomStartingPoints;


        public HybridQaoa(int numberOfIterations, int p, ProblemInstance problemInstance, int numberOfRandomStartingPoints = 1, Double[] initialBeta = null, Double[] initialGamma = null)
        {

            this.numberOfIterations = numberOfIterations;
            this.p = p;
            this.problemInstance = problemInstance;
            this.FreeParamsVector.beta = initialBeta;
            this.FreeParamsVector.gamma = initialGamma;
            bestHamiltonianValue = Double.MaxValue;
            bestVector = null;
            this.numberOfRandomStartingPoints = numberOfRandomStartingPoints;
        }



        public Double EvaluateCostFunction(string result, double[] costs)
        {
            double costFunctionValue = 0;
            for (int i = 0; i < problemInstance.problemSizeInBits; i++)
            {
                costFunctionValue += costs[i] * Char.GetNumericValue(result[i]);
            }

            return costFunctionValue;
        }

        /// # Summary
        /// Calculates the value of the objective function Hamiltonian for a binary string provided.
        ///
        /// # Input
        /// ## result
        /// A binary string. In this context it is a result that we get after measuring the QAOA state.
        ///
        /// # Output
        /// The value of the objective function Hamiltonian.
        ///
        /// # Remarks
        /// In the binary string, 0 is mapped to 1 and 1 is mapped to -1 since (-1,1) are eigenvalues of the Z operator which is currently supported in this implementation.

        public double EvaluateHamiltonian(string result)
        {
            double hamiltonianValue = 0;
            for (int i = 0; i < problemInstance.problemSizeInBits; i++)
            {
                hamiltonianValue += problemInstance.oneLocalHamiltonianCoefficients[i] * (1 - 2 * Char.GetNumericValue(result[i]));
            }

            for (int i = 0; i < problemInstance.problemSizeInBits; i++)
            {
                for (int j = i + 1; j < problemInstance.problemSizeInBits; j++)
                {
                    hamiltonianValue += problemInstance.twoLocalHamiltonianCoefficients[i * problemInstance.problemSizeInBits + j] * (1 - 2 * Char.GetNumericValue(result[i])) * (1 - 2 * Char.GetNumericValue(result[j]));
                }
            }

            return hamiltonianValue;
        }

        /// # Summary
        /// Uses a quantum function to get a solution string from the QAOA that relies on the current values of beta and gamma vectors.
        ///To get a reasonable estimate for the expectation value of a Hamiltonian that encodes the problem, we run the QAOA many times and calculate the expectation based on solutions obtained.
        ///If the expectation of the Hamiltonian is smaller than our current best, we update our best solution to the current solution. The solution vector for the current best solution is the mode of boolean strings that we obtained from the QAOA.
        ///
        /// # Input
        /// ## bigfreeParamsVector
        /// Beta and gamma vectors concatenated.
        ///
        /// # Output
        /// The expected value of a Hamiltonian that we calculated in this run.
        public Double CalculateObjectiveFunction(double[] bigfreeParamsVector)
        {
            Utils.FreeParamsVector freeParamsVector = Utils.ConvertVectorIntoHalves(bigfreeParamsVector);
            double hamiltonianExpectationValue = 0;
            List<bool[]> allSolutionVectors = new List<bool[]>();

            var beta = new QArray<Double>(freeParamsVector.beta);
            var gamma = new QArray<Double>(freeParamsVector.gamma);

            Utils.PrintCurrentBetaGamma(beta, gamma);

            var oneLocalHamiltonianCoefficients = new QArray<Double>(problemInstance.oneLocalHamiltonianCoefficients);
            var twoLocalHamiltonianCoefficients = new QArray<Double>(problemInstance.twoLocalHamiltonianCoefficients);

            using (var qsim = new QuantumSimulator())
            {

                for (int i = 0; i < numberOfIterations; i++)
                {
                    IQArray<bool> result = RunQaoa.Run(qsim, problemInstance.problemSizeInBits, beta, gamma, oneLocalHamiltonianCoefficients, twoLocalHamiltonianCoefficients, p).Result;
                    allSolutionVectors.Add(result.ToArray());
                    string solutionVector = Utils.GetBoolStringFromBoolArray(result.ToArray());
                    double hamiltonianValue = EvaluateHamiltonian(solutionVector);
                    hamiltonianExpectationValue += (hamiltonianValue/numberOfIterations);

                }

            }
            
            UpdateBestSolution(hamiltonianExpectationValue, allSolutionVectors, freeParamsVector);
            Utils.PrintCurrentBestSolution(this.bestHamiltonianValue, this.bestVector);

            return hamiltonianExpectationValue;
        }

        /// # Summary
        /// Updates the currently best solution if a new solution is better.
        ///  
        /// # Input
        /// ## hamiltonianExpectationValue
        /// Expectation value of a Hamiltonian.
        /// ## allSolutionVectors
        /// A vector of all binary solutions that were found by a QAOA.
        /// ## freeParamsVector
        /// A vector of beta and gamma coefficients.
        private void UpdateBestSolution(double hamiltonianExpectationValue, List<bool[]> allSolutionVectors, Utils.FreeParamsVector freeParamsVector)
        {
            if (hamiltonianExpectationValue < this.bestHamiltonianValue)
            {
                String mostProbableSolutionVectorTemp = Utils.GetModeFromBoolList(allSolutionVectors);
                bestHamiltonianValue = hamiltonianExpectationValue;
                bestVector = mostProbableSolutionVectorTemp;
                bestBeta = freeParamsVector.beta;
                bestGamma = freeParamsVector.gamma;
            }
        }


        /// # Summary
        /// Generates constraints for elements in beta and gamma vectors.
        ///
        /// # Output
        /// Generated constraints.
        ///
        /// # Remarks
        /// For the canonical choice of the mixing Hamiltonian (i.e. the sum of X operators acting on single qubits), the range of values in the beta vector is 0 <= beta_i <= PI.
        /// For the objective function Hamiltonian based on Z operators, the range of values in the gamma vector is 0 <= beta_i <= 2PI.
        private NonlinearConstraint[] GenerateConstraints()
        {

            NonlinearConstraint[] constraints = new NonlinearConstraint[4*p];
            foreach (var i in Enumerable.Range(0, p).Select(x => x * 2))
            {
                int gammaIndex = 2 * p + i;
                constraints[i] = new NonlinearConstraint(2 * p, x => x[i/2] >= 0);
                constraints[i + 1] = new NonlinearConstraint(2 * p, x => x[i/2] <= Math.PI);
                constraints[gammaIndex] = new NonlinearConstraint(2 * p, x => x[gammaIndex / 2] >= 0);
                constraints[gammaIndex + 1] = new NonlinearConstraint(2 * p, x => x[gammaIndex / 2] <= 2 * Math.PI);
            }
            return constraints;
            
        }

        /// # Summary
        /// We create beta and gamma vectors. If the user provided their set of parameters, we use them for the first run. Otherwise, we use randomly generated parameters.
        ///
        /// # Output
        /// Initialized beta and gamma vectors concatenated.
        private double[] SetUpFreeParameters()
        {
            double[] betaCoefficients;
            if (FreeParamsVector.beta != null)
            {
                betaCoefficients = FreeParamsVector.beta;
                FreeParamsVector.beta = null;
            }
            else
            {
                betaCoefficients = Utils.GetRandomVector(p, Math.PI);
            }

            double[] gammaCoefficients;
            if (FreeParamsVector.gamma != null)
            {
                gammaCoefficients = FreeParamsVector.gamma;
                FreeParamsVector.gamma = null;
            }
            else
            {
                gammaCoefficients = Utils.GetRandomVector(p, 2 * Math.PI);
            }

           return betaCoefficients.Concat(gammaCoefficients).ToArray();
        }

        /// # Summary
        /// Uses a classical optimizer to change beta and gamma parameters so that the objective function is minimized. The optimization is performed some number of times to decrease the chance of getting stuck in a local minimum.
        ///
        /// # Output
        /// Optimal solution to the optimization problem input by the user.
        ///
        /// # Remarks
        /// Currently used optimizer is Cobyla which is a gradient-free optimization technique.
        /// The objective function Hamiltonian is based on Z operators, the range of values in the gamma vector is 0 <= beta_i <= 2PI.
        public OptimalSolution RunOptimization()
        {

            Func<Double[], Double> objectiveFunction = CalculateObjectiveFunction;
            
            var optimizerObjectiveFunction = new NonlinearObjectiveFunction(2 * p, objectiveFunction);

            NonlinearConstraint[] constraints = GenerateConstraints();

            for (int i = 0; i < numberOfRandomStartingPoints; i++)
            {
                var cobyla = new Cobyla(optimizerObjectiveFunction, constraints);
                double[] freeParameters = SetUpFreeParameters();
                bool success = cobyla.Minimize(freeParameters);
                Utils.PrintSuccess(success);

            }

            return new OptimalSolution(this.bestVector, this.bestHamiltonianValue, this.bestBeta, this.bestGamma);
        }


    }
}
