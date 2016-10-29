using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EnableAchievements
{
	internal static class DebugUtils
	{
		public static void Log(string message)
		{
			Debug.Log("[Enable Achievements] " + message);
		}
	}
}