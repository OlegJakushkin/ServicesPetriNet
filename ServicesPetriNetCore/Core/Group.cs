using System;
using System.Collections.Generic;
using System.Linq;
using ServicesPetriNet.Core.Attributes;
using static ServicesPetriNet.Extensions;

namespace ServicesPetriNet.Core {

    //Group is a basic unit of Places, Transitions and logic combination in SPN
    //Pattern is for taking parts of a group and expanding it with pre-made mechanics
    //If places or transitions need to be added they shall be created mnually
    public class Group
    {
        public IGroupDescriptor Descriptor { get; set; }

        public int TimeScale { get; set; } = 1;

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
            InitAllTypeInstances<Place>(this);
            InitAllTypeInstances<Transition>(this);
            InitAllTypeInstances<Group>(this);
            Descriptor = GroupDescriptor.CreateInstance(this);
            SetTimeScales();
        }

        public void SetGlobatTransitionTimeScales()
        {
            Action<Group> g = gr =>
            {
                foreach (var keyValuePair in gr.Descriptor.Transitions) {
                    keyValuePair.Value.Value.TimeScale = gr.TimeScale;
                }
            };

            Action<FieldDescriptor<Group>> a = null;
            a = descriptor =>
            {
                g(descriptor.Value);
                descriptor.Value.Descriptor.ApplyToAllSubGroups(a);
            };

            g(this);
        }


        private void SetTimeScales()
        {
            Descriptor.SubGroups.Where(
                    pair => pair.Value.Attributes.Any(attribute => attribute.GetType() == typeof(TimeScaleAttribute))
                )
                .ToList().ForEach(
                    pair =>
                    {
                        pair.Value.Attributes.Where(attribute => attribute.GetType() == typeof(TimeScaleAttribute))
                            .ToList().ForEach(
                                attr =>
                                {
                                    var atr = attr as TimeScaleAttribute;
                                    Action<FieldDescriptor<Group>> a = null;
                                    a = descriptor =>
                                    {
                                        descriptor.Value.TimeScale *= atr.Scale;
                                        descriptor.Value.Descriptor.ApplyToAllSubGroups(a);
                                    };
                                    a(pair.Value);
                                }
                            );
                    }
                );
        }
    }

}
