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
        public static List<Type> GetAllMarkTypes<T>() { return GetAllInterfaceBasedTypes<IMarkType, T, Group>(); }
        
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
                .Where(fi => t.IsAssignableFrom(fi.FieldType) ).ToList()) {
                var n = Fi.Name;
                if (Fi.FieldType != typeof(Type)) {
                    var it = (Group) Fi.GetValue(instance);
                    if (it == null) {
                        var Tgroup = typeof(Group<>);
                        Fi.SetValue(instance, (Group) Activator.CreateInstance(Fi.FieldType));
                    }
                }
            }
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
            } while (t != typeof(Tbottom) && (t = t.BaseType) != null);

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
