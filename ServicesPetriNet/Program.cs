using System;
using Fractions;
using ServicesPetriNet.Core;

namespace ServicesPetriNet
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var minProcessors = 1;
            var maxProcessors = 2049;

            var pp = Fraction.FromDoubleRounded(0.5);
            for (var i = minProcessors; i < maxProcessors; i*=2) {
                var simulation = new SimulationControllerBase<AmdahlLaw>(generator: () => new AmdahlLaw(20, i, pp), strategy:SimulationStrategy.None);
                var stop = false;
                simulation.OnUpdate(
                    simulation.state.TopGroup.Done,
                    list =>
                    {
                        stop = true;
                    }
                );
                while (!stop) simulation.SimulationStep();

                Console.WriteLine(
                    $"processors: {i} steps: {(simulation.state.CurrentTime / simulation.state.TimeStep).ToDouble()} time: {simulation.state.CurrentTime.ToDouble()} timeStep {simulation.state.TimeStep}"
                );
            }

            Console.ReadLine();
        }
    }
}
