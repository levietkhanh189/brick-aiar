﻿using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

namespace Battlehub.RTCommon
{
    public delegate void RuntimeSelectionChanged(Object[] unselectedObjects);

    public static class IRuntimeSelectionExt
    {
        public static bool IsNullOrEmpty(this IRuntimeSelection selection)
        {
            return selection.objects == null || selection.objects.Length == 0;
        }
    }

    public interface IRuntimeSelection : IEnumerable
    {
        event RuntimeSelectionChanged SelectionChanged;
        bool Enabled
        {
            get;
            set;
        }

        bool EnableUndo
        {
            get;
            set;
        }

        GameObject activeGameObject
        {
            get;
            set;
        }
        Object activeObject
        {
            get;
            set;
        }
        Object[] objects
        {
            get;
            set;
        }

        GameObject[] gameObjects
        {
            get;
        }

        Transform activeTransform
        {
            get;
        }

        int Length
        {
            get;
        }

        bool IsSelected(Object obj);

        void Select(Object activeObject, Object[] selection);
    }

    public interface IRuntimeSelectionInternal : IRuntimeSelection
    {
        Object INTERNAL_activeObjectProperty
        {
            get;
            set;
        }

        Object[] INTERNAL_objectsProperty
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Runtime selection (rough equivalent of UnityEditor.Selection class) 
    /// </summary>
    public class RuntimeSelection : IRuntimeSelectionInternal
    {
        public event RuntimeSelectionChanged SelectionChanged;

        public Object INTERNAL_activeObjectProperty
        {
            get { return m_activeObject; }
            set
            {
                m_activeObject = value;
            }
        }

        public Object[] INTERNAL_objectsProperty
        {
            get { return m_objects; }
            set
            {
                SetObjects(value);
            }
        }

        private bool m_isEnabled = true;
        public bool Enabled
        {
            get { return m_isEnabled; }
            set
            {
                m_isEnabled = value;
                if (!m_isEnabled)
                {
                    objects = null;
                }
            }
        }

        private bool m_enableUndo = true;
        public bool EnableUndo
        {
            get { return m_enableUndo; }
            set { m_enableUndo = value; }
        }

        protected void RaiseSelectionChanged(Object[] unselectedObjects)
        {
            if (SelectionChanged != null)
            {
                SelectionChanged(unselectedObjects);
            }
        }

        public GameObject activeGameObject
        {
            get { return activeObject as GameObject; }
            set { activeObject = value; }
        }

        protected Object m_activeObject;
        public Object activeObject
        {
            get { return m_activeObject; }
            set
            {
                if (value == null)
                {
                    objects = null;
                }
                else
                {
                    objects = new[] { value };
                }
            }
        }

        protected Object[] m_objects;
        public Object[] objects
        {
            get { return m_objects; }
            set
            {
                if (!m_isEnabled)
                {
                    return;
                }

                if (IsSelectionChanged(value))
                {
                    if (m_editor != null && m_editor.Undo.Enabled && EnableUndo)
                    {
                        m_editor.Undo.Select(this, value, null);
                    }
                    else
                    {
                        SetObjects(value);
                    }
                }
            }
        }

        public int Length
        {
            get
            {
                if (m_objects == null)
                {
                    return 0;
                }
                return m_objects.Length;
            }
        }

        private HashSet<Object> m_selectionHS = null;
        private IRTE m_editor;
        public RuntimeSelection(IRTE rte)
        {
            m_editor = rte;
        }

        public RuntimeSelection()
        {
        }

        public bool IsSelected(Object obj)
        {
            if (m_selectionHS == null)
            {
                return false;
            }
            return m_selectionHS.Contains(obj);
        }

        private void UpdateHS()
        {
            if (m_objects != null)
            {
                m_selectionHS = new HashSet<Object>(m_objects);
            }
            else
            {
                m_selectionHS = null;
            }
        }

        private bool IsSelectionChanged(Object[] value)
        {
            if (m_objects == value)
            {
                return false;
            }

            if (m_objects == null)
            {
                return value.Length != 0;
            }

            if (value == null)
            {
                return m_objects.Length != 0;
            }

            if (m_objects.Length != value.Length)
            {
                return true;
            }

            for (int i = 0; i < m_objects.Length; ++i)
            {
                if (m_objects[i] != value[i])
                {
                    return true;
                }
            }

            return false;
        }

