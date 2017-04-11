﻿using System;
using System.Collections.Generic;
using System.Linq;
using EcsRx.Components;
using EcsRx.Unity.Helpers.UIAspects;
using EcsRx.Unity.MonoBehaviours;
using EcsRx.Unity.MonoBehaviours.Editor.EditorHelper;
using UnityEditor;
using UnityEngine;

namespace EcsRx.Unity.Helpers
{
    [CustomEditor(typeof(EntityView))]
    public class EntityViewInspector : Editor
    {
        private EntityView _entityView;

        public bool showComponents;

        private void PoolSection()
        {
            EditorGUIHelper.WithVerticalBoxLayout(() =>
            {
                if (GUILayout.Button("Destroy Entity"))
                {
                    _entityView.Pool.RemoveEntity(_entityView.Entity);
                    Destroy(_entityView.gameObject);
                }

                EditorGUIHelper.WithVerticalBoxLayout(() =>
                {
                    var id = _entityView.Entity.Id.ToString();
                    EditorGUIHelper.WithLabelField("Entity Id: ", id);
                });

                EditorGUIHelper.WithVerticalBoxLayout(() =>
                {
                    EditorGUIHelper.WithLabelField("Pool: ", _entityView.Pool.Name);
                });
            });
        }

        private void ComponentListings()
        {
            EditorGUILayout.BeginVertical(EditorGUIHelper.DefaultBoxStyle);
            EditorGUIHelper.WithHorizontalLayout(() =>
            {
                EditorGUIHelper.WithLabel("Components (" + _entityView.Entity.Components.Count() + ")");
                if (EditorGUIHelper.WithIconButton("▸")) { showComponents = false; }
                if (EditorGUIHelper.WithIconButton("▾")) { showComponents = true; }
            });

            var componentsToRemove = new List<int>();
            if (showComponents)
            {
                for (var i = 0; i < _entityView.Entity.Components.Count(); i++)
                {
                    var currentIndex = i;
                    EditorGUIHelper.WithVerticalBoxLayout(() =>
                    {
                        var componentType = _entityView.Entity.Components.ElementAt(currentIndex).GetType();
                        var typeName = componentType.Name;
                        var typeNamespace = componentType.Namespace;

                        EditorGUIHelper.WithVerticalLayout(() =>
                        {
                            EditorGUIHelper.WithHorizontalLayout(() =>
                            {
                                if (EditorGUIHelper.WithIconButton("-"))
                                {
                                    componentsToRemove.Add(currentIndex);
                                }

                                EditorGUIHelper.WithLabel(typeName);
                            });

                            EditorGUILayout.LabelField(typeNamespace);
                            EditorGUILayout.Space();
                        });
                        
                        var component = _entityView.Entity.Components.ElementAt(currentIndex);
                        ComponentUIAspect.ShowComponentProperties(component);
                    });
                }
            }

            EditorGUILayout.EndVertical();

            for (var i = 0; i < componentsToRemove.Count; i++)
            {
                var component = _entityView.Entity.Components.ElementAt(i);
                _entityView.Entity.RemoveComponent(component);
            }
        }

        public static Type GetTypeWithAssembly(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type != null) return type;
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = a.GetType(typeName);
                if (type != null)
                    return type;
            }
            return null;
        }

        private void ComponentSelectionSection()
        {
            EditorGUIHelper.WithVerticalBoxLayout(() =>
            {
                var availableTypes = ComponentLookup.AllComponents
                    .Where(x => !_entityView.Entity.Components.Select(y => y.GetType()).Contains(x))
                    .ToArray();

                var types = availableTypes.Select(x => string.Format("{0} [{1}]", x.Name, x.Namespace)).ToArray();
                var index = -1;
                index = EditorGUILayout.Popup("Add Component", index, types);
                if (index >= 0)
                {
                    var component = (IComponent)Activator.CreateInstance(availableTypes[index]);
                    _entityView.Entity.AddComponent(component);
                }
            });
        }

        public override void OnInspectorGUI()
        {
            _entityView = (EntityView)target;

            if (_entityView.Entity == null)
            {
                EditorGUILayout.LabelField("No Entity Assigned");
                return;
            }

            PoolSection();
            EditorGUILayout.Space();
            ComponentSelectionSection();
            ComponentListings();
        }
    }
}