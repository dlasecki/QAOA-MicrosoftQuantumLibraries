

namespace Microsoft.Quantum.Qaoa
{
    using System;
    using Microsoft.Quantum.Qaoa.QaoaHybrid;
    class Examples
    {


        static void Main(string[] args)
        {
            //PARAMETERS
            int numberOfIterations = 50;
            int p = 3;
            int nHamiltonianApplications = 2;
            int numberOfRandomStartingPoints = 3;

            //EXAMPLES

            //Quantum Santa (http://quantumalgorithmzoo.org/traveling_santa/)
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
            QaoaProblemInstance quantumSanta = new QaoaProblemInstance(dh, dJ);


            //MaxCut (medium.com/mdr-inc/qaoa-maxcut-using-blueqat-aaf33038f46e)
            dh = new Double[] { 0,0,0,0,0};
            dJ = new Double[]{ 0,1,0,1,0,
                               0,0,1,0,0,
                               0,0,0,1,1,
                               0,0,0,0,1,
                               0,0,0,0,0};
            QaoaProblemInstance maxCut1 = new QaoaProblemInstance(dh, dJ);
            

            //Rigetti MaxCut unit tests
            dh = new Double[]{-0.5,0,-1,0.5};
            dJ = new Double[]{0,1,2,0,
                              0,0,0.5,0,
                              0,0,0,2.5,
                              0,0,0,0};
            QaoaProblemInstance maxCut2 = new QaoaProblemInstance(dh, dJ);

            
            dh = new Double[] { 0.8, -0.5 };
            dJ = new Double[]{ 0, -1,
                               0, 0};
            QaoaProblemInstance maxCut3 = new QaoaProblemInstance(dh, dJ);

            dh = new Double[] {0, 0 };
            dJ = new Double[]{ 0, 1,
                               0, 0};
            QaoaProblemInstance maxCut4 = new QaoaProblemInstance(dh, dJ);

            //END EXAMPLES

            HybridQaoa cop = new HybridQaoa(numberOfIterations, nHamiltonianApplications, quantumSanta);

            QaoaSolution res = cop.RunOptimization(numberOfRandomStartingPoints);
            Console.WriteLine(res.SolutionVector);

            }
    }
}