using UnityEngine;
using UnityEditor;
//using UnityEditor.Compilation;
using System;
using System.Linq;
using System.Threading;
using System.Runtime;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using ACSL;
using ACSL.Interaction;
using ACSL.Network;

namespace ACSL.Utility
{
    /// <summary>
    /// A static class that holds generic utility methods.
    /// Adam Brown - Last Updated: 1/13/2019
    /// </summary>
    public static class GenericUtils
    {
        #region METHODS

        /// <summary>
        /// Strips all components from a GameObject. Has functionality to omit
        /// certain types of components
        /// Adam Brown - 12/27/2018
        /// </summary>
        /// <param name="components"></param>
        /// <param name="omitList"></param>
        public static void DestroyAllComponents(GameObject root, Type[] omitList)
        {
            foreach(Component c in root.GetComponentsInChildren<Component>(true))
            {
                bool delete = true;
                foreach(Type t in omitList)
                {
                    delete = c.GetType() != t && c.gameObject.CanDestroy(c.GetType());

                    if (!delete)
                        break;
                }
                if (delete)
                    Component.Destroy(c);
            }
        }

        /// <summary>
        /// Function to work in conjunction with CanDestroy. Checks if a component
        /// has a RequiresComponent attribute.
        /// Adam Brown - 12/27/2018
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="requirement"></param>
        /// <returns></returns>
        private static bool Requires(Type obj, Type requirement)
        {
            return Attribute.IsDefined(obj, typeof(RequireComponent)) &&
                   Attribute.GetCustomAttributes(obj, typeof(RequireComponent)).OfType<RequireComponent>()
                   .Any(rc => rc.m_Type0.IsAssignableFrom(requirement));
        }

        /// <summary>
        /// Function to work in conjunctions with Requires. Checks if a component 
        /// can be destroyed.
        /// Adam Brown - 12/27/2018
        /// </summary>
        /// <param name="go"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        static bool CanDestroy(this GameObject go, Type t)
        {
            return !go.GetComponents<Component>().Any(c => Requires(c.GetType(), t));
        }

        /// <summary>
        /// Copies a component from one GameObject to another.
        /// Adam Brown 11/18/2018
        /// </summary>
        /// <param name="original">The component to copy</param>
        /// <param name="destination">The target GameObject.</param>
        /// <returns></returns>
        public static Component CopyComponent(Component original, GameObject destination)
        {
            Type type = original.GetType();
            Component copy = destination.AddComponent(type);

            FieldInfo[] fields = type.GetFields();
            foreach (FieldInfo field in fields)
                field.SetValue(copy, field.GetValue(original));
            return copy;
        }

        /// <summary>
        /// Creates an AABB that surrounds all meshes within a target GameObject
        /// Adam Brown 11/18/2018
        /// </summary>
        /// <param name="o">The target GameObject.</param>
        /// <returns></retuns>
        public static Bounds FindBoundsOfMesh(GameObject o)
        {
            Bounds b = new Bounds(Vector3.zero, Vector3.zero);

            foreach(MeshFilter mf in o.GetComponentsInChildren<MeshFilter>())
                b.Encapsulate(mf.mesh.bounds);

            return b;
        }

        /// <summary>
        /// Returns whether or not an object was found in a delegate list
        /// provided by an event.
        /// Adam Brown 12/15/2018
        /// </summary>
        /// <param name="delegateList"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool CheckIfSubscribed(Delegate[] delegateList, UnityEngine.Object obj)
        {
            if (delegateList == null)
                return false;

            foreach(Delegate d in delegateList)
                if((d.Target as UnityEngine.Object).name == obj.name)
                    return true;

            return false;
        }

        /// <summary>
        /// Returns an array of all child classes of a type, if any.
        /// Adam Brown, 12/15/2018
        /// </summary>
        /// <param name="parentType"></param>
        /// <returns></returns>
        public static Type[] GetChildClasses(Type parentType)
        {
            List <Type> subclasses = new List<Type>();

            //get the assembly for the parent type
            Assembly[] assemblies = Thread.GetDomain().GetAssemblies();

            foreach (System.Reflection.Assembly a in assemblies)
                subclasses.AddRange(a.GetTypes());

            return subclasses.Where(t => t.IsSubclassOf(parentType)).ToArray();
        }

        #endregion
    }
}
