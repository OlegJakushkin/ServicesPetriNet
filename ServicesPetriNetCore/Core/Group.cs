using System;
using System.Collections.Generic;
using System.Linq;
using static ServicesPetriNet.Extensions;

namespace ServicesPetriNet.Core {
    public class Group
    {
        public IGroupDescriptor GroupDescriptor { get; set; }
        public Type Type;

        public List<MarkType> Marks
        {
            get => GroupDescriptor.Marks;
            set => GroupDescriptor.Refresh();
        }
    }

    public class Group<T> : Group where T : class
    {
        public Group()
        {
            Type = typeof(T);
            InitAllGroupTypeInstances(this as T);
            GroupDescriptor = new GroupDescriptor<T>(this as T);
        }
    }
}
