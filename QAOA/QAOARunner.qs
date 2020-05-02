namespace Quantum.QAOA {

    open Microsoft.Quantum.Canon;
    open Microsoft.Quantum.Intrinsic;

    //this implementation in inspired by https://github.com/stephenjordan/qaoa_tsp

    operation QAOARunner(problemSize: Int, beta: Double[], gamma: Double[], h: Double[], J: Double[], p: Int) : Bool[]
    {
        
        mutable result = new Bool[problemSize];
        using (x = Qubit[problemSize])
        {
            ApplyToEach(H, x);                          // prepare the uniform distribution
            for (i in 0..p-1)
            {
                ObjectiveHamiltonianEvolution(x, gamma[i], h, J);    // do Exp(-i H_C tz[i])
                MixingHamiltonianEvolution(x, beta[i]);            // do Exp(-i H_0 tx[i])
            }
            set result = MeasureAllAndReset(x);                 // measure in the computational basis
        }
        return result;
    }
}
