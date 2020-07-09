using ServicesPetriNet;
using ServicesPetriNet.Core;
using ServicesPetriNet.Core.Transitions;

using static ServicesPetriNet.Extensions;
namespace ServicesPetriNetCore.Core.Tests
{
    public class SimpleAtoB : Group<SimpleAtoB>
    {
        public Place A, B;

        private Transition Move;

        public SimpleAtoB()
        {
            Move.Action<OneToOne<Mark>>()
                .In<Mark>(A)
                .Out<Mark>(B);

            Marks = At(A, MarkType.Create<Mark>());
        }

        public class Mark : MarkType
        {
        }
    }
}
