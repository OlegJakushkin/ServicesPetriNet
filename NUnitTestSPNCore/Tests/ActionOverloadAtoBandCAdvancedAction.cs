using System;
using System.Linq;
using ServicesPetriNet;
using ServicesPetriNet.Core;

namespace ServicesPetriNetCore.Core.Tests
{
    public class ActionOverloadAtoBandCAdvancedAction : Group
    {
        public Place A, B, C;

        private Transition Summ;

        public ActionOverloadAtoBandCAdvancedAction()
        {
            Summ.Action<Add>()
                .In<Mark>(A)
                .Out<Mark>(B)
                .Out<Mark>(C);

            Marks = Extensions.At(A, MarkType.Create<Mark>(5));
        }

        public class Mark : MarkType
        {
            public int value;
        }

        public class Add : ActionBase
        {
            public override void ActionCaller()
            {
                var ins = Inputs.Values.SelectMany(list => list).Cast<Mark>().First();
                if (ins.value != 5)
                {
                    throw new Exception("Bad test inputs");
                }
                Outputs.First((p) => p.Key.Name == "B").Value.Add(MarkType.Create<Mark>(123));
                Outputs.First((p) => p.Key.Name == "C").Value.Add(MarkType.Create<Mark>(321));
            }
        
        }
    }
}