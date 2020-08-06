using System;
using System.Collections.Generic;
using ServicesPetriNet.Core;
using ServicesPetriNet.Examples;
using static ServicesPetriNet.Extensions;

namespace ServicesPetriNet
{
    // todo simple net
    // todo simple router https://www.youtube.com/watch?v=rYodcvhh7b8
    // 

    class Program
    {
        static void Main(string[] args)
        {
            var simulation =new SimulationController<SimpleTwoHostNetwork>();
            simulation.SimulationStep();
            Console.ReadLine();
        }
    }
}
