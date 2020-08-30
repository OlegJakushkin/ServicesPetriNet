using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dynamitey;
using Fractions;
using ServicesPetriNet.Core;

namespace ServicesPetriNet
{


     public static class Extensions
    {
        public static IEnumerable<T> Traverse<T>(this IEnumerable<T> items,
            Func<T, IEnumerable<T>> childSelector)
        {
            var stack = new Stack<T>(items);
            while (stack.Any()) {
                var next = stack.Pop();
                yield return next;
                foreach (var child in childSelector(next))
                    stack.Push(child);
            }
        }


        public static List<MarkType> GetMarks(this Place p) { return MarksController.GetPlaceMarks(p); }

        public static Fraction GreatestCommonDivisor(this IEnumerable<Fraction> numbers)
        {
            return numbers.Aggregate(GreatestCommonDivisor);
        }

        public static Fraction GreatestCommonDivisor(Fraction a, Fraction b)
        {
            //Calculate the Greatest Common Divisor of a and b.
            //Unless b == 0, the result will have the same sign as b(so that when
            //b is divided by it, the result comes out positive).

            while (b != 0) {
                var ta = new Fraction(b.Numerator, b.Denominator);
                b = a % b;
                a = ta;
            }

            return a;
        }

        public static object ReflectionCast<T>(this IEnumerable<T> value, Type elementType)
        {
            var castMethod = typeof(Enumerable).GetMethod(
                "Cast",
                BindingFlags.Static | BindingFlags.Public
            );
            var castGenericMethod = castMethod.MakeGenericMethod(elementType);
            var toListMethod = typeof(Enumerable).GetMethod(
                "ToList",
                BindingFlags.Static | BindingFlags.Public
            );
            var toListGenericMethod = toListMethod.MakeGenericMethod(elementType);

            var eo = castGenericMethod.Invoke(null, new object[] {value});
            var lo = toListGenericMethod.Invoke(null, new[] {eo});
            return lo;
        }

        public static List<E> ReflectionCast<T, E>(this IEnumerable<T> value)
        {
            var elementType = typeof(E);
            return (List<E>) value.ReflectionCast(elementType);
        }

        #region GDI

        public static Dictionary<string, FieldDescriptor<Place>> GetAllPlaces(Group instance)
        {
            return GetAllTypeInstancesBasedOn<Place>(instance);
        }

        public static Dictionary<string, FieldDescriptor<Group>> GetAllGroups(Group instance)
        {
            return GetAllTypeInstancesBasedOn<Group>(instance);
        }

        public static Dictionary<string, FieldDescriptor<Transition>> GetAllTransitions(Group instance)
        {
            return GetAllTypeInstancesBasedOn<Transition>(instance);
        }

        public static Dictionary<string, FieldDescriptor<Tbase>> GetAllTypeInstancesBasedOn<Tbase>(Group instance)
        {
            var t = typeof(Tbase);
            var allFields = new Dictionary<string, FieldDescriptor<Tbase>>();
            var Thost = instance.GetType();
            foreach (var Fi in Thost
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(fi => t.IsAssignableFrom(fi.FieldType)).ToList())
                allFields.Add(
                    Fi.Name,
                    new FieldDescriptor<Tbase> {
                        Value = (Tbase) Fi.GetValue(instance),
                        Attributes = Fi.GetCustomAttributes(typeof(Attribute), true).Cast<Attribute>().ToList()
                    }
                );

            t = typeof(List<Tbase>);

            foreach (var Fi in Thost
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(fi => fi.FieldType.IsList()).ToList()) {
                var tp = Fi.FieldType.GenericTypeArguments.First();
                if (tp == typeof(Tbase)) {
                    var it = Fi.GetValue(instance);

                    var l = (List<Tbase>) Fi.GetValue(instance);
                    var iterator = 0;
                    if (l != null)
                        l.ForEach(
                            i => allFields.Add(
                                Fi.Name + "_" + iterator++,
                                new FieldDescriptor<Tbase> {
                                    Value = i,
                                    Attributes =
                                        Fi.GetCustomAttributes(typeof(Attribute), true).Cast<Attribute>().ToList()
                                }
                            )
                        );
                }
            }

            return allFields;
        }

        public static void InitAllTypeInstances<TChild>(object instance)
        {
            if (instance == null)
                return;
            var Thost = instance.GetType();
            var t = typeof(TChild);
            foreach (var Fi in Thost
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(fi => t.IsAssignableFrom(fi.FieldType)).ToList())
                if (Fi.FieldType != typeof(Type)) {
                    var it = (TChild)Fi.GetValue(instance);
                    if (it == null) {
                        if (typeof(INode).IsAssignableFrom(Fi.FieldType)) {
                            Fi.SetValue(instance, (TChild)CreateNode(instance, Fi.FieldType));
                        }
                        else {
                            Fi.SetValue(instance, (TChild) Activator.CreateInstance(Fi.FieldType));
                        }
                    }
                }
        }

