using JetBrains.Annotations;
using ServicesPetriNet;
using ServicesPetriNet.Core;

namespace ServicesPetriNetCore.Core.Tests
{
    public class SimpleNamed : Group
    {
        public Place A, B, C;

        private Transition Summ;

        public SimpleNamed()
        {
            Summ.Action<BminusA>()
                .In<Mark>(A, Link.Count.One, nameof(A))
                .In<Mark>(B, Link.Count.One, nameof(B))
                .Out<Mark>(C);

            Marks = Extensions.At(A, MarkType.Create<Mark>(5))
                .At(B, MarkType.Create<Mark>(6));
        }

        public class Mark : MarkType
        {
            public int value;
        }

        public class BminusA
        {
            [UsedImplicitly]
            public Mark Action(Mark B, Mark A)
            {
                return new Mark {
                    value = B.value - A.value
                };
            }
        }
    }
}
