using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Sutro.Core.Settings
{
    public class SettingsContainsReferenceTypeException : Exception
    {
        public SettingsContainsReferenceTypeException() : base()
        {
        }

        public SettingsContainsReferenceTypeException(string message) : base(message)
        {
        }

        public SettingsContainsReferenceTypeException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Base class for settings objects.
    /// </summary>
    /// <remarks>
    /// Allows bi-directional copying and cloning between parent and child classes, as well as sibling classes. This facilitates working with settings instances, but should be used with caution. The CopyValuesFrom and CloneAs methods use reflection to copy any public properties or fields that are present in both types. Reference types (except for string) must derive from Settings also, to allow recursive deep copying.
    /// </remarks>
    public static class SettingsPrototype
    {
        public static void CopyValuesFrom<S, T>(S subject, T other)
        {
            foreach (PropertyInfo prop_this in subject.GetType().GetProperties())
            {
                if (prop_this.CanWrite)
                {
                    PropertyInfo prop_other = null;
                    try
                    {
                        prop_other = other.GetType().GetProperty(prop_this.Name);
                    }
                    catch (AmbiguousMatchException)
                    {
                        prop_other = other.GetType().GetProperty(prop_this.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
                    }
                    if (prop_other != null && prop_other.GetValue(other) != null)
                    {
                        if (prop_this.PropertyType.IsEnum)
                        {
                            if (prop_this.PropertyType == prop_other.PropertyType)
                            {
                                prop_this.SetValue(subject, prop_other.GetValue(other));
                            }
                        }
                        else
                        {
                            if (prop_this.GetValue(subject) == null)
                            {
                                var a = Activator.CreateInstance(prop_this.PropertyType);
                                prop_this.SetValue(subject, a);
                            }
                            prop_this.SetValue(subject, CopyValue(prop_other.GetValue(other)));
                        }
                    }
                }
            }

            foreach (FieldInfo field_this in subject.GetType().GetFields())
            {
                FieldInfo field_other = null;
                try
                {
                    field_other = other.GetType().GetField(field_this.Name);
                }
                catch (AmbiguousMatchException)
                {
                    field_other = other.GetType().GetField(field_this.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
                }

                if (field_other != null)
                {
                    if (field_this.FieldType.IsEnum)
                    {
                        if (field_this.FieldType == field_other.FieldType)
                        {
                            field_this.SetValue(subject, field_other.GetValue(other));
                        }
                    }
                    else
                    {
                        field_this.SetValue(subject, CopyValue(field_other.GetValue(other)));
                    }
                }
            }
        }

        private static object CopyValue(object v)
        {
            if (v is null)
                return null;

            var type = v.GetType();
            if (type.IsValueType)
            {
                return v;
            }
            else
            {
                if (type == typeof(string))
                {
                    return v;
                }
                else if (type.IsArray)
                {
                    return ((Array)v).Clone();
                }
                else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                {
                    var listType = typeof(List<>);
                    var t = type.GetGenericArguments();
                    var constructedListType = listType.MakeGenericType(t[0]);
                    var instance = Activator.CreateInstance(constructedListType);
                    foreach (var item in (IEnumerable)v)
                    {
                        var a = item;
                        instance.GetType().GetMethod("Add").Invoke(instance, new[] { CopyValue(item) });
                    }
                    return instance;
                }
                else if (v is object v_typed)
                {
                    var instance = Activator.CreateInstance(type);
                    CopyValuesFrom(instance, v_typed);
                    return instance;
                }
                else
                {
                    throw new SettingsContainsReferenceTypeException(
                        $"All reference types in classes derived from Settings should also inherit from Settings to " +
                        $"allow recursive deep copying. Type {type} was found on a public property or field; " +
                        $"to resolve, make {type} inherit from abstract base class Settings");
                }
            }
        }

        public static TClone CloneAs<TClone, TInput>(TInput subject) where TClone : new()
        {
            var clone = new TClone();
            CopyValuesFrom(clone, subject);
            return clone;
        }
    }
}