        public static List<INode> InitListOfNodes(object host, string name, in int count)
        {
            if (host == null)
                throw new Exception("Empty host");
            var result = new List<INode>();

            var Thost = host.GetType();
            var Fi = Thost
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .First(fi => fi.Name == name);
            if (Fi.FieldType != typeof(Type) &&
                Fi.FieldType.IsList()) {
                var it = Fi.GetValue(host);
                var tp = Fi.FieldType.GenericTypeArguments.First();

                if (it == null) {
                    for (var i = 0; i < count; ++i) {
                        var o = CreateNode(host, tp);
                        result.Add(o);
                    }

                    Fi.SetValue(host, result.ReflectionCast(tp));
                }
            } else {
                throw new Exception("Field not found");
            }

            return result;
        }

        private static INode CreateNode(object host, Type tp)
        {
            var o = (INode) Activator.CreateInstance(tp);
            var ts = host.GetType();
            if (typeof(Group).IsAssignableFrom(ts)) {
                o.From = (Group) host;
            } else if (typeof(IGroupDescriptor).IsAssignableFrom(ts)) {
                o.From = ((IGroupDescriptor) host).Host;
            }
            else if (typeof(Pattern).IsAssignableFrom(ts))
            {
                o.From = ((Pattern)host).Host;
            }
            else {
                throw new Exception("Shall be called from Group, Pattern or Desctiptor!");
            }

            return o;
        }

        public static INode InitSingleNode(object host, string name)
        {
            if (host == null)
                throw new Exception("Empty host");

            var Thost = host.GetType();
            var Fi = Thost
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .First(fi => fi.Name == name);
            if (Fi.FieldType != typeof(Type)) {
                var it = Fi.GetValue(host);
                if (it == null) {
                    var o = CreateNode(host, Fi.FieldType);
                    Fi.SetValue(host, o);
                    return o;
                }

                return it as INode;
            }

            throw new Exception("Field not found");
        }

        #endregion GDI


        #region Transition

        public static bool Decompose(this Group At, IMarkType whom, List<IPart> into )
        {
            var result = whom.Decompose(@into, At);
            return result;
        }

