using System;
using System.Collections.Generic;
using System.Linq;
using Fractions;
using ScottPlot;
using ServicesPetriNet.Core;

namespace ServicesPetriNet
{
    internal class Program
    {

        internal class PlotFrame
        {
            public double Key;
            public double Value;
            public PlotFrame(double key, double value)
            {
                Key = key;
                Value = value;
            }
        }
        private static void Main(string[] args)
        {
            var minProcessors = 1;
            var maxProcessors = 1025;
            var testSet = new List<Fraction>() {
                Fraction.FromDoubleRounded(0.5),
                Fraction.FromDoubleRounded(0.75),
                Fraction.FromDoubleRounded(0.9),
                Fraction.FromDoubleRounded(0.95),
            };


            var plotData = testSet.ToDictionary(fraction => fraction, fraction => new List<PlotFrame>());

            testSet.ForEach(
                pp =>
                {
                    for (var i = minProcessors; i < maxProcessors; i *= 2) {
                        var simulation = new SimulationControllerBase<AmdahlLaw>(
                            generator: () => new AmdahlLaw(20, i, pp),
                            strategy: SimulationStrategy.None
                        );
                        var stop = false;
                        simulation.OnUpdate(
                            simulation.state.TopGroup.Done,
                            list => { stop = true; }
                        );
                        while (!stop) simulation.SimulationStep();

                        Console.WriteLine(
                            $"processors: {i} steps: {(simulation.state.CurrentTime / simulation.state.TimeStep).ToDouble()} time: {simulation.state.CurrentTime.ToDouble()} timeStep {simulation.state.TimeStep}"
                        );
                        plotData[pp].Add(new PlotFrame(i, simulation.state.CurrentTime.ToDouble()) );
                    }
                }
            );
            var plt = new ScottPlot.Plot(600, 400);
            foreach (var kvp in plotData) {
                plt.PlotScatter(kvp.Value.Select(frame => frame.Key).ToArray(), kvp.Value.Select(frame => frame.Value).ToArray(), label: kvp.Key.ToString());
            }
            plt.Legend();
            plt.Ticks(logScaleY: true);
            plt.Title("Amdahl's law");
            plt.YLabel("Time");
            plt.XLabel("Number of processors");
            plt.SaveFig("AmdahlTime.png");

            var plt2 = new ScottPlot.Plot(600, 400);
            foreach (var kvp in plotData)
            {
                plt2.PlotScatter(kvp.Value.Select(frame => frame.Key).ToArray(), kvp.Value.Select(frame => kvp.Value[0].Value /frame.Value ).ToArray(), label: kvp.Key.ToString());
            }
            plt2.Legend();
            plt2.Ticks(logScaleY: true);
            plt2.Title("Amdahl's law");
            plt2.YLabel("SpeedUp");
            plt2.XLabel("Number of processors");
            plt2.SaveFig("AmdahlSpeedUp.png");

            Console.ReadLine();
        }
    }
}
