namespace ServicesPetriNet.Core {
    public interface ITreePart
    {
        bool HasParent { get; }
        IMarkType Parent { get; set; }
    }
}