        public static Transition Action<T>(this Transition t) where T : IAction
        {
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

        public static bool IsList(this Type o) { return o.FullName.StartsWith("System.Collections.Generic.List"); }

        //Zero check action type arguments
        public static bool CheckActionFunctions(this Transition t)
        {
            var actor = Dynamic.InvokeConstructor(t.Action);
            var action = (IAction) actor;
            action.Host = t;

            var method = t.Action.GetMethod(nameof(Action));

            var paramsToCheck = method.GetParameters().Where(p => !p.IsOut).Select(p => p.ParameterType).ToList();
            var listParams = paramsToCheck.Where(p => p.IsList()).ToList();
            if (listParams
                .Select(p => p.GenericTypeArguments.First())
                .Any(p => paramsToCheck.Contains(p))
            )
                throw new Exception(
                    "Error: list and seprate param. You can use (Tin1,..., Tin1) as Action  arguments or a single List<Tin1> as argument," +
                    " note that order is not guaranteed so " +
                    "(Tin1 A, Tin1 B) shall mean the same as (Tin1 B, Tin1 A) for your action"
                );

            if (listParams.GroupBy(n => n).Any(c => c.Count() > 1))
                throw new Exception(
                    "Error: Two+ of same type lists in Action. You can use (Tin1,..., Tin1) as Action  arguments or a single List<Tin1> as argument," +
                    " note that order is not guaranteed so " +
                    "(Tin1 A, Tin1 B) shall mean the same as (Tin1 B, Tin1 A) for your action"
                );

            return true;
        }

        //First check executability
        public static bool Check(this Transition t)
        {
            var result = t.Links
                .Where(l => l.From is Place)
                .All(link => link.Check());
            return result;
        }

        public class NodeDetails : IEquatable<NodeDetails>
        {
            public string ID;
            public string Name;
            public bool IsPatternMember;
            public Pattern PatternSource;
            public INode Host;

            public NodeDetails(INode o, Group g)
            {
                Host = o;

                var result = o.ToString();
                Action<Group> a = null;
                a = g =>
                {
                    var isT = g.Descriptor.Transitions.Any(pair => pair.Value.Value == o);
                    var isP = g.Descriptor.Places.Any(pair => pair.Value.Value == o);

                    if (isT) {
                        var tn = g.Descriptor.Transitions.First(pair => pair.Value.Value == o);
                        result = g.GetType().Name + "_" + tn.Key;
                        Name = tn.Key;
                    }
                    else if (isP){
                        var pn = g.Descriptor.Places.First(pair => pair.Value.Value == o);
                            result = g.GetType().Name + "_" + pn.Key;
                            Name = pn.Key;

                    }

                    if (isP || isT) {
                        var isInP = false;
                        Action<Pattern> act = null;
                            act = pattern =>
                        {
                            if(isInP) 
                                return;

                            pattern.ApplyToGeneratedNodes(
                                (s, node) =>
                                {
                                    if (node == o) {
                                        isInP = true;
                                        IsPatternMember = true;
                                        PatternSource = pattern;
                                    }
                                }
                            );
                            pattern.ApplyToNestedPatterns(pp=> act(pp));
                        };

                        g.Descriptor.Patterns.ForEach(pattern => act(pattern));

                    } else {
                        g.Descriptor.ApplyToAllSubGroups(descriptor => a(descriptor.Value));
                    }

                };
                a(g);
                ID =  result;
            }
            public override string ToString() { return ID; }
            public override int GetHashCode() {
                return (ID != null ? ID.GetHashCode() : 0);
            }

            public bool Equals(NodeDetails other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return ID == other.ID;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((NodeDetails) obj);
            }
        }
        public static NodeDetails DebugSource(this INode o, Group g)
        {
            return new NodeDetails(o, g);
        }

        public struct LinkKey : IEqualityComparer<LinkKey>
        {
            public Type Type;
            public string Name;

            public override bool Equals(object? obj)
            {
                var result = false;
                if (obj != null) {
                    var other = (LinkKey) obj;
                    result = other.Type == Type && other.Name == Name;
                }

                return result;
            }

            public bool Equals(LinkKey x, LinkKey y) { return x.Equals(y); }

            public int GetHashCode(LinkKey obj) { return Type.GetHashCode() + Name.GetHashCode(); }
        }

        //Second Move marks to transition
        public static Dictionary<LinkKey, List<MarkType>> Gather(this Transition t)
        {
            return t.Links
                .Where(l => l.From is Place)
                .Aggregate(
                    new Dictionary<LinkKey, List<MarkType>>(),
                    (accumulator, l) =>
                    {
                        var p = (Place) l.From;
                        var marks = new List<MarkType>();

                        var listType = typeof(List<>);
                        var constructedListType = listType.MakeGenericType(l.What);
                        var key = new LinkKey {
                            Name = l.ByTheNameOf,
                            Type = constructedListType
                        };
                        if (accumulator.TryGetValue(key, out var value) &&
                            value != null) marks = value;

                        if (l.CountStrategy == Link.Count.All) {
                            marks.AddRange(p.GetMarks().Where(m => m.GetType() == l.What).ToList());
                        } else if (l.CountStrategy == Link.Count.Some ||
                                   l.CountStrategy == Link.Count.One) {
                            var ms =
                                    p.GetMarks().Where(m => m.GetType() == l.What)
                                        .Take(l.CountStrategyAmmount)
                                        .ToList()
                                ;
                            marks.AddRange(ms);
                        }

                        marks.MoveMarksTo(t);

                        accumulator[key] = marks;
                        return accumulator;
                    }
                );
        }

        //Third Execute transition
        public static Dictionary<Type, List<MarkType>> Act(this Transition t, Dictionary<LinkKey, List<MarkType>> dict)
        {
            var actor = Dynamic.InvokeConstructor(t.Action);
            var action = (IAction)actor;
            action.Host = t;

            var method = t.Action.GetMethod(nameof(Action));


            var named = dict.Where((pair, i) => pair.Key.Name != "")
                .ToList();

            //Fill all named paramerters
            var ps = method.GetParameters().Select(
                param =>
                {
                    object result = param;
                    if (dict.Any(pair => pair.Key.Name == param.Name)) {
                        if (param.IsOut) throw new Exception("Link Name shall be of action input parameter ");
                        var k = dict.First(pair => pair.Key.Name == param.Name);

                        if (param.ParameterType.IsList()) {
                            result = k.Value;
                        } else {
                            result = k.Value.First();
                            k.Value.RemoveAt(0);
                        }

                        dict.Remove(k.Key);
                    }

                    return result;
                }
            ).ToArray();

            //Fill all other paramerters
            for (var index = 0; index < ps.Length; index++) {
                var o = ps[index];
                if (o is ParameterInfo) {
                    var param = (ParameterInfo) o;
                    if (param.IsOut) {
                        ps[index] = null;
                        continue;
                    }

                    if (param.ParameterType.IsList()) {
                        var key = new LinkKey {
                            Name = "",
                            Type = param.ParameterType
                        };
                        var value = dict.TryGetValue(key, out var pValue)
                            ? pValue
                            : throw new InvalidOperationException(
                                $"Parameter of type {param.ParameterType.Name} was not found"
                            );
                        var elementType = param.ParameterType.GenericTypeArguments.First();

                        var lo = value.ReflectionCast(elementType);
                        //var values = value.Select(x => Dynamic.InvokeConvert(x, elementType, true)).ToList();
                        ps[index] = lo;
                        dict.Remove(key);
                    } else {
                        var listType = typeof(List<>);
                        var constructedListType = listType.MakeGenericType(param.ParameterType);
                        var key = new LinkKey {
                            Name = "",
                            Type = constructedListType
                        };
                        var results = dict.TryGetValue(key, out var pValue)
                            ? pValue
                            : throw new InvalidOperationException(
                                $"Parameter of type {param.ParameterType.Name} was not found"
                            );
                        ps[index] = results.First();
                        results.RemoveAt(0);
                        if (results.Count == 0) dict.Remove(key);
                    }
                }
            }


            var rp = method.Invoke(actor, ps);

            var outs = new Dictionary<Type, List<MarkType>>();
            Action<object> release = variable =>
            {
                var marks = new List<MarkType>();
                var isArray = variable.GetType().IsList();
                Type type;
                if (isArray) type = (variable as IEnumerable).AsQueryable().ElementType;
                else type = variable.GetType();

                if (outs.TryGetValue(type, out var value) &&
                    value != null) marks = value;

                if (variable.GetType().IsList()) marks.AddRange((List<MarkType>) variable);
                else marks.Add((MarkType) variable);

                outs[type] = marks;
            };

            for (var index = 0; index < method.GetParameters().Length; index++) {
                var o = method.GetParameters()[index];
                if (o.IsOut) {
                    var variable = ps[index];
                    release(variable);
                }
            }

            if (method.ReturnType != typeof(void)) release(rp);

            return outs;
        }


        //Fourth move to outputs
        //Todo test multiple outs of same type
        public static Dictionary<Place, List<MarkType>> Distribute(this Transition t,
            Dictionary<Type, List<MarkType>> outs)
        {
            var result = new Dictionary<Place, List<MarkType>>();
            t.Links.Where(l => l.To.GetType() == typeof(Place)).ToList().ForEach(
                l =>
                {
                    var marks = new List<MarkType>();
                    if (outs.TryGetValue(l.What, out var value) &&
                        value != null) marks = value;
                    else throw new Exception("Transitions Action had to return Tout: " + l.What);
                    var key = (Place) l.To;

                    switch (l.CountStrategy) {
                        case Link.Count.One:
                        {
                            var m = marks.First();
                            m.Host = key;
                            var ms = MarksController.Marks;
                            marks.Remove(m);

                            if (result.TryGetValue(key, out var added)) {
                                added.Add(m);
                            } else {
                                added = new List<MarkType> {
                                    m
                                };
                                result.Add(key, added);
                            }

                            break;
                        }
                        case Link.Count.All:
                        {
                            marks.MoveMarksTo(key);

                            if (result.TryGetValue(key, out var added)) added.AddRange(marks);
                            else result.Add(key, marks);


                            marks.Clear();

                            break;
                        }
                        case Link.Count.Some:
                        {
                            var ms = marks.Take(l.CountStrategyAmmount).ToList();
                            ms.MoveMarksTo(l.To);

                            if (result.TryGetValue(key, out var added)) added.AddRange(marks);
                            else result.Add(key, marks);
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

            return result;
        }

        public static Transition In<T>(this Transition t, Place from, Link.Count howMany = Link.Count.One,
            string byName = "",
            int count = -1)
        {
            CheckNulls(t, from);

            t.Links.Add(new Link<T>(from, t, byName, howMany, count));
            return t;
        }

        public static void CheckNulls(Transition t, Place p)
        {
            if (p == null ||
                t == null) throw new Exception("traget and transition should be instantiated before configuration.");
        }

        public static Transition Out<T>(this Transition t, Place to, Link.Count howMany = Link.Count.One,
            int count = -1)
        {
            CheckNulls(t, to);

            t.Links.Add(new Link<T>(t, to, "", howMany, count));
            return t;
        }

        #endregion Transition

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

        public static List<IPart> AsParts<T>(this IEnumerable<T> list) where T : IMarkType
        {
            return list.Cast<IPart>().ToList();
        }

        #endregion Marks
    }
}
