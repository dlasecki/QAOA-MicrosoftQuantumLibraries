using System;
using System.Collections.Generic;
using System.Linq;
using Accord.Math;
using Accord.Math.Optimization;
using Microsoft.Quantum.Simulation.Core;
using Microsoft.Quantum.Simulation.Simulators;

namespace Quantum.QAOA

{
    public struct FreeParamsVector
    {
        public Double[] beta;
        public Double[] gamma;
    }

    public struct OptimalSolution
    {
        public String optimalVector;
        public Double optimalValue;
        public Double[] optimalBeta;
        public Double[] optimalGamma;
    }

    public class ClassicalOptimization //currently support up to 2-local Hamiltonians; will be generalized later
    {
        FreeParamsVector FreeParamsVector;
        int numberOfIterations;
        int p;
        ProblemInstance problemInstance;
        Double bestHamiltonian;
        String bestVector;
        Double[] bestBeta;
        Double[] bestGamma;
        int numberOfRandomStartingPoints;


        public ClassicalOptimization(int numberOfIterations, int p, ProblemInstance problemInstance, int numberOfRandomStartingPoints = 1, Double[] initialBeta = null, Double[] initialGamma = null)
        {

            this.numberOfIterations = numberOfIterations;
            this.p = p;
            this.problemInstance = problemInstance;
            FreeParamsVector.beta = initialBeta;
            FreeParamsVector.gamma = initialGamma;
            bestHamiltonian = Double.MaxValue;
            bestVector = null;
            this.numberOfRandomStartingPoints = numberOfRandomStartingPoints;
        }

        /// # Summary
        /// Converts concatenated beta and gamma vectors into separate beta and gamma vector.
        ///
        /// # Input
        /// ## bigfreeParamsVector
        /// Concatenated beta and gamma vectors.
        ///
        /// # Output
        /// FreeParamsVector that contains beta and gamma vectors.
        ///
        /// # Remarks
        /// Useful for getting beta and gamma vectors from a concatenated vector inside the optimized function.

        public FreeParamsVector convertfreeParamsVectorToVectors(double[] bigfreeParamsVector)
        {
            int betaTermsNumber = p;
            int gammaTermsNumber = p;

            FreeParamsVector freeParamsVector = new FreeParamsVector
            {

                beta = bigfreeParamsVector[0..betaTermsNumber],
                gamma = bigfreeParamsVector[betaTermsNumber..(betaTermsNumber + gammaTermsNumber)],

            };

            return freeParamsVector;
        }

        public Double evaluateCostFunction(string result, double[] costs)
        {
            double costFunctionValue = 0;
            for (int i = 0; i < problemInstance.ProblemSizeInBits; i++)
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

        public double evaluateHamiltonian(string result)
        {
            double hamiltonianValue = 0;
            for (int i = 0; i < problemInstance.ProblemSizeInBits; i++)
            {
                hamiltonianValue += problemInstance.OneLocalHamiltonianCoefficients[i] * (1 - 2 * Char.GetNumericValue(result[i]));
            }

            for (int i = 0; i < problemInstance.ProblemSizeInBits; i++)
            {
                for (int j = i + 1; j < problemInstance.ProblemSizeInBits; j++)
                {
                    hamiltonianValue += problemInstance.TwoLocalHamiltonianCoefficients[i * problemInstance.ProblemSizeInBits + j] * (1 - 2 * Char.GetNumericValue(result[i])) * (1 - 2 * Char.GetNumericValue(result[j]));
                }
            }

            return hamiltonianValue;
        }
        public Double calculateObjectiveFunction(double[] bigfreeParamsVector)
        {
            FreeParamsVector freeParamsVector = this.convertfreeParamsVectorToVectors(bigfreeParamsVector);
            double hamiltonianExpectationValue = 0;
            List<bool[]> allSolutionVectors = new List<bool[]>();
            using (var qsim = new QuantumSimulator())
            {
                var beta = new QArray<Double>(freeParamsVector.beta);
                Console.WriteLine("Current beta vector:");
                Console.WriteLine(beta);

                var gamma = new QArray<Double>(freeParamsVector.gamma);
                Console.WriteLine("Current gamma vector:");
                Console.WriteLine(gamma);

                var oneLocalHamiltonianCoefficients = new QArray<Double>(problemInstance.OneLocalHamiltonianCoefficients);
                var twoLocalHamiltonianCoefficients = new QArray<Double>(problemInstance.TwoLocalHamiltonianCoefficients);


                for (int i = 0; i < numberOfIterations; i++)
                {
                    IQArray<bool> result = QAOARunner.Run(qsim, problemInstance.ProblemSizeInBits, beta, gamma, oneLocalHamiltonianCoefficients, twoLocalHamiltonianCoefficients, p).Result;

                    allSolutionVectors.Add(result.ToArray());
                    string solutionVector = ClassicalOptimizationUtils.getBoolStringFromBoolArray(result.ToArray());
                    double hamiltonianValue = evaluateHamiltonian(solutionVector);
                    hamiltonianExpectationValue += hamiltonianValue / numberOfIterations;

                }

            }
            String mostProbableSolutionVectorTemp = ClassicalOptimizationUtils.getModeFromBoolList(allSolutionVectors);
            if (hamiltonianExpectationValue < this.bestHamiltonian)
            {
                bestHamiltonian = hamiltonianExpectationValue;
                bestVector = mostProbableSolutionVectorTemp;
                bestBeta = freeParamsVector.beta;
                bestGamma = freeParamsVector.gamma;
            }

            Console.WriteLine("Best fidelity");
            Console.WriteLine(this.bestHamiltonian);
            Console.WriteLine("Best string");
            Console.WriteLine(this.bestVector);
            return hamiltonianExpectationValue;
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
        private NonlinearConstraint[] generateConstraints()
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
        public double[] setUpFreeParameters()
        {
            double[] betaCoefficients;
            if (FreeParamsVector.beta != null)
            {
                betaCoefficients = FreeParamsVector.beta;
            }
            else
            {
                betaCoefficients = ClassicalOptimizationUtils.getRandomVector(p, Math.PI);
            }

            double[] gammaCoefficients;
            if (FreeParamsVector.gamma != null)
            {
                gammaCoefficients = FreeParamsVector.gamma;
            }
            else
            {
                gammaCoefficients = ClassicalOptimizationUtils.getRandomVector(p, 2 * Math.PI);
            }

           return betaCoefficients.Concat(gammaCoefficients).ToArray();
        }

        public OptimalSolution runOptimization()
        {
            //double[] bigfreeParamsVector = convertUserDataTofreeParamsVector();

            Func<Double[], Double> objectiveFunction = calculateObjectiveFunction;
            
            var optimizerObjectiveFunction = new NonlinearObjectiveFunction(2 * p, objectiveFunction);

            NonlinearConstraint[] constraints = generateConstraints();

            for (int i = 0; i < numberOfRandomStartingPoints; i++)
            {
                var cobyla = new Cobyla(optimizerObjectiveFunction, constraints);
                double[] freeParameters = setUpFreeParameters();
                bool success = cobyla.Minimize(freeParameters);
                Console.WriteLine("Was success?");
                Console.WriteLine(success);

            }

            OptimalSolution optimalSolution = new OptimalSolution
            {

                optimalVector = this.bestVector,
                optimalValue = this.bestHamiltonian,
                optimalBeta = this.bestBeta,
                optimalGamma = this.bestGamma,

            };

            return optimalSolution;
        }


    }
}
