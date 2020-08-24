using System;
using System.Collections.Generic;
using System.Linq;
using Fractions;
using ScottPlot;
using ServicesPetriNet.Core;

namespace ServicesPetriNet
{
    public class AmdahlLawDemoProgram
    {
        public static void Main(int tasks = 20, int imgWidth=600, int imgHeight=400, string imgPath="./")
        {
            //Model test configurations
            var minProcessors = 1;
            var maxProcessors = 1025;
            var testSet = new List<Fraction>() {
                Fraction.FromDoubleRounded(0.5),
                Fraction.FromDoubleRounded(0.75),
                Fraction.FromDoubleRounded(0.9),
                Fraction.FromDoubleRounded(0.95),
            };

            //Running models
            var plotData = testSet.ToDictionary(fraction => fraction, fraction => new List<PlotFrame>());
            testSet.ForEach(
                parallelPart =>
                {
                    for (Fraction i = minProcessors; i < maxProcessors; i*=2) {
                        var processors = i.ToInt32();
                        var simulation = new SimulationControllerBase<AmdahlLaw>(
                            generator: () => new AmdahlLaw(tasks, processors, parallelPart),
                            strategy: SimulationStrategy.None
                        );
                        var stop = false;
                        simulation.OnPlaceUpdate(
                            simulation.state.TopGroup.Done,
                            list => { stop = true; }
                        );

                        while (!stop) {
                            simulation.SimulationStep();
                        }

                        //Logging results

                        Console.WriteLine(
                            $"processors: {processors} steps: {((simulation.state.CurrentTime - simulation.TopGroup.DoneChecker.TimeScale) / simulation.state.TimeStep).ToDouble()} time: {simulation.state.CurrentTime.ToDouble()} timeStep {simulation.state.TimeStep}"
                        );
                        plotData[parallelPart].Add(
                            new PlotFrame(processors, (simulation.state.CurrentTime - simulation.TopGroup.DoneChecker.TimeScale).ToDouble())
                        );
                    }
                }
            );

            //Plotting
            var i = 0;
            var xPositions = plotData.Values.First().Select(frame => Convert.ToDouble(++i)).ToArray();
            var xLabels = plotData.Values.First().Select(frame => frame.Key.ToString()).ToArray();

            var plt = new ScottPlot.Plot(imgWidth, imgHeight);
            foreach (var kvp in plotData) {
                var ys = kvp.Value.Select(frame => frame.Value).ToArray();
                var esi = new ScottPlot.Statistics.Interpolation.EndSlopeSpline(xPositions, ys, resolution: 15);
                plt.PlotScatter(esi.interpolatedXs, esi.interpolatedYs,  markerSize:0, label: null);
                plt.PlotScatter(xPositions, ys, label: kvp.Key.ToString(), markerSize: 5, lineWidth: 0);
            }
            plt.XTicks(xPositions, xLabels);
            plt.PlotHLine(tasks, lineStyle: LineStyle.Dash);
            plt.PlotHLine(1, lineStyle: LineStyle.Dash);
            plt.Legend();
            plt.Title("Amdahl's law");
            plt.YLabel("Time");
            plt.XLabel("Number of processors");
            plt.SaveFig(imgPath + "AmdahlTime.png");

            var plt2 = new ScottPlot.Plot(imgWidth, imgHeight);
            foreach (var kvp in plotData) {
                var ys = kvp.Value.Select(frame => kvp.Value[0].Value / frame.Value).ToArray();
                var esi = new ScottPlot.Statistics.Interpolation.EndSlopeSpline(xPositions, ys, resolution: 15);
                plt2.PlotScatter(esi.interpolatedXs, esi.interpolatedYs,  markerSize: 0, label: null);
                plt2.PlotScatter(xPositions, ys, label: kvp.Key.ToString(), markerSize: 5, lineWidth: 0);
            }
            plt2.PlotHLine(1, lineStyle: LineStyle.Dash);
            plt2.PlotHLine(tasks, lineStyle: LineStyle.Dash);
            plt2.Legend();
            plt2.XTicks(xPositions, xLabels);
            plt2.Title("Amdahl's law");
            plt2.YLabel("SpeedUp");
            plt2.XLabel("Number of processors");
            plt2.SaveFig(imgPath + "AmdahlSpeedUp.png");

        }
    }
}
