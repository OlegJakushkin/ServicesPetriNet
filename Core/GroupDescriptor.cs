using System;
using System.Collections.Generic;
using static ServicesPetriNet.Extensions;

namespace ServicesPetriNet.Core {
    public class GroupDescriptor
    {
        public Dictionary<string, Place> Places { get; set; }

        public Dictionary<string, Transition> Transitions { get; set; }

        public List<Type> MarkTypes { get; set; }
        public List<MarkType> Marks { get; set; }
    }

    public class GroupDescriptor<T> : GroupDescriptor
    {
        public GroupDescriptor(T instance)
        {
            Places = GetAllPlaces(instance);
            Transitions = GetAllTransitions(instance);

            MarkTypes = GetAllMarkTypes<T>();
        }
    }
}
