namespace ServicesPetriNet.Core
{
    public interface IMarkType : IPart, IDecomposable
    {
        INode Host { get; set; }
    }
}
