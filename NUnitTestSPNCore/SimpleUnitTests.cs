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
            var Simulation = new SimpleAtoB();
            Run(Simulation, 100);
            Assert.AreEqual(1, Simulation.B.GetMarks().Count);
        }
        [Test]
        public void TestAplusBtoC()
        {
            var Simulation = new SimpleAplusBtoC();
            Run(Simulation, 100);
            var result = Simulation.C.GetMarks().First() as SimpleAplusBtoC.Mark;
            Assert.AreEqual(5+6, result.value);
        }

        [Test]
        public void TestAplusBplusCtoDviaList()
        {
            var Simulation = new SimpleAplusBplusCtoDviaList();
            Run(Simulation, 100);
            var result = Simulation.D.GetMarks().First() as SimpleAplusBplusCtoDviaList.Mark;
            Assert.AreEqual(5+6+7, result.value);
        }

        [Test]
        public void TestSummChainAtoF()
        {
            var Simulation = new SimpleSummChainAtoF();
            Run(Simulation, 100);
            var result = Simulation.F.GetMarks().First() as SimpleAplusBplusCtoDviaList.Mark;
            Assert.AreEqual(5 + 6 + 7+8, result.value);
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