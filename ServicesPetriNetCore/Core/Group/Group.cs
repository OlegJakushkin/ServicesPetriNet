using System;
using System.Collections.Generic;
using System.Linq;
using Fractions;
using ServicesPetriNet.Core.Attributes;
using static ServicesPetriNet.Extensions;

namespace ServicesPetriNet.Core
{
    //Group is a basic unit of Places, Transitions and logic combination in SPN
    //Pattern is for taking parts of a group and expanding it with pre-made mechanics
    //If places or transitions need to be added they shall be created mnually
    public class Group
    {
        public Group()
        {
            InitAllTypeInstances<Place>(this);
            InitAllTypeInstances<Transition>(this);
            InitAllTypeInstances<Group>(this);
            Descriptor = GroupDescriptor.CreateInstance(this);
            SetTimeScales();
        }

        public IGroupDescriptor Descriptor { get; set; }

        public Fraction TimeScale { get; set; } = 1;

        public List<MarkType> Marks { get => Descriptor.Marks; set => Descriptor.Refresh(); }

        public List<Pattern> Patterns { get => Descriptor.Patterns; set => Descriptor.Refresh(); }

        //Returns Fraction - minimal frame time span
        public Fraction SetGlobatTransitionTimeScales()
        {
            var timeScales = new List<Fraction>();
            Action<Group> g = gr =>
            {
                timeScales.Add(gr.TimeScale);
                foreach (var keyValuePair in gr.Descriptor.Transitions) {
                    keyValuePair.Value.Value.TimeScale *= gr.TimeScale;
                    timeScales.Add(keyValuePair.Value.Value.TimeScale);
                }
            };

            Action<FieldDescriptor<Group>> a = null;
            a = descriptor =>
            {
                g(descriptor.Value);
                descriptor.Value.Descriptor.ApplyToAllSubGroups(a);
            };

            g(this);

            return timeScales.GreatestCommonDivisor();
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
