using System;
using ServicesPetriNet.Core;

namespace ServicesPetriNet {
    // A class for transition requirements regulation
    public class Link
    {
        public INode From;
        public INode To;
        public Type What { get; }

        public Count CountStrategy { get; }
        public int CountStrategyAmmount { get; }

        public enum Count { One, All, Some, None }

        public Link(INode @from, INode to, Type what, Count howMany, int count = -1)
        {
            From = @from;
            To = to;
            What = what;
            CountStrategy = howMany;
            CountStrategyAmmount = count;

            if (howMany == Count.One)
            {
                CountStrategyAmmount = 1;
            }
            else if (howMany == Count.None)
            {
                CountStrategyAmmount = 0;
            } else if (howMany == Count.Some && CountStrategyAmmount < 0) {
                throw new Exception("If Count is set to Some, CountStrategyAmmount shall be > 0!");
            }

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
        {
        }
    }
}
