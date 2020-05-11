using System;
using System.Collections.Generic;
using System.Text;

namespace Quantum.QAOA
{
    public class ClassicalOptimizationUtils
    {

        public struct FreeParamsVector
        {
            public Double[] beta;
            public Double[] gamma;
        }


        public static double[] getRandomVector(int length, double maximum)
        {
            var rand = new Random();
            double[] randomVector = new double[length];
            for (int i = 0; i < length; i++)
            {
                randomVector[i] = maximum * rand.NextDouble();
            }

            return randomVector;
        }

        public static String getModeFromBoolList(List<bool[]> list)
        {
            Dictionary<string, int> counter = new Dictionary<string, int>();
            foreach (bool[] boolArray in list)
            {
                String boolString = getBoolStringFromBoolArray(boolArray);
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
            }

            return result;
        }

        public static string getBoolStringFromBoolArray(bool[] boolArray)
        {
            System.Text.StringBuilder sb = new StringBuilder();
            foreach (bool b in boolArray)
            {
                sb.Append(b ? "1" : "0");
            }
            return sb.ToString();
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

        public static FreeParamsVector convertVectorIntoHalves(double[] bigfreeParamsVector)
        {
            int size = bigfreeParamsVector.Length;
            int vectorTermsNumber = size / 2;
            FreeParamsVector freeParamsVector = new FreeParamsVector
            {

                beta = bigfreeParamsVector[0..vectorTermsNumber],
                gamma = bigfreeParamsVector[vectorTermsNumber..(2*vectorTermsNumber)],

            };

            return freeParamsVector;
        }
    }
}