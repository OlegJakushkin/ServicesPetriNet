using System;
using System.Collections.Generic;
using System.Linq;
using static ServicesPetriNet.Extensions;

namespace ServicesPetriNet.Core {
    public class GroupDescriptor<T> : IGroupDescriptor
    {
        public GroupDescriptor(T instance)
        {
            Places = GetAllPlaces(instance);
            Transitions = GetAllTransitions(instance);
            Refresh();
        }

        public static List<Type> GetAllMarkTypes(List<MarkType> marks, List<Transition> transitions)
        {
            var result = marks.Select(m => m.GetType()).ToList();
            result.AddRange(transitions.SelectMany(t => t.Links.Select(link => link.What)));
            return result.Distinct().ToList();
        }

        public Dictionary<string, Place> Places { get; set; }
        public Dictionary<string, Transition> Transitions { get; set; }
        public List<Type> MarkTypes { get; set; }
        public List<MarkType> Marks { get; set; }

        public virtual void Refresh()
        {
            Marks = Places.SelectMany(p => p.Value.GetMarks()).ToList();
            MarkTypes = GetAllMarkTypes(Marks, Transitions.Values.ToList());
        }

    }
}