        protected void SetObjects(Object[] value)
        {
            if (!IsSelectionChanged(value))
            {
                return;
            }

            Object[] oldObjects = m_objects != null ? m_objects.Where(obj => obj != null).ToArray() : m_objects;
            if (value == null)
            {
                m_objects = null;
                m_activeObject = null;
            }
            else
            {
                m_objects = value.Where(v => v != null).ToArray();
                if (m_activeObject == null || !m_objects.Contains(m_activeObject))
                {
                    m_activeObject = m_objects.OfType<Object>().FirstOrDefault();
                }
            }

            UpdateHS();
            RaiseSelectionChanged(oldObjects);
        }

        public GameObject[] gameObjects
        {
            get
            {
                if (m_objects == null)
                {
                    return null;
                }

                return m_objects.OfType<GameObject>().ToArray();
            }
        }

        public Transform activeTransform
        {
            get
            {
                if (m_activeObject == null)
                {
                    return null;
                }

                if (m_activeObject is GameObject)
                {
                    return ((GameObject)m_activeObject).transform;
                }
                return null;
            }
        }

        public void Select(Object activeObject, Object[] selection)
        {
            if (IsSelectionChanged(selection))
            {
                m_activeObject = activeObject;
                SetObjects(selection);
            }
        }

        private Object[] m_empty = new Object[0];
        public IEnumerator GetEnumerator()
        {
            if (m_objects != null)
            {
                return m_objects.GetEnumerator();
            }
            return m_empty.GetEnumerator();
        }
    }

    public static class IRuntimeSelectionExtensions
    {
        private static readonly ExposeToEditor[] s_emptyExposeToEditor = new ExposeToEditor[0];
        private static readonly Transform[] s_emptyTransform = new Transform[0];

        public static IList<Transform> GetTransforms(this IRuntimeSelection selection)
        {
            if (selection.activeGameObject == null)
            {
                return s_emptyTransform;
            }
            return selection.GetComponents<Transform>();
        }

        public static IList<ExposeToEditor> GetExposedToEditor(this IRuntimeSelection selection)
        {
            if (selection.activeGameObject == null)
            {
                return s_emptyExposeToEditor;
            }
            return selection.GetComponents<ExposeToEditor>();
        }

        public static IList<T> GetComponents<T>(this IRuntimeSelection selection) where T : Component
        {
            if (selection.activeGameObject == null)
            {
                return new T[0];
            }

            List<T> result = new List<T>();
            for (int i = 0; i < selection.objects.Length; ++i)
            {
                GameObject go = selection.objects[i] as GameObject;
                if (go != null)
                {
                    T exposedToEditor = go.GetComponent<T>();
                    if (exposedToEditor != null)
                    {
                        result.Add(exposedToEditor);
                    }
                }
            }
            return result;
        }
    }

    public static class RuntimeSelectionUtil
    {
        private static HashSet<Object> m_rootsHs = new HashSet<Object>();
        public static Object[] GetRoots(IList<Object> objects)
        {
            if (objects == null)
            {
                return null;
            }

            if (objects.Count == 0)
            {
                return objects.ToArray();
            }

            if (objects.Count == 1)
            {
                return new[] { GetRoot(objects[0]) };
            }

            for (int i = 0; i < objects.Count; ++i)
            {
                m_rootsHs.Add(GetRoot(objects[i]));
            }

            Object[] roots = m_rootsHs.ToArray();
            m_rootsHs.Clear();

            return roots;
        }

        private static Object GetRoot(Object obj)
        {
            GameObject go = obj as GameObject;
            if (go == null)
            {
                Component component = obj as Component;
                if(component)
                {
                    go = component.gameObject;
                }
                else
                {
                    return obj;
                }
            }

            while (go.transform.parent != null && go.transform.parent.tag != ExposeToEditor.HierarchyRootTag)
            {
                go = go.transform.parent.gameObject;
            }

            return go;
        }
    }
}
