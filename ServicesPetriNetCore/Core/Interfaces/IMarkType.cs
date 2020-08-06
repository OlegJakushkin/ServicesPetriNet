namespace ServicesPetriNet.Core
{
    public interface IMarkType : ITreePart, IDecomposable
    {
        INode Host { get; set; }
    }
}
