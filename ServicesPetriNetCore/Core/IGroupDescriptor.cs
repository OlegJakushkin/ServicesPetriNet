using System;
using System.Collections.Generic;

namespace ServicesPetriNet.Core
{
    public interface IGroupDescriptor
    {
        Dictionary<string, Place> Places { get; set; }

        Dictionary<string, Transition> Transitions { get; set; }

        List<Type> MarkTypes { get; set; }
        List<MarkType> Marks { get; set; }
        void Refresh();
    }
}
