using System;
using static ServicesPetriNet.Extensions;

namespace ServicesPetriNet.Core {
    public class Group
    {
        public GroupDescriptor GroupDescriptor { get; set; }
        public Type Type;
    }

    public class Group<T> : Group where T : class
    {
        public Group()
        {
            Type = typeof(T);
            InitAllGroupTypeInstances(this as T);
        }
    }
}
