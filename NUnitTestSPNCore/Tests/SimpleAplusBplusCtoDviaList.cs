using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using ServicesPetriNet;
using ServicesPetriNet.Core;

namespace ServicesPetriNetCore.Core.Tests
{
    public class SimpleAplusBplusCtoDviaList : Group
    {
        public Place A, B, C, D;

        private Transition Summ;

        public SimpleAplusBplusCtoDviaList()
        {
            Summ.Action<Add>()
                .In<Mark>(A)
                .In<Mark>(B)
                .In<Mark>(C)
                .Out<Mark>(D);

            Marks = Extensions.At(A, MarkType.Create<Mark>(5))
                .At(B, MarkType.Create<Mark>(6))
                .At(C, MarkType.Create<Mark>(7));
        }

        public class Mark : MarkType
        {
            public int value;
        }

        public class Add
        {
            [UsedImplicitly]
            public Mark Action(List<Mark> marks)
            {
                return new Mark {
                    value = marks.Aggregate(0, (i, mark) => i + mark.value)
                };
            }
        }
    }
}
