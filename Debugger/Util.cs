using System;
using System.IO;
using System.Linq;
using System.Reflection;
using ModTools.Utils;
using UnityEngine;

namespace ModTools
{

    public static class Util
    {
        public static bool ComponentIsEnabled(Component component)
        {
            var prop = component.GetType().GetProperty("enabled");
            if (prop == null)
            {
                return true;
            }

            return (bool)prop.GetValue(component, null);
        }
    }

}
