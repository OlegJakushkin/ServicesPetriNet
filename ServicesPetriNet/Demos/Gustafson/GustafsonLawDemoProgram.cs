using System;
using System.Collections.Generic;
using System.Linq;
using Fractions;
using ScottPlot;
using ServicesPetriNet.Core;

namespace ServicesPetriNet
{
    public class GustafsonLawDemoProgram
    {
        public static void Main( float time = 20, int imgWidth = 600, int imgHeight = 400, string imgPath = "./")
        {
            //Model test configurations
            var minProcessors = 1;
            var maxProcessors = 121;
            var seialFraction = new List<Fraction>() {
                Fraction.FromDoubleRounded(0.1),
                Fraction.FromDoubleRounded(0.2),
                Fraction.FromDoubleRounded(0.3),
                Fraction.FromDoubleRounded(0.4),
                Fraction.FromDoubleRounded(0.5),
                Fraction.FromDoubleRounded(0.6),
                Fraction.FromDoubleRounded(0.7),
                Fraction.FromDoubleRounded(0.8),
                Fraction.FromDoubleRounded(0.9),
            };

            //Running models
            var plotData = seialFraction.ToDictionary(fraction => fraction, fraction => new List<PlotFrame>());
            seialFraction.ForEach(
                parallelPart =>
                {
                    for (Fraction i = minProcessors; i < maxProcessors; )
                    {
                        var processors = i.ToInt32();
                        var simulation = new SimulationControllerBase<GustafsonLaw>(
                            generator: () => new GustafsonLaw(time, processors, parallelPart), 
                            strategy: SimulationStrategy.None
                        );
                        var stop = false;
                        simulation.OnPlaceUpdate(
                            simulation.state.TopGroup.Done,
                            list => { stop = true; }
                        );

                        while (!stop)
                        {
                            simulation.SimulationStep();
                        }

                        //Logging results

                        Console.WriteLine(
                            $"preformed tasks: {simulation.TopGroup.DoneTasks.GetMarks().Count} on processors: {processors} steps: {((simulation.state.CurrentTime - simulation.TopGroup.DoneChecker.TimeScale) / simulation.state.TimeStep).ToDouble()} time: {simulation.state.CurrentTime.ToDouble()} timeStep {simulation.state.TimeStep}"
                        );
                        plotData[parallelPart].Add(
                            new PlotFrame(processors, simulation.TopGroup.DoneTasks.GetMarks().Count)
                        );
                        i += 10;
                        if (i % 10 != 0) {
                            i -= 1;
                        }
                    }

                }
            );


            //Plotting
            var i = 0;
            var xPositions = plotData.Values.First().Select(frame => Convert.ToDouble(++i)).ToArray();
            var xLabels = plotData.Values.First().Select(frame => frame.Key.ToString()).ToArray();

            var plt2 = new ScottPlot.Plot(imgWidth, imgHeight);
            foreach (var kvp in plotData)
            {
                var ys = kvp.Value.Select(frame => frame.Value / kvp.Value[0].Value).ToArray();
                var esi = new ScottPlot.Statistics.Interpolation.NaturalSpline(xPositions, ys, resolution: 15);
                plt2.PlotScatter(esi.interpolatedXs, esi.interpolatedYs, markerSize: 0, label: null);
                plt2.PlotScatter(xPositions, ys, label: kvp.Key.ToString(), markerSize: 5, lineWidth: 0);
            }
            plt2.PlotHLine(1, lineStyle: LineStyle.Dash);
            plt2.Legend();
            plt2.XTicks(xPositions, xLabels);
            plt2.Title("Gustafson's law");
            plt2.YLabel("SpeedUp");
            plt2.XLabel("Number of processors");
            plt2.SaveFig(imgPath + "GustafsonSpeedUp.png");

        }
    }
}
