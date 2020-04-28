using System;
using System.Collections.Generic;
using System.Text;

namespace Quantum.QAOA
{
    public class ProblemInstance
    {
        public Double[] OneLocalHamiltonianCoefficients { get; }
        public Double[] TwoLocalHamiltonianCoefficients { get; }
        public int ProblemSizeInBits { get; }

        public ProblemInstance(Double[] oneLocalHamiltonianCoefficients, Double[] twoLocalHamiltonianCoefficients)
        {
            OneLocalHamiltonianCoefficients = oneLocalHamiltonianCoefficients;
            TwoLocalHamiltonianCoefficients = twoLocalHamiltonianCoefficients;
            ProblemSizeInBits = OneLocalHamiltonianCoefficients.Length;
        }
    }
}
