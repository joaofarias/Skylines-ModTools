using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModTools
{
	public class ResourcesExplorer : SceneExplorer
	{
		public ResourcesExplorer()
			: base("Resources Explorer")
		{

		}

		protected override void UpdateObjectsList()
		{
			sceneRoots = GameObjectUtil.FindAllGameObjects();
		}

		protected override List<GameObject> FindGameObjectByName(string name)
		{
			return GameObjectUtil.FindGameObjectsByName(name, false);
		}

		protected override List<Component> FindComponentsByTypeName(string typeName)
		{
			return GameObjectUtil.FindComponentsOfType(typeName, false);
		}
	}
}
