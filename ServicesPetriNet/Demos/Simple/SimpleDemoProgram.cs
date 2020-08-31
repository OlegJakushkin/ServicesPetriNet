using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ServicesPetriNet.Core;
using ServicesPetriNetCore.Core.Simulation.Draw;

namespace ServicesPetriNet
{
    public class SimpleDemoProgram
    {
        public static void Main()
        {
            var simulation = new SimulationController<Sample>();

            var s = simulation.DebugGraphToDot();
            Console.Write(s);
            File.WriteAllText("./simple.dot", s);

            simulation.SimulationStep();
            var result = simulation.TopGroup.C.GetMarks().First() as Mark;
            Assert.AreEqual(5 + 6, result.value);
            Console.Write("Simulation completed!");

            Console.ReadLine();
        }

        public class Mark : MarkType
        {
            public int value;
        }

        public class Add : ActionBase
        {
            public static Mark Action(Mark fromA, Mark fromB)
            {
                return new Mark {
                    value = fromA.value + fromB.value
                };
            }
        }

        public class Sample : Group
        {
            public Place A, B, C;

            private Transition Summ;

            public Sample()
            {
                Summ.Action<Add>()
                    .In<Mark>(A)
                    .In<Mark>(B)
                    .Out<Mark>(C);

                Marks = Extensions.At(A, MarkType.Create<Mark>(5))
                    .At(B, MarkType.Create<Mark>(6));
            }
        }
    }
}
