using System.IO;
using System.Linq;
using NUnit.Framework;
using ServicesPetriNet;
using ServicesPetriNet.Core;
using ServicesPetriNetCore.Core.Tests;

namespace NUnitTestSPNCore
{
    public class Tests
    {
        [Test]
        public void TestAtoB()
        {
            var simulation = Run<SimpleTransitionAtoB>(100);
            Assert.AreEqual(1, simulation.B.GetMarks().Count);
        }

        [Test]
        public void TestAplusBtoC()
        {
            var simulation = Run<SimpleAction>(100);
            var result = simulation.C.GetMarks().First() as SimpleAction.Mark;
            Assert.AreEqual(5 + 6, result.value);
        }

        [Test]
        public void TestAplusBplusCtoDviaList()
        {
            var simulation = Run<SimpleAplusBplusCtoDviaList>(100);
            var result = simulation.D.GetMarks().First() as SimpleAplusBplusCtoDviaList.Mark;
            Assert.AreEqual(5 + 6 + 7, result.value);
        }

        [Test]
        public void TestSummChainAtoF()
        {
            var simulation = Run<SimpleOutRefSummChainAtoF>(100);
            var result = (SimpleOutRefSummChainAtoF.Mark) simulation.F.GetMarks().First();
            Assert.AreEqual(5 + 6 + 7 + 8, result.value);
        }

        [Test]
        public void TestProbabilety()
        {
            var simulation = Run<SimpleAssistProbabiletyLoop>(100);
            Assert.AreEqual(1, simulation.Descriptor.Marks.Count);
        }

        [Test]
        public void TestPattern()
        {
            var simulation = Run<SimplePattern>(100);
            Assert.AreEqual(
                123 + 321,
                simulation.C.GetMarks().Aggregate(0, (i, m) => i + ((SimplePattern.Mark) m).value)
            );
        }

        [Test]
        public void TestMarksNaming()
        {
            var simulation = Run<SimpleNamed>(100);
            Assert.AreEqual(1, simulation.C.GetMarks().Aggregate(0, (i, m) => i + ((SimpleNamed.Mark) m).value));
        }

        [Test]
        public void TestEmptyPlace()
        {
            var simulation = Run<SimpleEmptyCheck>(100);
            var d = simulation.Descriptor.DebugGetMarksTree();
            Assert.AreEqual(2, simulation.C.GetMarks().Count);
        }

        private static T Run<T>(int steps)
            where T : Group, new()
        {
            var simulation = new SimulationController<T>();
            for (var i = 0; i < steps; i++) simulation.SimulationStep();

            return simulation.state.TopGroup;
        }
    }

    [TestFixture]
    public class SimulationTests
    {
        string path = "./sim.json";

        private SimulationController<SimpleEmptyCheck> Controller;
        [SetUp]
        public void init()
        {
            Controller = new SimulationController<SimpleEmptyCheck>(false, path);
            for (var i = 0; i < 100; i++) Controller.SimulationStep();
            Controller.Save();

            var simulation = Controller.state.TopGroup;
            var d = simulation.Descriptor.DebugGetMarksTree();
            Assert.AreEqual(2, simulation.C.GetMarks().Count);
        }

        [Test]
        public void TestLatestState()
        {
            var rtg = Controller.Frames.GetState();
            var sss = rtg.TopGroup.Descriptor.DebugGetMarksTree();
            Assert.AreEqual(2, rtg.TopGroup.C.GetMarks().Count);
        }

        [Test]
        public void TestRestoredController()
        {
            var restoredController = new SimulationController<SimpleEmptyCheck>(true, path);
            var restoredSimulation = restoredController.state.TopGroup;
            var m = MarksController.Marks;
            var s = restoredSimulation.Descriptor.DebugGetMarksTree();
            Assert.AreEqual(2, restoredSimulation.C.GetMarks().Count);
            Assert.AreNotEqual(restoredSimulation.C, Controller.state.TopGroup.C);
        }

        [Test]
        public void TestPastState()
        {
            var restoredController = new SimulationController<SimpleEmptyCheck>(true, path);
            var firstStep = restoredController.Frames.GetState(0);
            Assert.AreEqual(2, firstStep.TopGroup.B.GetMarks().Count);
        }

    }

    public class UITests
    {
        [Test]
        private static void PlotMarksInSystemOverTime() { }

        [Test]
        private static void PlotMarksInPlaceForTypeOverTime() { }

        [Test]
        private static void PlotMarksAtTimeMomentInMultiplePlaces() { }

        private static SimulationController<T> Run<T>(int steps)
            where T : Group, new()
        {
            var simulation = new SimulationController<T>();
            for (var i = 0; i < steps; i++) simulation.SimulationStep();

            return simulation;
        }
    }
}
