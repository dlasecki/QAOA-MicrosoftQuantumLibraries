namespace Quantum.QAOA {

   open Microsoft.Quantum.Canon;
    open Microsoft.Quantum.Intrinsic;

    //this implementation in inspired by https://github.com/stephenjordan/qaoa_tsp


    operation MixingHamiltonianEvolution(qubits: Qubit[], beta: Double) : Unit
    {
        for(i in 0..Length(qubits)-1)
        {
            R(PauliX, -2.0*beta, qubits[i]);
        }
    }

    operation ObjectiveHamiltonianEvolution(qubits: Qubit[], gamma: Double, h: Double[], J: Double[]) : Unit
    {
	    let numberOfQubits = Length(qubits);
        using (ancillaQubit = Qubit[1])
        {
            for(i in 0..numberOfQubits-1)
            {
                R(PauliZ, 2.0*gamma*h[i],qubits[i]);
            }
            for(i in 0..numberOfQubits-1)
            {
                for (j in i+1..numberOfQubits-1)
                {
                    PhaseKickback(qubits, ancillaQubit, [i,j], 2.0*gamma*J[numberOfQubits*i+j]);
                }
            }
        }
    }
}
