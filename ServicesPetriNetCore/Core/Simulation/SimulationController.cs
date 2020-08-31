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
        public SimulationController(bool load = false, string path = "./model.json",
            SimulationStrategy strategy = SimulationStrategy.Plane)
            : base(load, path, () => new TGroup()) { }
    }

    public enum SimulationStrategy
    {
        None,
        Plane, // 70% of run time is spent on serialization
        Diffs // 50% of run time is spent on diff creation
    }

    public class SimulationControllerBase<TGroup> where TGroup : Group
    {
        private readonly Dictionary<Transition, List<Action>> _activationListners =
            new Dictionary<Transition, List<Action>>();

        private readonly Dictionary<Transition, List<Action>> _completeListners =
            new Dictionary<Transition, List<Action>>();

        private readonly Dictionary<Place, List<Action<List<MarkType>>>> _eventListners =
            new Dictionary<Place, List<Action<List<MarkType>>>>();

        private readonly IFrameController<State> Frames;

        public State state = new State();


        public SimulationControllerBase(bool load = false, string path = "./model.json", Func<TGroup> generator = null,
            SimulationStrategy strategy = SimulationStrategy.Plane)
        {
            if (load) {
                switch (strategy) {
                    case SimulationStrategy.None:
                    {
                        Frames = new SimulationNoMemoryFrameController<State>();
                        break;
                    }
                    case SimulationStrategy.Plane:
                    {
                        Frames = new SimulationPlaneFrameController<State>(path);
                        break;
                    }
                    case SimulationStrategy.Diffs:
                    {
                        Frames = new SimulationDiffFrameController<State>(path);
                        break;
                    }
                    default: throw new ArgumentOutOfRangeException(nameof(strategy), strategy, null);
                }


                state = Load();
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

                switch (strategy) {
                    case SimulationStrategy.None:
                    {
                        Frames = new SimulationNoMemoryFrameController<State>();

                        break;
                    }
                    case SimulationStrategy.Plane:
                    {
                        Frames = new SimulationPlaneFrameController<State>(path, false);

                        break;
                    }
                    case SimulationStrategy.Diffs:
                    {
                        Frames = new SimulationDiffFrameController<State>(path, false);
                        break;
                    }
                    default: throw new ArgumentOutOfRangeException(nameof(strategy), strategy, null);
                }

                Frames.SaveState(state);
            }
        }

        public TGroup TopGroup => state.TopGroup;
        public int Frame => (state.CurrentTime / state.TimeStep).ToInt32();

        public List<FieldDescriptor<Transition>> Transitions { get; set; }

        public void OnPlaceUpdate(Place key, Action<List<MarkType>> action)
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

        public void OnBeforeTransitionFired(Transition key, Action action)
        {
            List<Action> listners = null;
            if (_activationListners.TryGetValue(key, out listners)) {
                listners.Add(action);
            } else {
                listners = new List<Action> {
                    action
                };
                _activationListners.Add(key, listners);
            }
        }

        public void OnAfterTransitionFired(Transition key, Action action)
        {
            List<Action> listners = null;
            if (_activationListners.TryGetValue(key, out listners)) {
                listners.Add(action);
            } else {
                listners = new List<Action> {
                    action
                };
                _activationListners.Add(key, listners);
            }
        }

        public void Save() { Frames.Save(); }

        public State Load(int frame = -1)
        {
            var s = Frames.GetState(frame);
            s.CleanUpMarks();
            return s;
        }

        public void SimulationStep()
        {
            state.CurrentTime += state.TimeStep;
            var readyToAct = new List<TransitionStage>();
            //Check availabilety
            foreach (var transition in Transitions) {
                var t = transition.Value;
                var mod = state.CurrentTime % t.TimeScale;
                var time = mod == 0;
                if (time) {
                    var avail = t.Check();

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
                                Transition = t
                            }
                        );
                    }
                }
            }

            //Debug section start
            foreach (var kvp in readyToAct)
                if (_activationListners.TryGetValue(kvp.Transition, out var listeners))
                    foreach (var listener in listeners)
                        listener();
            //Debug section end

            //Grab marks
            var ToRemove = new List<TransitionStage>();
            foreach (var kvp in readyToAct)
                if (kvp.Transition.Check()) kvp.Marks = kvp.Transition.Gather();
                else ToRemove.Add(kvp);
            foreach (var transitionStage in ToRemove) readyToAct.Remove(transitionStage);

            //Act
            foreach (var transition in readyToAct) {
                var t = transition.Transition;
                var results = t.Act(transition.Marks);
                var added = t.Distribute(results);

                //Debug section start
                foreach (var kvp in readyToAct)
                    if (_completeListners.TryGetValue(kvp.Transition, out var listeners))
                        foreach (var listener in listeners)
                            listener();

                foreach (var kvp in added)
                    if (_eventListners.TryGetValue(kvp.Key, out var listeners))
                        foreach (var listener in listeners)
                            listener(kvp.Value);
                //Debug section end
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

            public void CleanUpMarks()
            {
                var ms = TopGroup.Descriptor.DebugGetMarksTree();

                foreach (KeyValuePair<string, object> kvp in ms) {
                    var l = (List<MarkType>) kvp.Value;
                    l.ForEach(
                        type =>
                        {
                            if (!Marks.Contains(type)) type.Host = null;
                        }
                    );
                }
            }
        }

        private class TransitionStage
        {
            public Dictionary<Extensions.LinkKey, List<MarkType>> Marks;
            public Transition Transition;
        }
    }
}
