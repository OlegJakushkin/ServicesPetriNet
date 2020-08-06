using System;
using ServicesPetriNet.Core;
using ServicesPetriNet.Examples;

namespace ServicesPetriNet
{
    // todo simple net
    // todo simple router https://www.youtube.com/watch?v=rYodcvhh7b8
    // 

    internal class Program
    {
        private static void Main(string[] args)
        {
            var simulation = new SimulationController<SimpleTwoHostNetwork>();
            simulation.SimulationStep();
            Console.ReadLine();
        }
    }
}
