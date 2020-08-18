using System;
using System.Collections.Generic;
using Fractions;
using ServicesPetriNet.Core;
using ServicesPetriNet.Core.Transitions;

namespace ServicesPetriNet
{
    public class AmdahlLaw : Group
    {
        public Place Done;
        public Transition DoneChecker;
        public Place DoneTasks;

        public Place LinearTasks;

        public Transition LinearWorker;
        public Place ParallelTasks;
        public List<Transition> ParallelWorkers;

        public AmdahlLaw(int tasks, int processors,
            Fraction parallelPart)
        {
            var parallelTasks = (tasks * parallelPart).ToInt32();
            var linearTasks = tasks - parallelTasks;
            var timeScale = new Fraction(1);
            if (parallelTasks < processors) timeScale = new Fraction(parallelTasks, processors);
            Extensions.InitListOfNodes(this, nameof(ParallelWorkers), processors);

            for (var i = 0; i < parallelTasks; i++) Extensions.At(ParallelTasks, MarkType.Create<Mark>());

            for (var i = 0; i < linearTasks; i++) Extensions.At(LinearTasks, MarkType.Create<Mark>());

            DoneChecker.Action<GenerateOne<Mark>>()
                .In<Mark>(LinearTasks, Link.Count.None)
                .In<Mark>(ParallelTasks, Link.Count.None)
                .Out<Mark>(Done);
            DoneChecker.TimeScale = timeScale;

            LinearWorker.Action<OneToOne<Mark>>()
                .In<Mark>(LinearTasks)
                .Out<Mark>(DoneTasks);

            for (var i = 0; i < processors; i++) {
                var cw = ParallelWorkers[i];

                cw.Action<OneToOne<Mark>>()
                    .In<Mark>(LinearTasks, Link.Count.None)
                    .In<Mark>(ParallelTasks)
                    .Out<Mark>(DoneTasks);

                if (parallelTasks < processors) cw.TimeScale = timeScale;
            }

            Descriptor.Refresh();
        }
    }


    public class Mark : MarkType { }


    internal class Program
    {
        private static void Main(string[] args)
        {
            var maxProcessors = 40;
            var pp = Fraction.FromDoubleRounded(0.5);
            for (var i = 21; i < maxProcessors; i++) {
                var simulation = new SimulationControllerBase<AmdahlLaw>(generator: () => new AmdahlLaw(20, i, pp));
                var stop = false;
                simulation.OnUpdate(
                    simulation.state.TopGroup.Done,
                    list =>
                    {
                        var s = simulation.state.TopGroup.Descriptor.DebugGetMarksTree();
                        Console.WriteLine(s);
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
