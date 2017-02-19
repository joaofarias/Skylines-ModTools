using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ModTools
{
    public class Watches : GUIWindow
    {

        private readonly List<ReferenceChain> watches = new List<ReferenceChain>();

        private Configuration config => ModTools.Instance.config;

        private Vector2 watchesScroll = Vector2.zero;

        public Watches()
            : base("Watches", new Rect(504, 128, 800, 300), skin)
        {
            onDraw = DoWatchesWindow;
        }

        public void AddWatch(ReferenceChain refChain)
        {
            watches.Add(refChain);
            visible = true;
        }

        public void RemoveWatch(ReferenceChain refChain)
        {
            if (watches.Contains(refChain))
            {
                watches.Remove(refChain);
            }
        }

        public Type GetWatchType(ReferenceChain refChain)
        {
            Type ret = null;
            if (watches.Contains(refChain))
            {
                var item = refChain.LastItem;
                var info = item as FieldInfo;
                ret = info?.FieldType ?? (item as PropertyInfo)?.PropertyType;
            }
            return ret;
        }

        void DoWatchesWindow()
        {
            watchesScroll = GUILayout.BeginScrollView(watchesScroll);

            foreach (var watch in watches.ToArray())
            {
                GUILayout.BeginHorizontal();

                var type = GetWatchType(watch);

                GUI.contentColor = config.typeColor;
                GUILayout.Label(type.ToString());
                GUI.contentColor = config.nameColor;
                GUILayout.Label(watch.ToString());
                GUI.contentColor = Color.white;
                GUILayout.Label(" = ");

                GUI.enabled = false;

                var value = watch.Evaluate();
                GUI.contentColor = config.valueColor;

                if (value == null || !TypeUtil.IsSpecialType(type))
                {
                    GUILayout.Label(value == null ? "null" : value.ToString());
                }
                else
                {
                    try
                    {
                        GUIControls.EditorValueField(watch, "watch." + watch, type, value);
                    }
                    catch (Exception)
                    {
                        GUILayout.Label(value == null ? "null" : value.ToString());
                    }
                }

                GUI.contentColor = Color.white;

                GUI.enabled = true;

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Find in Scene Explorer"))
                {
                    var sceneExplorer = FindObjectOfType<SceneExplorer>();
                    sceneExplorer.ExpandFromRefChain(watch.Trim(watch.Length - 1));
                    sceneExplorer.visible = true;
                }

				if (GUILayout.Button("Find in Resources Explorer"))
				{
					var sceneExplorer = FindObjectOfType<ResourcesExplorer>();
					sceneExplorer.ExpandFromRefChain(watch.Trim(watch.Length - 1));
					sceneExplorer.visible = true;
				}

				if (GUILayout.Button("x", GUILayout.Width(24)))
                {
                    RemoveWatch(watch);
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
        }

    }

}
