using System;
using Microsoft.Quantum.Simulation.Core;
using Microsoft.Quantum.Simulation.Simulators;

namespace Quantum.QAOA
{
    class Driver
    {
        static void Main(string[] args)
        {
            using (var qsim = new QuantumSimulator())
            {
                double[] dtx = { 0.619193, 0.742566, 0.060035, -1.568955, 0.045490 };
                double[] dtz = { 3.182203, -1.139045, 0.221082, 0.537753, -0.417222 };
                double[] segmentCosts = { 4.70, 9.09, 9.03, 5.70, 8.02, 1.71 };
                double[] dh = { 4 * 20 - 0.5 * 4.7, 4 * 20 - 0.5 * 9.09, 4 * 20 - 0.5 * 9.03, 4 * 20 - 0.5 * 5.70, 4 * 20 - 0.5 * 8.02, 4 * 20 - 0.5 * 1.71 };
                double[] dJ = { 40.0,40.0,20.0,40.0,40.0,40.0,
                40.0,40.0,40.0,20.0,40.0,40.0,
                40.0,40.0,40.0,40.0,40.0,40.0,
                40.0,40.0,40.0,40.0,40.0,40.0,
                40.0,40.0,40.0,40.0,40.0,20.0,
                40.0,40.0,40.0,40.0,40.0,40.0};

                // Convert parameters to QArray<Double> to pass them to Q#
                var tx = new QArray<Double>(dtx);
                var tz = new QArray<Double>(dtz);
                var costs = new QArray<Double>(segmentCosts);
                var h = new QArray<Double>(dh);
                var J = new QArray<Double>(dJ);
                for (int i = 0; i < 100; i++)
                {
                    var result = QAOARunner.Run(qsim, 6, tx, tz, h, J, 5).Result;
                    Console.WriteLine(result);
                }
            }
        }
    }
}