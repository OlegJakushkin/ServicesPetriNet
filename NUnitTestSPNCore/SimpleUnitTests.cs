using System.Linq;
using NUnit.Framework;
using ServicesPetriNet;
using ServicesPetriNet.Core;
using ServicesPetriNetCore.Core.Tests;

namespace NUnitTestSPNCore
{
    public class AdvancedTests
    {
        [Test]
        public void TestPreciseSameTypeMarkPlacementAtoBandC()
        {
            var simulation = Test.Run<ActionOverloadAtoBandCAdvancedAction>(100);
            Assert.AreEqual(true, simulation.A.GetMarks().Count == 0);
            Assert.AreEqual(true, simulation.B.GetMarks().Cast<ActionOverloadAtoBandCAdvancedAction.Mark>().First().value == 123);
            Assert.AreEqual(true, simulation.C.GetMarks().Cast<ActionOverloadAtoBandCAdvancedAction.Mark>().First().value == 321);
        }
    }
    public class SimpleTests
    {
        [Test]
        public void TestAtoB()
        {
            var simulation = Test.Run<SimpleTransitionAtoB>(100);
            Assert.AreEqual(1, simulation.B.GetMarks().Count);
        }

        [Test]
        public void TestAplusBtoC()
        {
            var simulation = Test.Run<SimpleAction>(100);
            var result = simulation.C.GetMarks().First() as SimpleAction.Mark;
            Assert.AreEqual(5 + 6, result.value);
        }

        [Test]
        public void TestAplusBplusCtoDviaList()
        {
            var simulation = Test.Run<SimpleAplusBplusCtoDviaList>(100);
            var result = simulation.D.GetMarks().First() as SimpleAplusBplusCtoDviaList.Mark;
            Assert.AreEqual(5 + 6 + 7, result.value);
        }

        [Test]
        public void TestSummChainAtoF()
        {
            var simulation = Test.Run<SimpleOutRefSummChainAtoF>(100);
            var result = (SimpleOutRefSummChainAtoF.Mark) simulation.F.GetMarks().First();
            Assert.AreEqual(5 + 6 + 7 + 8, result.value);
        }

        [Test]
        public void TestProbabilety()
        {
            var simulation = Test.Run<SimpleAssistProbabiletyLoop>(100);
            Assert.AreEqual(1, simulation.Descriptor.Marks.Count);
        }

        [Test]
        public void TestPattern()
        {
            var simulation = Test.Run<SimplePattern>(100);
            Assert.AreEqual(
                123 + 321,
                simulation.C.GetMarks().Aggregate(0, (i, m) => i + ((SimplePattern.Mark) m).value)
            );
        }

        [Test]
        public void TestMarksNaming()
        {
            var simulation = Test.Run<SimpleNamed>(100);
            Assert.AreEqual(1, simulation.C.GetMarks().Aggregate(0, (i, m) => i + ((SimpleNamed.Mark) m).value));
        }

        [Test]
        public void TestEmptyPlace()
        {
            var simulation = Test.Run<SimpleEmptyCheck>(100);
            var d = simulation.Descriptor.DebugGetMarksTree();
            Assert.AreEqual(2, simulation.C.GetMarks().Count);
        }


    }

    public class Test
    {
        public static T Run<T>(int steps)
            where T : Group, new()
        {
            var simulation = new SimulationController<T>();
            for (var i = 0; i < steps; i++) simulation.SimulationStep();

            return simulation.state.TopGroup;
        }
    }
}
