using System;
using System.Collections.Generic;
using System.Linq;
using Accord.Math.Random;
using ServicesPetriNet.Core.Attributes;

namespace ServicesPetriNet.Core
{
    public class SimulationController<TGroup> where TGroup : Group, new()
    {
        private int step;

        public Group TopGroup;

        public SimulationController()
        {
            TopGroup = new TGroup();
            TopGroup.SetGlobatTransitionTimeScales();

            var fat = TopGroup.Descriptor.SubGroups.Values
                .Traverse(g => g.Value.Descriptor.SubGroups.Values)
                .SelectMany(g => g.Value.Descriptor.Transitions)
                .Select(pair => pair.Value).ToList();
            fat.AddRange(TopGroup.Descriptor.Transitions.Values);

            Transitions = fat.OrderBy(
                    x =>
                    {
                        if (x.Attributes.Any(a => a is PrioretyAttribute)) {
                            var pa = (PrioretyAttribute) x.Attributes.First(a => a is PrioretyAttribute);
                            return pa.Priorety;
                        }

                        return 0;
                    }
                )
                .ToList();

            if (Transitions
                .Any(descriptor => !descriptor.Value.CheckActionFunctions()))
                throw new Exception("Bad Action routing detected");
        }

        public List<FieldDescriptor<Transition>> Transitions { get; set; }

        public void SimulationStep()
        {
            step += 1;
            var readyToAct = new List<TransitionStage>();
            foreach (var transition in Transitions) {
                var t = transition.Value;
                var time = step % t.TimeScale == 0;
                var avail = t.Check();
                //var which = t.DebugSource(TopGroup);
                if (time  && avail) {
                    if (transition.Attributes.Any(a => a is ProbabiletyAttribute) &&
                        transition.Attributes.First(a => a is ProbabiletyAttribute) is ProbabiletyAttribute pa) {
                        var pad = pa.Distribution as IRandomNumberGenerator<int>;
                        if (pad != null) {
                            if (pad.Generate() <= 0) continue;
                        } else {
                            throw new Exception("Bad ProbabiletyAttribute: IRandomNumberGenerator<int> is required");
                        }
                    }


                    readyToAct.Add(
                        new TransitionStage {
                            Transition = t,
                            Marks = t.Gather()
                        }
                    );
                }
            }

            foreach (var transition in readyToAct) {
                var t = transition.Transition;
                var ts = t.DebugSource(TopGroup);
                var results = t.Act(transition.Marks);
                t.Distribute(results);
            }

            MarksController.Marks.RemoveAll(mark => mark.Host == null || mark.Host is Transition);

        }

        private class TransitionStage
        {
            public Dictionary<Extensions.LinkKey, List<MarkType>> Marks;
            public Transition Transition;
        }
    }
}
