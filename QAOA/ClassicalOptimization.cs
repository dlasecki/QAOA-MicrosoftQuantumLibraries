using System;
using Accord.Math.Optimization;

namespace Quantum.QAOA
{
    class ClassicalOptimization
    {

        private static Double[] hamiltonianCoefficients; //probably use a more sofisticated structure

        public static Double convertUserDataToDataVector()
        {
            return 0.0;
        }

        public static Double convertDataVectorToVectors()
        {
            return 0.0;
        }

        public static Double evaluateHamiltonianExpectation(Double[] hamiltonianCoefficients, Double[] result)
        {
            return 0.0;
        }
        public static Double calculateObjectiveFunction(Double[] dataVector)
        {
            Double temp1 = convertDataVectorToVectors();
            //var result = QAOARunner.Run(qsim, 6, tx, tz, h, J, 5).Result; //we get a result string here
            //Double hamiltonianExpectation = evaluateHamiltonianExpectation(hamiltonianCoefficients);
            return 0.0;
        }


        //temp
        public static Double[] dataVector = { 1 }; //need to put all data into one vector
        //temp
        public static int numberOfVariables = 1; //probably that many first variables in dataVector considered as free, ours will be beta and gamma

        public static Double runOptimization(int numberOfVariables) //user data as another argument
        {
            Double temp1 = convertUserDataToDataVector();
           Func<Double[], Double> objectiveFunction = calculateObjectiveFunction;
           Cobyla cobyla = new Cobyla(numberOfVariables, objectiveFunction);

           return 0.0;
        }
    }
}
