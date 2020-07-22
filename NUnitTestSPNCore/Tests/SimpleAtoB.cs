using System;
using ServicesPetriNet;
using ServicesPetriNet.Core;
using ServicesPetriNet.Core.Transitions;
using static ServicesPetriNet.Core.MarkType;
using static ServicesPetriNet.Extensions;
namespace ServicesPetriNetCore.Core.Tests
{
    public class SimpleAtoB : Group
    {
        public Place A, B;

        private Transition Move;

        public SimpleAtoB()
        {
            Move.Action<OneToOne<Mark>>()
                .In<Mark>(A)
                .Out<Mark>(B);

            Marks = At(A, Create<Mark>());
        }

        public class Mark : MarkType
        {
        }

    }

    public class SimpleEmptyCheck : Group
    {
        public Place A, B, C;

        private Transition Move;

        public SimpleEmptyCheck()
        {
            Move.Action<OneToOne<Mark>>()
                .In<Mark>(B)
                .Out<Mark>(A);

            Move.Action<OneToOne<Mark>>()
                .In<Mark>(A)
                .In<Mark>(B, Link.Count.None)
                .Out<Mark>(C);

            Marks = At(B, Create<Mark>()).At(B, Create<Mark>());
        }

        public class Mark : MarkType
        {
        }

    }
}
