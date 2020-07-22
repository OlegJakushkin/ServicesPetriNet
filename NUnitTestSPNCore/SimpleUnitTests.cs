using System;
using System.Linq;
using NUnit.Framework;
using ServicesPetriNet;
using ServicesPetriNet.Core;
using ServicesPetriNetCore.Core.Tests;
using static ServicesPetriNet.Extensions;

namespace NUnitTestSPNCore
{
    public class Tests
    {

        [Test]
        public void TestAtoB()
        {
            var simulation = new SimpleAtoB();
            Run(simulation, 100);
            Assert.AreEqual(1, simulation.B.GetMarks().Count);
        }
        [Test]
        public void TestAplusBtoC()
        {
            var simulation = new SimpleAction();
            Run(simulation, 100);
            var result = simulation.C.GetMarks().First() as SimpleAction.Mark;
            Assert.AreEqual(5+6, result.value);
        }

        [Test]
        public void TestAplusBplusCtoDviaList()
        {
            var simulation = new SimpleAplusBplusCtoDviaList();
            Run(simulation, 100);
            var result = simulation.D.GetMarks().First() as SimpleAplusBplusCtoDviaList.Mark;
            Assert.AreEqual(5+6+7, result.value);
        }

        [Test]
        public void TestSummChainAtoF()
        {
            var simulation = new SimpleSummChainAtoF();
            Run(simulation, 100);
            var result = simulation.F.GetMarks().First() as SimpleAplusBplusCtoDviaList.Mark;
            Assert.AreEqual(5 + 6 + 7+8, result.value);
        }

        [Test]
        public void TestProbabilety()
        {
            var simulation = new SimpleAssistProbabiletyLoop();
            Run(simulation, 100);
            Assert.AreEqual(1, simulation.Descriptor.Marks.Count);
        }

        [Test]
        public void TestPattern()
        {
            var simulation = new SimplePattern();
            Run(simulation, 100);
            Assert.AreEqual(123+321, simulation.C.GetMarks().Aggregate(0, (i,m)=> i + ((SimplePattern.Mark)m).value));
        }

        [Test]
        public void TestMarksNaming()
        {
            var simulation = new SimpleNamed();
            Run(simulation, 100);
            Assert.AreEqual(1, simulation.C.GetMarks().Aggregate(0, (i, m) => i + ((SimplePattern.Mark)m).value));
        }
        [Test]
        public void TestEmptyPlace()
        {
            var simulation = new SimpleEmptyCheck();
            Run(simulation, 100);
            Assert.AreEqual(2, simulation.C.GetMarks().Count);
        }

        private static void Run<T>(T simulation, int steps) 
            where T : Group
        {
            var groups = GetAllGroups(simulation);
            groups.Add(nameof(simulation), simulation);
            foreach (var @group in groups)
            {
                Console.WriteLine(@group.Key);
            }
        }
    }
}