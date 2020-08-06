using ServicesPetriNet;
using ServicesPetriNet.Core;
using ServicesPetriNet.Core.Transitions;

namespace ServicesPetriNetCore.Core.Tests
{
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

            Marks = Extensions.At(B, MarkType.Create<Mark>()).At(B, MarkType.Create<Mark>());
        }

        public class Mark : MarkType { }
    }
}
