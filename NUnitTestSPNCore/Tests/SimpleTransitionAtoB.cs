using ServicesPetriNet.Core;
using ServicesPetriNet.Core.Transitions;
using static ServicesPetriNet.Core.MarkType;
using static ServicesPetriNet.Extensions;

namespace ServicesPetriNetCore.Core.Tests
{
    public class SimpleTransitionAtoB : Group
    {
        public Place A, B;

        private Transition Move;

        public SimpleTransitionAtoB()
        {
            Move.Action<OneToOne<Mark>>()
                .In<Mark>(A)
                .Out<Mark>(B);

            Marks = At(A, Create<Mark>());
        }

        public class Mark : MarkType { }
    }
}
