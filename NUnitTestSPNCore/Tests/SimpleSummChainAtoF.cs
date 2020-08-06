using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using ServicesPetriNet;
using ServicesPetriNet.Core;

namespace ServicesPetriNetCore.Core.Tests
{
    public class SimpleSummChainAtoF : Group
    {
        public Place A, B, C, D, E, F;

        private Transition SummABCtoD, SummDEtoF;

        public SimpleSummChainAtoF()
        {
            SummABCtoD.Action<Add>()
                .In<Mark>(A)
                .In<Mark>(B)
                .In<Mark>(C)
                .Out<Mark>(D);

            SummDEtoF.Action<Add>()
                .In<Mark>(D)
                .In<Mark>(E)
                .Out<Mark>(F);

            Marks = Extensions.At(A, MarkType.Create<Mark>(5))
                .At(B, MarkType.Create<Mark>(6))
                .At(C, MarkType.Create<Mark>(7))
                .At(E, MarkType.Create<Mark>(8));
        }

        public class Mark : MarkType
        {
            public int value;
        }

        public class Add
        {
            [UsedImplicitly]
            public void Action(List<Mark> marks, out Mark result)
            {
                result = new Mark {
                    value = marks.Aggregate(0, (i, mark) => i + mark.value)
                };
            }
        }
    }
}
