using System.Collections.Generic;
using System.Linq;
using Fractions;
using ServicesPetriNet.Core;
using ServicesPetriNet.Core.Transitions;

namespace ServicesPetriNet
{
    public class GustafsonLaw : Group
    {
        public Place Done;
        public Transition DoneChecker;
        public Place DoneTasks;

        public Place LinearTasks;
        public Transition LinearWorker;

        public Place ParallelTasks;
        public List<Transition> ParallelBalancedWorkers;

        public GustafsonLaw(float time, int processors,
            Fraction seialFraction)
        {
            var serialTime = Fraction.FromDoubleRounded(time) * seialFraction;
            var parallelTime = Fraction.FromDoubleRounded(time) - serialTime;
            var timeStep = Extensions.GreatestCommonDivisor(serialTime, parallelTime);
            var linearTasks = serialTime / timeStep;
            var parallelTasks = processors * (parallelTime/ timeStep);

            Extensions.InitListOfNodes(this, nameof(ParallelBalancedWorkers), processors);

            //Marks are representing tasks
            for (var i = 0; i < linearTasks; i++) Extensions.At(LinearTasks, MarkType.Create<Mark>());
            for (var i = 0; i < parallelTasks; i++) Extensions.At(ParallelTasks, MarkType.Create<Mark>());

            //After all is done we want to check that no tasks are left behind
            DoneChecker.Action<GenerateOne<Mark>>()
                .In<Mark>(LinearTasks, Link.Count.None)
                .In<Mark>(ParallelTasks, Link.Count.None)
                .Out<Mark>(Done);
            //And we make sure DoneChecker would wast as low time as possible
            DoneChecker.TimeScale = timeStep;

            LinearWorker.Action<OneToOne<Mark>>()
                .In<Mark>(LinearTasks)
                .Out<Mark>(DoneTasks);
            LinearWorker.TimeScale = timeStep;

            //Note that ParallelBalancedWorkers are empty when parallelTasks < processors
            for (var i = 0; i < processors; i++)
            {
                var cw = ParallelBalancedWorkers[i];

                //Await Linear tasks, run at one step of the time
                cw.Action<OneToOne<Mark>>()
                    .In<Mark>(LinearTasks, Link.Count.None)
                    .In<Mark>(ParallelTasks)
                    .Out<Mark>(DoneTasks);
                cw.TimeScale = timeStep;
            }

            Descriptor.Refresh();
        }
    }
}
