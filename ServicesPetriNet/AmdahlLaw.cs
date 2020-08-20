using System.Collections.Generic;
using System.Linq;
using Fractions;
using ServicesPetriNet.Core;
using ServicesPetriNet.Core.Transitions;

namespace ServicesPetriNet
{
    public class Mark : MarkType { }

    public class AmdahlLaw : Group
    {
        public Place Done;
        public Transition DoneChecker;
        public Place DoneTasks;

        public Place LinearTasks;
        public Transition LinearWorker;

        public Place ParallelBalancedTasks;
        public Place SquizeParallelTasks;
        public List<Transition> SquizeParallelWorkers;
        public List<Transition> ParallelBalancedWorkers;

        public AmdahlLaw(int tasks, int processors,
            Fraction parallelPart)
        {
            //There are two kinds of tasks: linear and parallel
            var parallelTasks = (tasks * parallelPart).ToInt32();
            var linearTasks = tasks - parallelTasks;

            // From parallel tasks some are balanced between CPUs and have same time scale as linear tasks, while some tasks are speed up by spreading between nodes
            var squizeTasks = parallelTasks % processors;
            var parallelBalancedTasks = parallelTasks - squizeTasks;


            //There are #processors represented via Workers, and one worker for linear tasks
            Extensions.InitListOfNodes(this, nameof(ParallelBalancedWorkers), processors);
            Extensions.InitListOfNodes(this, nameof(SquizeParallelWorkers), processors);

            //Marks are representing tasks
            for (var i = 0; i < linearTasks; i++) Extensions.At(LinearTasks, MarkType.Create<Mark>());
            for (var i = 0; i < parallelBalancedTasks; i++) Extensions.At(ParallelBalancedTasks, MarkType.Create<Mark>());
            for (var i = 0; i < squizeTasks; i++) Extensions.At(SquizeParallelTasks, MarkType.Create<Mark>());

            //After all is done we want to check that no tasks are left behind
            DoneChecker.Action<GenerateOne<Mark>>()
                .In<Mark>(LinearTasks, Link.Count.None)
                .In<Mark>(ParallelBalancedTasks, Link.Count.None)
                .In<Mark>(SquizeParallelTasks, Link.Count.None)
                .Out<Mark>(Done);
            //And we make sure DoneChecker would wast as low time as possible
            DoneChecker.TimeScale = new Fraction(1, processors);

            LinearWorker.Action<OneToOne<Mark>>()
                .In<Mark>(LinearTasks)
                .Out<Mark>(DoneTasks);

            //Note that ParallelBalancedWorkers are empty when parallelTasks < processors
            for (var i = 0; i < processors; i++) {
                var cw = ParallelBalancedWorkers[i];

                //Await Linear tasks, run at one step of the time
                cw.Action<OneToOne<Mark>>()
                    .In<Mark>(LinearTasks, Link.Count.None)
                    .In<Mark>(ParallelBalancedTasks)
                    .Out<Mark>(DoneTasks);
            }

            // Fire squize tasks when balanced and linear tasks are done
            var squizeTimeScale = new Fraction(squizeTasks, processors)
                                  + LinearWorker.TimeScale * linearTasks
                                  + ParallelBalancedWorkers.First().TimeScale * (parallelBalancedTasks/processors);

            for (var i = 0; i < processors; i++)
            {
                var cw = SquizeParallelWorkers[i];

                //Await Linear and Equally Balanced tasks, run at reduced time step
                cw.Action<OneToOne<Mark>>()
                    .In<Mark>(LinearTasks, Link.Count.None)
                    .In<Mark>(ParallelBalancedTasks, Link.Count.None)
                    .In<Mark>(SquizeParallelTasks)
                    .Out<Mark>(DoneTasks);

                    cw.TimeScale = squizeTimeScale;
            }
            Descriptor.Refresh();
            var s = Descriptor.DebugGetMarksTree();
        }
    }
}
