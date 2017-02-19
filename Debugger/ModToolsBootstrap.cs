using System;
using System.Linq;
using UnityEngine;

namespace ModTools
{
    public static class ModToolsBootstrap
    {
        public static bool initialized;
        private static bool bootstrapped;

        public static void Bootstrap()
        {
            if (!bootstrapped)
            {
                bootstrapped = true;
            }
            if (initialized)
            {
                return;
            }
            try
            {
                InitModTools();
                initialized = true;
            }
            catch (Exception e)
            {
                Debug.Log(e.GetType().Name + ": " + e.Message);
            }
        }

        private static void InitModTools()
        {
            var modToolsGameObject = GameObject.Find("ModTools");
            if (modToolsGameObject != null)
            {
                return;
            }

            modToolsGameObject = new GameObject("ModTools");
            var modTools = modToolsGameObject.AddComponent<ModTools>();
            modTools.Initialize();
        }

    }
}
