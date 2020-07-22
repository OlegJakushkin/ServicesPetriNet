using ServicesPetriNet;
using ServicesPetriNet.Core;
using ServicesPetriNet.Core.Attributes;
using ServicesPetriNet.Core.Transitions;
using static ServicesPetriNet.Extensions;

namespace ServicesPetriNetCore.Core.Tests
{
    public class SimpleAssistProbabiletyLoop : Group
    {
        public Place A, B, C;

        public Transition AtoC, BtoC;

        [Probabilety(0.5)]
        public Transition CtoA;

        [Probabilety(1,0,1,0,1,0)]
        public Transition CtoB;

        public SimpleAssistProbabiletyLoop()
        {
            AtoC.Action<OneToOne<Mark>>()
                .In<Mark>(A)
                .Out<Mark>(C);

            BtoC.Action<OneToOne<Mark>>()
                .In<Mark>(B)
                .Out<Mark>(C);

            CtoA.Action<OneToOne<Mark>>()
                .In<Mark>(C)
                .Out<Mark>(A);

            CtoB.Action<OneToOne<Mark>>()
                .In<Mark>(C)
                .Out<Mark>(B);

            Marks = At(A, MarkType.Create<Mark>());
        }

        public class Mark : MarkType
        {
        }
    }
}
