using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dynamitey;
using ServicesPetriNet.Core;

namespace ServicesPetriNet
{
    public static class Extensions
    {
        #region GDI

        public static Dictionary<string, Place> GetAllPlaces<T>(T instance)
        {
            return GetAllTypeInstancesBasedOn<T, Place>(instance);
        }

        public static Dictionary<string, Group> GetAllGroups<T>(T instance)
        {
            return GetAllTypeInstancesBasedOn<T, Group>(instance);
        }

        public static Dictionary<string, Transition> GetAllTransitions<T>(T instance)
        {
            return GetAllTypeInstancesBasedOn<T, Transition>(instance);
        }



        public static Dictionary<string, Tbase> GetAllTypeInstancesBasedOn<Thost, Tbase>(Thost instance)
        {
            var t = typeof(Tbase);
            var allFields = new Dictionary<string, Tbase>();

            foreach (var Fi in typeof(Thost)
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(fi => t.IsAssignableFrom(fi.FieldType)).ToList()) {
                allFields.Add(Fi.Name, (Tbase) Fi.GetValue(instance));
            }

            return allFields;
        }

        public static void InitAllGroupTypeInstances<Thost>(Thost instance)
        {
            if (instance == null)
                return;

            var t = typeof(Group);
            foreach (var Fi in typeof(Thost)
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(fi => t.IsAssignableFrom(fi.FieldType)).ToList()) {
                var n = Fi.Name;
                if (Fi.FieldType != typeof(Type)) {
                    var it = (Group) Fi.GetValue(instance);
                    if (it == null) {
                        var Tgroup = typeof(Group<>);
                        Fi.SetValue(instance, (Group) Dynamic.InvokeConstructor(Fi.FieldType));
                    }
                }
            }
        }

        #endregion GDI

        #region Transition

        public static Transition Action<T>(this Transition t)
        {
            t.Links = new List<Link>();
            t.Action = typeof(T);

            return t;
        }

        //ToDo implement
        public static bool Check(this Link l)
        {
            var result = false;
            if (l.From is Place p) {
                var marks = p.GetMarks();
                switch (l.CountStrategy) {
                    case Link.Count.One:
                    {
                        result = marks.Count(m => m.GetType() == l.What) >= l.CountStrategyAmmount;
                        break;
                    }
                    case Link.Count.All:
                    {
                        result = true;
                        break;
                    }
                    case Link.Count.Some:
                    {
                        result = marks.Count(m => m.GetType() == l.What) >= l.CountStrategyAmmount;
                        break;
                    }
                    case Link.Count.None:
                    {
                        result = marks.All(m => m.GetType() != l.What);
                        break;
                    }
                    default: throw new ArgumentOutOfRangeException();
                }
            }

            return result;
        }

        public static bool IsList(this object o)
        {
            if (o == null) return false;
            return o is IList &&
                   o.GetType().IsGenericType &&
                   o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
        }

        //Zero check action type arguments
        public static bool CheckActionFunctions(this Transition t)
        {
            var actor = Dynamic.InvokeConstructor(t.Action);

            var method = t.Action.GetMethod(nameof(Action));

            var paramsToCheck = method.GetParameters().Where(p => !p.IsOut).Select(p => p.ParameterType);
            var listParams = paramsToCheck.Where(p => p.IsList());
            if (listParams
                .Select(p => (p as IEnumerable).AsQueryable().ElementType)
                .Any(p => paramsToCheck.Contains(p))
            ) {
                throw new Exception(
                    "Error: list and seprate param. You can use (Tin1,..., Tin1) as Action  arguments or a single List<Tin1> as argument," +
                    " note that order is not guaranteed so " +
                    "(Tin1 A, Tin1 B) shall mean the same as (Tin1 B, Tin1 A) for your action"
                );
            }

            if (listParams.GroupBy(n => n).Any(c => c.Count() > 1)) {
                throw new Exception(
                    "Error: Two+ of same type lists in Action. You can use (Tin1,..., Tin1) as Action  arguments or a single List<Tin1> as argument," +
                    " note that order is not guaranteed so " +
                    "(Tin1 A, Tin1 B) shall mean the same as (Tin1 B, Tin1 A) for your action"
                );
            }

            return true;
        }

        //First check executability
        public static bool Check(this Transition t)
        {
            return t.Links
                .Where(l => l.From is Place)
                .Any(link => !link.Check());
        }

        //Second Move marks to transition
        public static Dictionary<Type, List<MarkType>> Gather(this Transition t)
        {
            return t.Links
                .Where(l => l.From is Place)
                .Aggregate(
                    new Dictionary<Type, List<MarkType>>(),
                    (accumulator, l) =>
                    {
                        var p = (Place) l.From;
                        var marks = new List<MarkType>();

                        var listType = typeof(List<>);
                        var constructedListType = listType.MakeGenericType(l.What);

                        if (accumulator.TryGetValue(constructedListType, out var value) &&
                            value != null) {
                            marks = value;
                        }

                        if (l.CountStrategy == Link.Count.All) {
                            marks.AddRange(p.GetMarks().Where(m => m.GetType() == l.What).ToList());
                        } else if (l.CountStrategy == Link.Count.Some ||
                                   l.CountStrategy == Link.Count.One) {
                            marks.AddRange(
                                p.GetMarks().Where(m => m.GetType() == l.What).Take(l.CountStrategyAmmount).ToList()
                            );
                        }

                        marks.MoveMarksTo(t);

                        accumulator[l.What] = marks;
                        return accumulator;
                    }
                );
        }

