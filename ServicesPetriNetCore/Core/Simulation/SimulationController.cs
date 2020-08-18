using System;
using System.Collections.Generic;
using System.Linq;
using Accord.Math.Random;
using Fractions;
using ServicesPetriNet.Core.Attributes;

namespace ServicesPetriNet.Core
{
    public class SimulationController<TGroup> : SimulationControllerBase<TGroup> where TGroup : Group, new()
    {
        public SimulationController(bool load = false, string path = "./model.json")
            : base(load, path, () => new TGroup()) { }
    }

    public class SimulationControllerBase<TGroup> where TGroup : Group
    {
        private readonly Dictionary<Place, List<Action<List<MarkType>>>> _eventListners =
            new Dictionary<Place, List<Action<List<MarkType>>>>();

        public SimulationFrameController<State> Frames;

        public State state = new State();

        public SimulationControllerBase(bool load = false, string path = "./model.json", Func<TGroup> generator = null)
        {
            if (load) {
                Frames = new SimulationFrameController<State>(path);
                state = Frames.GetState();
            } else {
                if (generator != null) state.TopGroup = generator();
                else throw new Exception("Bad generator");

                state.RefreshMarks();
                state.TimeStep = state.TopGroup.SetGlobatTransitionTimeScales();

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

        public void OnUpdate(Place key, Action<List<MarkType>> action)
        {
            List<Action<List<MarkType>>> listners = null;
            if (_eventListners.TryGetValue(key, out listners)) {
                listners.Add(action);
            } else {
                listners = new List<Action<List<MarkType>>> {
                    action
                };
                _eventListners.Add(key, listners);
            }
        }

        public void Save() { Frames.Save(); }

        public void SimulationStep()
        {
            state.CurrentTime += state.TimeStep;
            var readyToAct = new List<TransitionStage>();
            foreach (var transition in Transitions) {
                var t = transition.Value;
                var time = state.CurrentTime % t.TimeScale == 0;
                if (time) {
                    var avail = t.Check();
                    //var which = t.DebugSource(TopGroup);
                    if (avail) {
                        if (transition.Attributes.Any(a => a is ProbabiletyAttribute) &&
                            transition.Attributes.First(a => a is ProbabiletyAttribute) is ProbabiletyAttribute pa) {
                            var pad = pa.Distribution as IRandomNumberGenerator<int>;
                            if (pad != null) {
                                if (pad.Generate() <= 0) continue;
                            } else {
                                throw new Exception(
                                    "Bad ProbabiletyAttribute: IRandomNumberGenerator<int> is required"
                                );
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
            }

            foreach (var transition in readyToAct) {
                var t = transition.Transition;
                var ts = t.DebugSource(state.TopGroup);
                var results = t.Act(transition.Marks);
                var added = t.Distribute(results);
                foreach (var kvp in added)
                    if (_eventListners.TryGetValue(kvp.Key, out var listeners))
                        foreach (var listener in listeners)
                            listener(kvp.Value);
            }

            MarksController.Marks.RemoveAll(mark => mark.Host == null || mark.Host is Transition);
            state.RefreshMarks();
            Frames.SaveState(state);
        }

        public class State
        {
            public Fraction CurrentTime;
            public List<MarkType> Marks = new List<MarkType>();
            public Fraction TimeStep;
            public TGroup TopGroup;

            public void RefreshMarks()
            {
                var ms = TopGroup.Descriptor.DebugGetMarksTree();
                Marks = new List<MarkType>();
                foreach (KeyValuePair<string, object> kvp in ms) Marks.AddRange((List<MarkType>) kvp.Value);
            }
        }

        private class TransitionStage
        {
            public Dictionary<Extensions.LinkKey, List<MarkType>> Marks;
            public Transition Transition;
        }
    }
}
