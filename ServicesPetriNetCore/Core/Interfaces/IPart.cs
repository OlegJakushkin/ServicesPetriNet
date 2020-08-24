namespace ServicesPetriNet.Core
{
    public interface IPart
    {
        bool IsPart { get; }
        int Number { get; set; }
        int From { get; set; }
        public IMarkType Parent { get; set; }

    }
}
