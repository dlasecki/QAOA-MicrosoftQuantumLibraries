using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Accord.Math.Optimization;
using Microsoft.Quantum.Arrays;
using Microsoft.Quantum.Simulation.Core;
using Microsoft.Quantum.Simulation.Simulators;

namespace Quantum.QAOA

{
    public struct DataVectors
    {
        public Double[] beta;
        public Double[] gamma;
    }

    public class ClassicalOptimization //currently support up to 2-local Hamiltonians; will be generalized later
    {
        private DataVectors dataVectors;
        private int numberOfIterations;
        private int problemSizeInBits;
        private int p;
        private Double[] oneLocalHamiltonianCoefficients;
        private Double[] twoLocalHamiltonianCoefficients;
        private Double[] costs;
        private Double bestHamiltonian;
        private String bestVector;
        private double[] results = new double[5];


        public ClassicalOptimization(int numberOfIterations, int problemSizeInBits, int p, Double[] costs, Double[] oneLocalHamiltonianCoefficients, Double[] twoLocalHamiltonianCoefficients, Double[] initialBeta, Double[] initialGamma)
        {

            this.numberOfIterations = numberOfIterations;
            this.problemSizeInBits = problemSizeInBits;
            this.p = p;
            this.costs = costs;
            this.oneLocalHamiltonianCoefficients = oneLocalHamiltonianCoefficients;
            this.twoLocalHamiltonianCoefficients = twoLocalHamiltonianCoefficients;
            dataVectors.beta = initialBeta;
            dataVectors.gamma = initialGamma;
            bestHamiltonian = 1000000000;
            bestVector = "";
        }

        private Double[] convertUserDataToDataVector()
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

        public Double evaluateCostFunction(String result)
        {
            double costFunctionValue = 0;
            for (int i = 0; i < problemSizeInBits; i++)
            {
                costFunctionValue += costs[i] * Char.GetNumericValue(result[i]);
            }

            return costFunctionValue;
        }

        public Double evaluateHamiltonian(String result)
        {
            double hamiltonianExpectation = 0;
            for (int i = 0; i < problemSizeInBits; i++)
            {
                hamiltonianExpectation += oneLocalHamiltonianCoefficients[i]*(1-2*Char.GetNumericValue(result[i]));
            }

            for (int i = 0; i < problemSizeInBits; i++)
            {
                for (int j = i + 1; j < problemSizeInBits; j++)
                {
                    hamiltonianExpectation += twoLocalHamiltonianCoefficients[i*problemSizeInBits+j] * (1-2*Char.GetNumericValue(result[i])) * (1-2*Char.GetNumericValue(result[j]));
                }
            }

            //Console.WriteLine("Ham exp");
            //Console.WriteLine(hamiltonianExpectation);

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
                var oneLocalHamiltonianCoefficients = new QArray<Double>(this.oneLocalHamiltonianCoefficients);
                var twoLocalHamiltonianCoefficients = new QArray<Double>(this.twoLocalHamiltonianCoefficients);

                
                for (int i = 0; i < numberOfIterations; i++)
                {
                    IQArray<bool> result = QAOARunner.Run(qsim, problemSizeInBits, beta, gamma, oneLocalHamiltonianCoefficients, twoLocalHamiltonianCoefficients, p).Result;
                    //Console.WriteLine("Result");
                    //Console.WriteLine(result);
                    allSolutionVectors.Add(result.ToArray());
                    String solutionVector = boolStringFromBoolArray(result.ToArray());
                    Double hamiltonianValue = evaluateHamiltonian(solutionVector);
                    hamiltonianExpectationValue += hamiltonianValue / numberOfIterations;
                }

            }
            String mostProbableSolutionVectorTemp = modeOfABoolList(allSolutionVectors);
            if(hamiltonianExpectationValue < this.bestHamiltonian)
            {
                this.bestHamiltonian = hamiltonianExpectationValue;
                this.bestVector = mostProbableSolutionVectorTemp;
            }

            Console.WriteLine("Best fidelity");
            Console.WriteLine(this.bestHamiltonian);
            Console.WriteLine("Best string");
            Console.WriteLine(this.bestVector);
            return hamiltonianExpectationValue;
        }

        public String runOptimization()
        {
            Double[] bigDataVector = convertUserDataToDataVector();
            bigDataVector[3] -= 0.04;
            bigDataVector[8] += 0.02;

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
            var f = new NonlinearObjectiveFunction(2*p, objectiveFunction);

            // Under the following constraints
            

            // Create a Cobyla algorithm for the problem
            

            for (int i = 0; i < 5; i++)
            {
                var cobyla = new Cobyla(f, constraints);
                double[] randomVector = this.randomVector(2 * p);
                Console.WriteLine("Random vector:");
                Console.WriteLine(randomVector);
                bool success = cobyla.Minimize(randomVector);
                Console.WriteLine("Was success?");
                Console.WriteLine(success);
                double minimum = cobyla.Value; //optimal expectation value
                double[] solution = cobyla.Solution; //beta, gamma
                this.results[i] = minimum;
            }
            Console.WriteLine(results[0]);
            Console.WriteLine(results[1]);
            Console.WriteLine(results[2]);
            Console.WriteLine(results[3]);
            Console.WriteLine(results[4]);

            /*var lbfgs = new NelderMead(f);
            bool succes = lbfgs.Minimize();*/


            return bestVector;
        }

        public double[] randomVector(int length)
        {
            var rand = new Random();
            double[] randomVector = new double[length];
            for(int i = 0; i < length; i++)
            {
                randomVector[i] = Math.PI*rand.NextDouble();
            }

            return randomVector;
        }

        public static String modeOfABoolList(List<bool[]> list)
        {
            Dictionary<string, int> counter = new Dictionary<string, int>();
            foreach (bool[] boolArray in list)
            {
                String boolString = boolStringFromBoolArray(boolArray);
                if (counter.ContainsKey(boolString))
                {
                    counter[boolString] += 1;
                }
                else
                {
                    counter[boolString] = 1;
                }
                
            }
            int maxi = 0;
            String result = null;
            foreach (string key in counter.Keys)
            {
                if (counter[key] > maxi)
                {
                    maxi = counter[key];
                    result = key;
                }
                //Console.WriteLine(key);
                //Console.WriteLine(counter[key]);
            }
            
            return result;
        }

        public static string boolStringFromBoolArray(bool[] boolArray)
        {
            System.Text.StringBuilder sb = new StringBuilder();
            foreach (bool b in boolArray)
            {
                sb.Append(b ? "1" : "0");
            }
            return sb.ToString();
        }
    }
}
