using System.Collections.Generic;

namespace ServicesPetriNet.Core
{
    public interface IDecomposable
    {
        bool Decomposed { get; }
        List<IPart> Parts { get; }
        bool Decompose(List<IPart> into);
        (bool, IMarkType) Combine(List<IPart> what);
    }
}
