using System;
using System.Collections.Generic;
using System.Linq;
using Dynamitey.DynamicObjects;
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
               // Fraction.FromDoubleRounded(0.5),
               // Fraction.FromDoubleRounded(0.75),
               // Fraction.FromDoubleRounded(0.9),
                Fraction.FromDoubleRounded(0.95),
            };

            var bugTest = new List<Fraction>() {
                8,
                9
            };
            var plotData = testSet.ToDictionary(fraction => fraction, fraction => new List<PlotFrame>());

            testSet.ForEach(
                parallelPart =>
                {
                    for (Fraction i = minProcessors; i < maxProcessors;) {
                    //bugTest.ForEach( 
                    //       i =>
                    //    {
                            var processors = i.ToInt32();
                            var simulation = new SimulationControllerBase<AmdahlLaw>(
                                generator: () => new AmdahlLaw(40, processors, parallelPart),
                                strategy: SimulationStrategy.None
                            );
                            var stop = false;
                            simulation.OnPlaceUpdate(
                                simulation.state.TopGroup.Done,
                                list => { stop = true; }
                            );

                            Fraction calls = 0;
                            simulation.TopGroup.ParallelBalancedWorkers.ForEach(
                                t =>
                                {
                                    simulation.OnAfterTransitionFired(t, ()=>calls+=1);
                                } );

                            
                            while (!stop) {
                                simulation.SimulationStep();
                               // Console.WriteLine(simulation.state.TopGroup.DoneChecker.TimeScale + " || " + simulation.state.CurrentTime + " : " + simulation.Frame + " called PT times: " + calls + " tasks done: " + simulation.TopGroup.DoneTasks.GetMarks().Count);
                                //calls = 0;
                            }

                            Console.WriteLine(
                                $"processors: {processors} steps: {((simulation.state.CurrentTime - simulation.TopGroup.DoneChecker.TimeScale) / simulation.state.TimeStep).ToDouble()} time: {simulation.state.CurrentTime.ToDouble()} timeStep {simulation.state.TimeStep}"
                            );
                            plotData[parallelPart].Add(
                                new PlotFrame(processors, simulation.state.CurrentTime.ToDouble())
                            );
                           var oi = i.ToInt32();
                           i *= Fraction.FromDouble(1.05);
                           if (oi == i.ToInt32()) {
                               i += 1;
                           }
                    }
                    //);
                }
            );

            var i = 0;
            double[] xPositions = plotData.Values.First().Select(frame => Convert.ToDouble(++i)).ToArray();

            i = 0;
            double[] xprintPositions = plotData.Values.First().Select(frame =>
            {
                return frame.Key;
            }).ToArray();
            string[] xLabels = plotData.Values.First().Select(frame => frame.Key.ToString()).ToArray();

            var plt = new ScottPlot.Plot(600, 400);
            foreach (var kvp in plotData) {

                var ys = kvp.Value.Select(frame => frame.Value).ToArray();
                var esi = new ScottPlot.Statistics.Interpolation.EndSlopeSpline(xPositions, ys, resolution: 15);
                plt.PlotScatter(esi.interpolatedXs, esi.interpolatedYs, label: kvp.Key.ToString(), markerSize:0);
                plt.PlotScatter(xPositions, ys, label: kvp.Key.ToString(), markerSize: 10, lineWidth: 0);
            }
            plt.XTicks(xprintPositions, xLabels);
            plt.Legend();
            plt.Title("Amdahl's law");
            plt.YLabel("Time");
            plt.XLabel("Number of processors");
            plt.SaveFig("AmdahlTime.png");

            var plt2 = new ScottPlot.Plot(600, 400);
            foreach (var kvp in plotData) {
                var ys = kvp.Value.Select(frame => kvp.Value[0].Value / frame.Value).ToArray();
                var esi = new ScottPlot.Statistics.Interpolation.EndSlopeSpline(xPositions, ys, resolution: 15);
                plt2.PlotScatter(esi.interpolatedXs, esi.interpolatedYs, label: kvp.Key.ToString(), markerSize: 0);
                plt2.PlotScatter(xPositions, ys, label: kvp.Key.ToString(), markerSize: 10, lineWidth: 0);
            }
            plt2.XTicks(xPositions, xLabels);
            plt2.Legend();
            plt2.Title("Amdahl's law");
            plt2.YLabel("SpeedUp");
            plt2.XLabel("Number of processors");
            plt2.SaveFig("AmdahlSpeedUp.png");

        }
    }
}
