using ServicesPetriNet;
using ServicesPetriNet.Core;
using ServicesPetriNet.Core.Attributes;
using ServicesPetriNet.Core.Transitions;

namespace ServicesPetriNetCore.Core.Tests
{
    public class SimpleSubgroupTest : Group
    {
        public SimpleSubgroup A;
        public SimpleSubgroup B;

        [TimeScale(3)]
        private Transition MoveAB;

        private Transition MoveBA;

        public SimpleSubgroupTest()
        {
            MoveAB.Action<OneToOne<Mark>>()
                .In<Mark>(A.B)
                .Out<Mark>(B.A);

            MoveBA.Action<OneToOne<Mark>>()
                .In<Mark>(B.B)
                .Out<Mark>(A.A);
        }

        public class Mark : MarkType { }

        public class SimpleSubgroup : Group
        {
            public Place A, B;

            private Transition Move;

            public SimpleSubgroup()
            {
                Move.Action<OneToOne<Mark>>()
                    .In<Mark>(A)
                    .Out<Mark>(B);

                Marks = Extensions.At(A, MarkType.Create<Mark>());
            }
        }
    }
}
