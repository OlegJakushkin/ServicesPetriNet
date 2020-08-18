using System.Collections.Generic;
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
}
