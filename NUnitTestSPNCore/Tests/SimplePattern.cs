using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using MiscUtil;
using ServicesPetriNet;
using ServicesPetriNet.Core;

namespace ServicesPetriNetCore.Core.Tests
{
    public class SimplePattern : Group
    {
        public Place A, B, C;

        public SimplePattern()
        {
            var aggregator = new AggregateAllPattern<Mark>(this, new[] {A, B}, C);
            RegisterPattern(aggregator);
            Marks = Extensions.At(A, MarkType.Create<Mark>(123)).At(B, MarkType.Create<Mark>(321));
        }

        public class Mark : MarkType
        {
            public int value;

            public static Mark operator +(Mark a, Mark b)
            {
                return new Mark {
                    value = a.value + b.value
                };
            }
        }

        public class AggregateAllPattern<TMark> : Pattern
            where TMark : new()
        {
            public List<Place> Inputs;
            public Place Output;

            private Transition reduction;

            public AggregateAllPattern(Group ctx, ICollection<Place> inputs, Place output) : base(ctx)
            {
                Inputs = inputs.ToList();
                Output = output;
                RegisterNode(nameof(reduction));
                reduction.Action<Act>();
                reduction.Out<TMark>(Output);
                Inputs.ForEach(place => reduction.In<Mark>(place, Link.Count.All));
            }

            private class Act : ActionBase
            {
                [UsedImplicitly]
                public TMark Action(List<TMark> inputs)
                {
                    return inputs.Aggregate(new TMark(), (mark, mark1) => Operator.Add(mark, mark1));
                }
            }
        }
    }
}
