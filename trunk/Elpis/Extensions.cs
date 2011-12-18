/*
 * Copyright 2012 - Adam Haile
 * http://adamhaile.net
 *
 * This file is part of Elpis.
 * Elpis is free software: you can redistribute it and/or modify 
 * it under the terms of the GNU General Public License as published by 
 * the Free Software Foundation, either version 3 of the License, or 
 * (at your option) any later version.
 * 
 * Elpis is distributed in the hope that it will be useful, 
 * but WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License 
 * along with Elpis. If not, see http://www.gnu.org/licenses/.
*/

using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace Elpis
{
    public static class DependencyObjectExtensions
    {
        public static T FindParent<T>(this DependencyObject child) where T : DependencyObject
        {
            DependencyObject parent = LogicalTreeHelper.GetParent(child);
            if (parent != null && typeof (T) != parent.GetType())
                return parent.FindParent<T>();

            return (T) parent;
        }

        public static T FindParentByName<T>(this DependencyObject child, string name) where T : DependencyObject
        {
            DependencyObject parent = LogicalTreeHelper.GetParent(child);
            if (parent != null &&
                (typeof (T) != parent.GetType() ||
                 ((string) parent.GetValue(FrameworkElement.NameProperty)).Equals(name)))
                return parent.FindParent<T>();

            return (T) parent;
        }

        public static T FindSiblingByName<T>(this DependencyObject sibling, string name) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(sibling);

            if (parent == null) return null;

            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                DependencyObject sib = VisualTreeHelper.GetChild(parent, i);
                if (sib != sibling &&
                    (typeof (T) == sib.GetType() && ((string) sib.GetValue(FrameworkElement.NameProperty)).Equals(name)))
                    return (T) sib;
            }

            return null;
        }

        public static T FindChildByName<T>(this DependencyObject parent, string childName) where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                var childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChildByName<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T) child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T) child;
                    break;
                }
            }

            return foundChild;
        }
    }

    public static class DispatcherExtensions
    {
        public static TResult Dispatch<TResult>(this DispatcherObject source, Func<TResult> func)
        {
            if (source.Dispatcher.CheckAccess())
                return func();

            return (TResult) source.Dispatcher.Invoke(func);
        }

        public static TResult Dispatch<T, TResult>(this T source, Func<T, TResult> func) where T : DispatcherObject
        {
            if (source.Dispatcher.CheckAccess())
                return func(source);

            return (TResult) source.Dispatcher.Invoke(func, source);
        }

        public static TResult Dispatch<TSource, T, TResult>(this TSource source, Func<TSource, T, TResult> func,
                                                            T param1) where TSource : DispatcherObject
        {
            if (source.Dispatcher.CheckAccess())
                return func(source, param1);

            return (TResult) source.Dispatcher.Invoke(func, source, param1);
        }

        public static TResult Dispatch<TSource, T1, T2, TResult>(this TSource source,
                                                                 Func<TSource, T1, T2, TResult> func, T1 param1,
                                                                 T2 param2) where TSource : DispatcherObject
        {
            if (source.Dispatcher.CheckAccess())
                return func(source, param1, param2);

            return (TResult) source.Dispatcher.Invoke(func, source, param1, param2);
        }

        public static TResult Dispatch<TSource, T1, T2, T3, TResult>(this TSource source,
                                                                     Func<TSource, T1, T2, T3, TResult> func, T1 param1,
                                                                     T2 param2, T3 param3)
            where TSource : DispatcherObject
        {
            if (source.Dispatcher.CheckAccess())
                return func(source, param1, param2, param3);

            return (TResult) source.Dispatcher.Invoke(func, source, param1, param2, param3);
        }

        public static void Dispatch(this DispatcherObject source, Action func)
        {
            if (source.Dispatcher.CheckAccess())
                func();
            else
                source.Dispatcher.Invoke(func);
        }

        public static void Dispatch<TSource>(this TSource source, Action<TSource> func) where TSource : DispatcherObject
        {
            if (source.Dispatcher.CheckAccess())
                func(source);
            else
                source.Dispatcher.Invoke(func, source);
        }

        public static void Dispatch<TSource, T1>(this TSource source, Action<TSource, T1> func, T1 param1)
            where TSource : DispatcherObject
        {
            if (source.Dispatcher.CheckAccess())
                func(source, param1);
            else
                source.Dispatcher.Invoke(func, source, param1);
        }

        public static void Dispatch<TSource, T1, T2>(this TSource source, Action<TSource, T1, T2> func, T1 param1,
                                                     T2 param2) where TSource : DispatcherObject
        {
            if (source.Dispatcher.CheckAccess())
                func(source, param1, param2);
            else
                source.Dispatcher.Invoke(func, source, param1, param2);
        }

        public static void Dispatch<TSource, T1, T2, T3>(this TSource source, Action<TSource, T1, T2, T3> func,
                                                         T1 param1, T2 param2, T3 param3)
            where TSource : DispatcherObject
        {
            if (source.Dispatcher.CheckAccess())
                func(source, param1, param2, param3);
            else
                source.Dispatcher.Invoke(func, source, param1, param2, param3);
        }

        //Begin Overloads
        public static void BeginDispatch<TResult>(this DispatcherObject source, Func<TResult> func)
        {
            if (source.Dispatcher.CheckAccess())
                func();

            source.Dispatcher.BeginInvoke(func);
        }

        public static void BeginDispatch<T, TResult>(this T source, Func<T, TResult> func) where T : DispatcherObject
        {
            if (source.Dispatcher.CheckAccess())
                func(source);

            source.Dispatcher.BeginInvoke(func, source);
        }

        public static void BeginDispatch<TSource, T, TResult>(this TSource source, Func<TSource, T, TResult> func,
                                                              T param1) where TSource : DispatcherObject
        {
            if (source.Dispatcher.CheckAccess())
                func(source, param1);

            source.Dispatcher.BeginInvoke(func, source, param1);
        }

        public static void BeginDispatch<TSource, T1, T2, TResult>(this TSource source,
                                                                   Func<TSource, T1, T2, TResult> func, T1 param1,
                                                                   T2 param2) where TSource : DispatcherObject
        {
            if (source.Dispatcher.CheckAccess())
                func(source, param1, param2);

            source.Dispatcher.BeginInvoke(func, source, param1, param2);
        }

        public static void BeginDispatch<TSource, T1, T2, T3, TResult>(this TSource source,
                                                                       Func<TSource, T1, T2, T3, TResult> func,
                                                                       T1 param1, T2 param2, T3 param3)
            where TSource : DispatcherObject
        {
            if (source.Dispatcher.CheckAccess())
                func(source, param1, param2, param3);

            source.Dispatcher.BeginInvoke(func, source, param1, param2, param3);
        }

        public static void BeginDispatch(this DispatcherObject source, Action func)
        {
            if (source.Dispatcher.CheckAccess())
                func();
            else
                source.Dispatcher.BeginInvoke(func);
        }

        public static void BeginDispatch<TSource>(this TSource source, Action<TSource> func)
            where TSource : DispatcherObject
        {
            if (source.Dispatcher.CheckAccess())
                func(source);
            else
                source.Dispatcher.BeginInvoke(func, source);
        }

        public static void BeginDispatch<TSource, T1>(this TSource source, Action<TSource, T1> func, T1 param1)
            where TSource : DispatcherObject
        {
            if (source.Dispatcher.CheckAccess())
                func(source, param1);
            else
                source.Dispatcher.BeginInvoke(func, source, param1);
        }

        public static void BeginDispatch<TSource, T1, T2>(this TSource source, Action<TSource, T1, T2> func, T1 param1,
                                                          T2 param2) where TSource : DispatcherObject
        {
            if (source.Dispatcher.CheckAccess())
                func(source, param1, param2);
            else
                source.Dispatcher.BeginInvoke(func, source, param1, param2);
        }

        public static void BeginDispatch<TSource, T1, T2, T3>(this TSource source, Action<TSource, T1, T2, T3> func,
                                                              T1 param1, T2 param2, T3 param3)
            where TSource : DispatcherObject
        {
            if (source.Dispatcher.CheckAccess())
                func(source, param1, param2, param3);
            else
                source.Dispatcher.BeginInvoke(func, source, param1, param2, param3);
        }
    }
}