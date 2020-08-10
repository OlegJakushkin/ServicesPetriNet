using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Accord.Math.Random;
using Newtonsoft.Json;
using ServicesPetriNet.Core.Attributes;

namespace ServicesPetriNet.Core
{
    public class SimulationController<TGroup> where TGroup : Group, new()
    {
        public SimulationFrameController<State> Frames;
        public class State
        {
            public List<MarkType> Marks = new List<MarkType>();
            public TGroup TopGroup;

            public void RefreshMarks()
            {
                var ms = TopGroup.Descriptor.DebugGetMarksTree();
                Marks = new List<MarkType>();
                foreach (KeyValuePair<string, object> kvp in ms)
                {
                    Marks.AddRange((List<MarkType>)kvp.Value);
                }
            }

            public int Step;
        }

        public State state = new State();

        public SimulationController(bool load = false, string path = "./model.json")
        {
            if (load) {
                Frames = new SimulationFrameController<State>(path, true);
                state = Frames.GetState();
            } else {
                state.TopGroup = new TGroup();
                state.RefreshMarks();
                state.TopGroup.SetGlobatTransitionTimeScales();

                var fat = state.TopGroup.Descriptor.SubGroups.Values
                    .Traverse(g => g.Value.Descriptor.SubGroups.Values)
                    .SelectMany(g => g.Value.Descriptor.Transitions)
                    .Select(pair => pair.Value).ToList();
                fat.AddRange(state.TopGroup.Descriptor.Transitions.Values);

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

                Frames = new SimulationFrameController<State>(path, false);
                Frames.SaveState(state);
            }
        }

        public List<FieldDescriptor<Transition>> Transitions { get; set; }

        public void Save() { Frames.Save(); }

        public void SimulationStep()
        {
            state.Step += 1;
            var readyToAct = new List<TransitionStage>();
            foreach (var transition in Transitions) {
                var t = transition.Value;
                var time = state.Step % t.TimeScale == 0;
                var avail = t.Check();
                //var which = t.DebugSource(TopGroup);
                if (time && avail) {
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
                var ts = t.DebugSource(state.TopGroup);
                var results = t.Act(transition.Marks);
                t.Distribute(results);
            }

            MarksController.Marks.RemoveAll(mark => mark.Host == null || mark.Host is Transition);
            state.RefreshMarks();
            Frames.SaveState(state);
        }

        private class TransitionStage
        {
            public Dictionary<Extensions.LinkKey, List<MarkType>> Marks;
            public Transition Transition;
        }
    }
}
