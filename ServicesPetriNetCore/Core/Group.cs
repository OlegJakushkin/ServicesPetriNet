using System;
using System.Collections.Generic;
using System.Linq;
using static ServicesPetriNet.Extensions;

namespace ServicesPetriNet.Core {
    public class Group
    {
        public IGroupDescriptor Descriptor { get; set; }
        public Type Type;

        public List<MarkType> Marks
        {
            get => Descriptor.Marks;
            set => Descriptor.Refresh();
        }
        public List<Pattern> Patterns
        {
            get => Descriptor.Patterns;
            set => Descriptor.Refresh();
        }

        public Group()
        {
            Type = this.GetType();
            InitAllTypeInstances<Place>(this);
            InitAllTypeInstances<Transition>(this);
            InitAllTypeInstances<Group>(this);

            Descriptor = GroupDescriptor.CreateInstance(this);
        }
    }
    //Group<T> is a basic unit of Places, Transitions and logic combination in SPN

    //Pattern is for taking parts of a group and expanding it with pre-made mechanics
    //If places or transitions need to be added they shall be created mnually
}
