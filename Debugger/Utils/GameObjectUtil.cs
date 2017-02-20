using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ModTools
{
    public static class GameObjectUtil
    {

        public static Dictionary<GameObject, bool> FindSceneRoots()
        {
            Dictionary<GameObject, bool> roots = new Dictionary<GameObject, bool>();

            GameObject[] objects = GameObject.FindObjectsOfType<GameObject>();
            foreach (var obj in objects)
            {
                if (!roots.ContainsKey(obj.transform.root.gameObject))
                {
                    roots.Add(obj.transform.root.gameObject, true);
                }
            }

            return roots;
        }

		public static Dictionary<GameObject, bool> FindAllGameObjects()
		{
			Dictionary<GameObject, bool> roots = new Dictionary<GameObject, bool>();

			GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>().ToArray();
			foreach (var obj in objects)
			{
				if (!roots.ContainsKey(obj.transform.root.gameObject))
				{
					roots.Add(obj.transform.root.gameObject, true);
				}
			}

			return roots;
		}

		public static List<GameObject> FindGameObjectsByName(string name, bool inScene)
		{
			List<GameObject> gameObjects = new List<GameObject>();
			string nameLowerCase = name.ToLower();

			GameObject[] objects = inScene ? GameObject.FindObjectsOfType<GameObject>()
										   : Resources.FindObjectsOfTypeAll<GameObject>().Except(GameObject.FindObjectsOfType<GameObject>()).ToArray();
			foreach (var go in objects)
			{
				if (go.name.ToLower().Contains(nameLowerCase))
				{
					gameObjects.Add(go);
				}
			}

			return gameObjects;
		}

		public static List<Component> FindComponentsOfType(string typeName, bool inScene)
		{
			string typeNameLowerCase = typeName.ToLower();
			List<Component> components = new List<Component>();

			foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (Type t in a.GetTypes())
				{
					if (t.FullName.ToLower().Contains(typeNameLowerCase) && typeof(Component).IsAssignableFrom(t))
					{
						object[] objects = inScene ? GameObject.FindObjectsOfType(t)
												   : Resources.FindObjectsOfTypeAll(t).Except(GameObject.FindObjectsOfType(t)).ToArray();
						foreach (var obj in objects)
						{
							if (obj.GetType() == t)
							{
								components.Add(obj as Component);
							}
						}
					}
				}
			}			

			return components;
		}

        public static string WhereIs(GameObject gameObject, bool logToConsole = true)
        {
            string outResult = gameObject.name;
            WhereIsInternal(gameObject, ref outResult);
            
            if (logToConsole)
            {
                Debug.LogWarning(outResult);
            }

            return outResult;
        }

        private static void WhereIsInternal(GameObject gameObject, ref string outResult)
        {
            outResult = gameObject.name + "." + outResult;
            if (gameObject.transform.parent != null)
            {
                WhereIsInternal(gameObject.transform.parent.gameObject, ref outResult);
            }
        }

    }

}
