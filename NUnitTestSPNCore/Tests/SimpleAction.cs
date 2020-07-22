using System;
using JetBrains.Annotations;
using ServicesPetriNet;
using ServicesPetriNet.Core;
using ServicesPetriNet.Core.Attributes;

namespace ServicesPetriNetCore.Core.Tests
{
    public class SimpleAction : Group
    {
        public Place A, B, C;

        private Transition Summ;

        public SimpleAction()
        {
            Summ.Action<Add>()
                .In<Mark>(A)
                .In<Mark>(B)
                .Out<Mark>(C);

            Marks = Extensions.At(A, MarkType.Create<Mark>(5))
                .At(B, MarkType.Create<Mark>(6));
        }

        public class Mark : MarkType
        {
            public int value;
        }

        public class Add
        {
            [UsedImplicitly]
            public static Mark Action(Mark fromA, Mark fromB)
            {
                return  new Mark(){value = fromA.value+fromB.value};
            }
        }
    }
}
