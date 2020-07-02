namespace ServicesPetriNet.Core {
    public interface IPart
    {
        int Number { get; set; }
        int From { get; set; }
        IMarkType Data { get; }
    }
}
