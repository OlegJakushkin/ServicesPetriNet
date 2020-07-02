using System.Collections.Generic;

namespace ServicesPetriNet.Core {
    public interface IDecomposable
    {
        bool Decomposed { get; }
        bool Decompose(List<IPart> into);
        (bool, IMarkType) Combine(List<IPart> @what);
        List<IPart> Parts { get; }
    }
}
