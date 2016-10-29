using ICities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnableAchievements
{
	public class EnableAchievementsMod : IUserMod
	{
		public EnableAchievementsMod()
		{
			EnableAchievementsBootstrap.Bootstrap();
		}

		public string Description => "Enables achievements even when mods are active.";

		public string Name
		{
			get
			{
				return "Enable Achievements";
			}
		}
	}
}