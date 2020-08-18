using NUnit.Framework;
using ServicesPetriNet;
using ServicesPetriNet.Core;
using ServicesPetriNetCore.Core.Tests;

namespace NUnitTestSPNCore
{
    [TestFixture]
    public class SimulationTests
    {
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

        private readonly string path = "./sim.json";

        private SimulationController<SimpleEmptyCheck> Controller;

        [Test]
        public void TestLatestState()
        {
            var rtg = Controller.Frames.GetState();
            var sss = rtg.TopGroup.Descriptor.DebugGetMarksTree();
            Assert.AreEqual(2, rtg.TopGroup.C.GetMarks().Count);
        }

        [Test]
        public void TestPastState()
        {
            var restoredController = new SimulationController<SimpleEmptyCheck>(true, path);
            var firstStep = restoredController.Frames.GetState(0);
            Assert.AreEqual(2, firstStep.TopGroup.B.GetMarks().Count);
            Assert.AreEqual(0, firstStep.TopGroup.C.GetMarks().Count);
        }

        [Test]
        public void TestRestoredController()
        {
            var restoredController = new SimulationController<SimpleEmptyCheck>(true, path);
            var restoredSimulation = restoredController.state.TopGroup;
            var m = MarksController.Marks;
            var s = restoredSimulation.Descriptor.DebugGetMarksTree();
            Assert.AreEqual(2, restoredSimulation.C.GetMarks().Count);
            Assert.AreEqual(0, restoredSimulation.B.GetMarks().Count);
            Assert.AreNotEqual(restoredSimulation.C, Controller.state.TopGroup.C);
        }
    }
}
