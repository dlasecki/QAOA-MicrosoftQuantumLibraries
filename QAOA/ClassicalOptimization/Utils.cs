using System;
using System.Collections.Generic;
using System.Text;

namespace Quantum.QAOA
{
    public class Utils
    {
        public static double[] getRandomVectorOfSize(int length)
        {
            var rand = new Random();
            double[] randomVector = new double[length];
            for (int i = 0; i < length; i++)
            {
                randomVector[i] = Math.PI * rand.NextDouble();
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
    }
}