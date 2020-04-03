using System;
using ServicesPetriNet.Core;

namespace ServicesPetriNet {
    public class Link
    {
        public INode From;
        public INode To;
        public Type What { get; }

        public Count CountStrategy { get; }
        public int CountStrategyAmmount { get; }

        public enum Count { One, All, Some }

        public Link(INode @from, INode to, Type what, Count howMany, int count = -1)
        {
            From = @from;
            To = to;
            What = what;
            CountStrategy = howMany;
            CountStrategyAmmount = count;
        }
    }

    public class Link<T> : Link
    {
        public Link(INode @from, INode to, Count howMany = Count.One, int count = -1) : base(
            @from,
            to,
            typeof(T),
            howMany,
            count
        )
        { }
    }
}
