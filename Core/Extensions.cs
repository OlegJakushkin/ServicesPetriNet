using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ServicesPetriNet.Core;

namespace ServicesPetriNet
{
    public static class Extensions
    {
        #region GDI

        public static Dictionary<string, Place> GetAllPlaces<T>(T instance)
        {
            return GetAllTypesBasedOn<T, Place, Group>(instance);
        }

        public static Dictionary<string, Transition> GetAllTransitions<T>(T instance)
        {
            return GetAllTypesBasedOn<T, Transition, Group>(instance);
        }
        public static List<Type> GetAllMarkTypes<T>() { return GetAllInterfaceBasedTypes<IMarkType, T, Group>(); }
        
        public static Dictionary<string, Tbase> GetAllTypesBasedOn<Thost, Tbase, Tbottom>(Thost instance)
        {
            var t = typeof(Tbase);
            var allIntegerFields = new Dictionary<string, Tbase>();

            do {
                foreach (var Fi in typeof(Thost)
                    .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(fi => fi.FieldType.IsAssignableFrom(t) && fi.DeclaringType != typeof(Tbottom)).ToList()) {
                    allIntegerFields.Add(Fi.Name, (Tbase) Fi.GetValue(instance));
                }
            } while ((t = t.BaseType) != null || t != typeof(Tbottom));

            return allIntegerFields;
        }

        public static List<Type> GetAllInterfaceBasedTypes<Tinterface, Tbase, Tbottom>()
        {
            var t = typeof(Tbase);

            HashSet<Type> allIntegerFields = new HashSet<Type>();
            do {
                foreach (var Ti in t.GetNestedTypes(BindingFlags.DeclaredOnly | BindingFlags.Public)
                    .Where(ti => ti.GetInterfaces().Contains(typeof(Tinterface))).ToList()) {
                    allIntegerFields.Add(Ti);
                }
            } while ((t = t.BaseType) != null ||
                     t != typeof(Tbottom));

            return allIntegerFields.ToList();
        }
        #endregion GDI


        #region Transition

        public static Transition Action<T>(this Transition t)
        {
            t.Links = new List<Link>();
            t.Action = typeof(T);
            return t;
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

        #region Marks
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
        #endregion Marks

    }
}