        //Third Execute transition
        public static Dictionary<Type, List<MarkType>> Act(this Transition t, Dictionary<Type, List<MarkType>> dict)
        {
            var actor = Dynamic.InvokeConstructor(t.Action);

            var method = t.Action.GetMethod(nameof(Action));

            var parameters = method.GetParameters()
                .Select<ParameterInfo, object>(
                    param =>
                    {
                        if (param.IsOut) {
                            return null;
                        }

                        if (param.ParameterType.IsList()) {
                            return dict.TryGetValue(param.ParameterType, out var pValue)
                                ? pValue
                                : throw new InvalidOperationException(
                                    $"Parameter of type {param.ParameterType.Name} was not found"
                                );
                        } else {
                            var listType = typeof(List<>);
                            var constructedListType = listType.MakeGenericType(param.ParameterType);
                            var results = dict.TryGetValue(constructedListType, out var pValue)
                                ? pValue
                                : throw new InvalidOperationException(
                                    $"Parameter of type {param.ParameterType.Name} was not found"
                                );
                            var result = results.First();
                            results.RemoveAt(0);
                            return result;
                        }
                    }
                )
                .ToArray();
            var rp = method.Invoke(actor, parameters);

            var outs = new Dictionary<Type, List<MarkType>>();
            Action<object> act = (variable) =>
            {
                var marks = new List<MarkType>();
                var isArray = variable.IsList();
                Type type;
                if (isArray) {
                    type = (variable as IEnumerable).AsQueryable().ElementType;
                } else {
                    type = variable.GetType();
                }

                if (outs.TryGetValue(type, out var value) &&
                    value != null) {
                    marks = value;
                }

                if (variable.IsList()) {
                    marks.AddRange((List<MarkType>) variable);
                } else {
                    marks.Add((MarkType) variable);
                }

                outs[type] = marks;
            };

            for (var index = 0; index < method.GetParameters().Length; index++) {
                var o = method.GetParameters()[index];
                if (o.IsOut) {
                    var variable = parameters[index];
                    act(variable);
                }
            }

            if (method.ReturnType != typeof(void)) {
                act(rp);
            }

            return outs;
        }

        //Fourth move to outputs
        public static void Distribute(this Transition t, Dictionary<Type, List<MarkType>> outs)
        {
            t.Links.Where(l => l.To.GetType() == typeof(Place)).ToList().ForEach(
                l =>
                {
                    var marks = new List<MarkType>();
                    if (outs.TryGetValue(l.What, out var value) &&
                        value != null) {
                        marks = value;
                    } else {
                        throw new Exception("Transitions Action had to return Tout: " + l.What);
                    }

                    switch (l.CountStrategy) {
                        case Link.Count.One:
                        {
                            var m = marks.First();
                            marks.Remove(m);
                            m.Host = l.To;
                            break;
                        }
                        case Link.Count.All:
                        {
                            marks.MoveMarksTo(l.To);
                            marks.Clear();

                            break;
                        }
                        case Link.Count.Some:
                        {
                            var ms = marks.Take(l.CountStrategyAmmount).ToList();
                            ms.MoveMarksTo(l.To);
                            marks.RemoveAll(m => ms.Contains(m));
                            break;
                        }
                        case Link.Count.None:
                        {
                            throw new Exception("Error: A Transition link to outgoing place shall not be null");
                        }
                        default: throw new ArgumentOutOfRangeException();
                    }
                }
            );
        }

        public static Transition In<T>(this Transition t, Place @from, Link.Count howMany = Link.Count.One,
            int count = -1)
        {
            t.Links.Add(new Link<T>((INode) @from, t, howMany, count));
            return t;
        }

        public static Transition Out<T>(this Transition t, Place to, Link.Count howMany = Link.Count.One,
            int count = -1)
        {
            t.Links.Add(new Link<T>(t, to, howMany, count));
            return t;
        }

        #endregion Transition


        public static List<MarkType> GetMarks(this Place p) { return MarksController.GetPlaceMarks(p); }

        #region Marks

        public static List<MarkType> At(Place place, MarkType addMark)
        {
            addMark.Host = place;
            return new List<MarkType> {
                addMark
            };
        }

        public static List<MarkType> At(this List<MarkType> arr, Place place, MarkType addMark)
        {
            addMark.Host = place;
            arr.Add(addMark);
            return arr;
        }

        public static void MoveMarksTo(this IEnumerable<MarkType> marks, INode p)
        {
            marks.ToList().ForEach(m => m.Host = p);
        }

        #endregion Marks
    }
}
