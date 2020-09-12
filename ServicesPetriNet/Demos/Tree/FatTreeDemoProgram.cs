using System;
using System.IO;
using ServicesPetriNet.Core;
using ServicesPetriNetCore.Core.Simulation.Draw;

namespace ServicesPetriNet
{
    public class FatTreeDemoProgram
    {
        public static void Main()
        {
            var simulation = new SimulationControllerBase<FatTreeCluster>(generator: () => new FatTreeCluster());

            simulation.SimulationStep();

            var filterOut = new Type[] {typeof(FatTree), typeof(NetworkChannel) };
            var s = simulation.DebugGraphToDot(filterOut);
            Console.Write(simulation);
            File.WriteAllText("./fattree.dot", s);
            Console.ReadLine();
        }
    }
}
