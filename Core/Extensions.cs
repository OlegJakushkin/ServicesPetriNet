using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ServicesPetriNet.Core;

namespace ServicesPetriNet
{
    public static class Extensions
    {
        public static Dictionary<string, Place> GetAllPlaces<T>(T instance)
        {
            return GetAllTypesBasedOn<T, Place>(instance);
        }

        public static Dictionary<string, Transition> GetAllTransitions<T>(T instance)
        {
            return GetAllTypesBasedOn<T, Transition>(instance);
        }
        public static List<Type> GetAllMarkTypes<T>() { return GetAllInterfaceBasedTypes<T>(typeof(IMarkType)); }

        //Todo fill
        public static Dictionary<Enum, Node> NodesCatch;

        public class Link
        {
            public Node From;
            public Node To;
            public Type What { get; }

            public Count CountStrategy { get; }
            public int CountStrategyAmmount { get; }

            public enum Count { One, All, Some }

            public Link(Node @from, Node to, Type what, Count howMany, int count = -1)
            {
                From = @from;
                To = to;
                What = what;
                CountStrategy = howMany;
                CountStrategyAmmount = count;
            }
        }
        public class Link<T> : Link
        {
            public Link(Node @from, Node to, Count howMany = Count.One, int count = -1) : base(
                @from,
                to,
                typeof(T),
                howMany,
                count
            ) { }
        }

        public static Transition Action<T>(this Transition t)
        {
            t.Action = typeof(T);
            return t;
        }

        public static Transition In<T>(this Transition t, Place @from, Link.Count howMany = Link.Count.One,
            int count = -1)
        {
            t.Links.Add(new Link<T>((Node) @from, t, howMany, count));
            return t;
        }

        public static Transition Out<T>(this Transition t, Place to, Link.Count howMany = Link.Count.One,
            int count = -1)
        {
            t.Links.Add(new Link<T>(t, to, howMany, count));
            return t;
        }


        public static Dictionary<string, Tbase> GetAllTypesBasedOn<Thost, Tbase>(Thost instance)
        {
            var t = typeof(Tbase);
            var allIntegerFields = new Dictionary<string, Tbase>();

            foreach (var Fi in typeof(Thost)
                .GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(fi => fi.FieldType.IsAssignableFrom(t)).ToList()) {
                allIntegerFields.Add(Fi.Name, (Tbase) Fi.GetValue(instance));
            }

            return allIntegerFields;
        }

        public static List<Type> GetAllInterfaceBasedTypes<T>(Type t)
        {
            List<Type> allIntegerFields = new List<Type>();

            foreach (var Ti in typeof(T).GetNestedTypes(BindingFlags.DeclaredOnly | BindingFlags.Public)
                .Where(ti => ti.GetInterfaces().Contains(t)).ToList()) {
                allIntegerFields.Add(Ti);
            }

            return allIntegerFields;
        }

        public static List<MarkType> At(Place place, MarkType addMark)
        {
            return new List<MarkType> {
                addMark
            };
        }

        public static List<MarkType> At(this List<MarkType> arr, Place place, MarkType addMark)
        {
            arr.Add(addMark);
            return arr;
        }
    }
}
