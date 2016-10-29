using ColossalFramework;
using ColossalFramework.Plugins;
using ICities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnableAchievements
{
	public class EnableAchievementsLoader : LoadingExtensionBase
	{
		public override void OnLevelLoaded(LoadMode mode)
		{
			var metaData = Singleton<SimulationManager>.instance.m_metaData;
			if(metaData.m_disableAchievements != SimulationMetaData.MetaBool.False)
			{
				metaData.m_disableAchievements = SimulationMetaData.MetaBool.False;
				DebugUtils.Log("Updating Disable Achievements to False");
			}

			EnableAchievementsBootstrap.UpdateOptionsPanelContent();
		}
	}
}