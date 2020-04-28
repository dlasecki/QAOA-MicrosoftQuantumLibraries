using System;
using System.Collections.Generic;
using System.Linq;
using Accord.Math;
using Accord.Math.Optimization;
using Microsoft.Quantum.Simulation.Core;
using Microsoft.Quantum.Simulation.Simulators;

namespace Quantum.QAOA

{
    public struct DataVectors
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
        DataVectors dataVectors;
        int numberOfIterations;
        int p;
        ProblemInstance problemInstance;
        Double bestHamiltonian;
        String bestVector;
        Double[] bestBeta;
        Double[] bestGamma;
        int numberOfRandomStartingPoints;


        public ClassicalOptimization(int numberOfIterations, int p, ProblemInstance problemInstance, Double[] initialBeta, Double[] initialGamma, int numberOfRandomStartingPoints)
        {

            this.numberOfIterations = numberOfIterations;
            this.p = p;
            this.problemInstance = problemInstance;
            dataVectors.beta = initialBeta;
            dataVectors.gamma = initialGamma;
            bestHamiltonian = Double.MaxValue;
            bestVector = null;
            this.numberOfRandomStartingPoints = numberOfRandomStartingPoints;
        }

        public Double[] convertUserDataToDataVector()
        {
            return dataVectors.beta.Concat(dataVectors.gamma).ToArray();
        }

        public DataVectors convertDataVectorToVectors(Double[] bigDataVector)
        {
            int betaTermsNumber = p;
            int gammaTermsNumber = p;

            DataVectors dataVector = new DataVectors
            {

                beta = bigDataVector[0..betaTermsNumber],
                gamma = bigDataVector[betaTermsNumber..(betaTermsNumber + gammaTermsNumber)],

            };

            return dataVector;
        }

        public Double evaluateCostFunction(String result, Double[] costs)
        {
            double costFunctionValue = 0;
            for (int i = 0; i < problemInstance.ProblemSizeInBits; i++)
            {
                costFunctionValue += costs[i] * Char.GetNumericValue(result[i]);
            }

            return costFunctionValue;
        }

        public Double evaluateHamiltonian(String result)
        {
            double hamiltonianExpectation = 0;
            for (int i = 0; i < problemInstance.ProblemSizeInBits; i++)
            {
                hamiltonianExpectation += problemInstance.OneLocalHamiltonianCoefficients[i] * (1 - 2 * Char.GetNumericValue(result[i]));
            }

            for (int i = 0; i < problemInstance.ProblemSizeInBits; i++)
            {
                for (int j = i + 1; j < problemInstance.ProblemSizeInBits; j++)
                {
                    hamiltonianExpectation += problemInstance.TwoLocalHamiltonianCoefficients[i * problemInstance.ProblemSizeInBits + j] * (1 - 2 * Char.GetNumericValue(result[i])) * (1 - 2 * Char.GetNumericValue(result[j]));
                }
            }

            return hamiltonianExpectation;
        }
        public Double calculateObjectiveFunction(Double[] bigDataVector)
        {
            DataVectors dataVector = this.convertDataVectorToVectors(bigDataVector);
            Double hamiltonianExpectationValue = 0;
            List<bool[]> allSolutionVectors = new List<bool[]>();
            using (var qsim = new QuantumSimulator())
            {
                var beta = new QArray<Double>(dataVector.beta);
                Console.WriteLine("Beta");
                //Console.WriteLine(beta.Length);
                Console.WriteLine(beta);
                var gamma = new QArray<Double>(dataVector.gamma);
                Console.WriteLine("Gamma");
                //Console.WriteLine(gamma.Length);
                Console.WriteLine(gamma);
                var oneLocalHamiltonianCoefficients = new QArray<Double>(problemInstance.OneLocalHamiltonianCoefficients);
                var twoLocalHamiltonianCoefficients = new QArray<Double>(problemInstance.TwoLocalHamiltonianCoefficients);


                for (int i = 0; i < numberOfIterations; i++)
                {
                    IQArray<bool> result = QAOARunner.Run(qsim, problemInstance.ProblemSizeInBits, beta, gamma, oneLocalHamiltonianCoefficients, twoLocalHamiltonianCoefficients, p).Result;

                    allSolutionVectors.Add(result.ToArray());
                    String solutionVector = Utils.getBoolStringFromBoolArray(result.ToArray());
                    Double hamiltonianValue = evaluateHamiltonian(solutionVector);
                    hamiltonianExpectationValue += hamiltonianValue / numberOfIterations;

                }

            }
            String mostProbableSolutionVectorTemp = Utils.getModeFromBoolList(allSolutionVectors);
            if (hamiltonianExpectationValue < this.bestHamiltonian)
            {
                bestHamiltonian = hamiltonianExpectationValue;
                bestVector = mostProbableSolutionVectorTemp;
                bestBeta = dataVector.beta;
                bestGamma = dataVector.gamma;
            }

            Console.WriteLine("Best fidelity");
            Console.WriteLine(this.bestHamiltonian);
            Console.WriteLine("Best string");
            Console.WriteLine(this.bestVector);
            return hamiltonianExpectationValue;
        }

        public OptimalSolution runOptimization()
        {
            Double[] bigDataVector = convertUserDataToDataVector();

            Func<Double[], Double> objectiveFunction = calculateObjectiveFunction;
            var constraints = new[]
                {
                    new NonlinearConstraint(2*p, x =>  x[0] >= 0),
                    new NonlinearConstraint(2*p, x =>  x[0] <= Math.PI),
                    new NonlinearConstraint(2*p, x =>  x[1] >= 0),
                    new NonlinearConstraint(2*p, x =>  x[1] <= Math.PI),
                    new NonlinearConstraint(2*p, x =>  x[2] >= 0),
                    new NonlinearConstraint(2*p, x =>  x[2] <= Math.PI),
                    new NonlinearConstraint(2*p, x =>  x[3] >= 0),
                    new NonlinearConstraint(2*p, x =>  x[3] <= Math.PI),
                    new NonlinearConstraint(2*p, x =>  x[4] >= 0),
                    new NonlinearConstraint(2*p, x =>  x[4] <= Math.PI),
                    new NonlinearConstraint(2*p, x =>  x[5] >= 0),
                    new NonlinearConstraint(2*p, x =>  x[5] <= 2*Math.PI),
                    new NonlinearConstraint(2*p, x =>  x[6] >= 0),
                    new NonlinearConstraint(2*p, x =>  x[6] <= 2*Math.PI),
                    new NonlinearConstraint(2*p, x =>  x[7] >= 0),
                    new NonlinearConstraint(2*p, x =>  x[7] <= 2*Math.PI),
                    new NonlinearConstraint(2*p, x =>  x[8] >= 0),
                    new NonlinearConstraint(2*p, x =>  x[8] <= 2*Math.PI),
                    new NonlinearConstraint(2*p, x =>  x[9] >= 0),
                    new NonlinearConstraint(2*p, x =>  x[9] <= 2*Math.PI),

                };
            var optimizerObjectiveFunction = new NonlinearObjectiveFunction(2 * p, objectiveFunction);


            for (int i = 0; i < numberOfRandomStartingPoints; i++)
            {
                var cobyla = new Cobyla(optimizerObjectiveFunction);
                double[] randomVector = Utils.getRandomVectorOfSize(2 * p);
                Console.WriteLine("Random vector:");
                Console.WriteLine(randomVector);
                bool success = cobyla.Minimize(randomVector);
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
