using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.HTTP.Paradox;
using ColossalFramework.Packaging;
using ColossalFramework.Plugins;
using ColossalFramework.Steamworks;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static ColossalFramework.Plugins.PluginManager;

namespace EnableAchievements
{
	internal static class EnableAchievementsBootstrap
	{
		private static UIButton m_ModsButton;
		private static Dictionary<string, PluginInfo> m_Plugins;
		private static UISprite m_ActiveSprite;
		private static int m_ActiveModCount;


		public static void Bootstrap()
		{
			try
			{
				DebugUtils.Log("Redirecting Calls to EnabledModCount");

				var sourceMethodInfo = typeof(PluginManager).GetProperty("enabledModCount").GetGetMethod();
				var targetMethodInfo = typeof(EnableAchievementsBootstrap).GetProperty("PluginManagerEnabledModCount", BindingFlags.NonPublic | BindingFlags.Static).GetGetMethod(true);

				RedirectionHelper.RedirectCalls(sourceMethodInfo, targetMethodInfo);

				DebugUtils.Log("Calls to EnabledModCount redirected");

				DebugUtils.Log("Redirecting Calls to CustomContentInfo");

				sourceMethodInfo = typeof(TelemetryManager).GetMethod("CustomContentInfo");
				targetMethodInfo = typeof(EnableAchievementsBootstrap).GetMethod("TelemetryManagerCustomContentInfo", BindingFlags.NonPublic | BindingFlags.Static);

				RedirectionHelper.RedirectCalls(sourceMethodInfo, targetMethodInfo);

				DebugUtils.Log("Calls to CustomContentInfo redirected");

				DebugUtils.Log("Redirecting Calls to CreateCategories");

				sourceMethodInfo = typeof(OptionsMainPanel).GetMethod("CreateCategories", BindingFlags.NonPublic | BindingFlags.Instance);
				targetMethodInfo = typeof(EnableAchievementsBootstrap).GetMethod("OptionsMainPanelCreateCategories", BindingFlags.NonPublic | BindingFlags.Static);

				RedirectionHelper.RedirectCalls(sourceMethodInfo, targetMethodInfo);

				DebugUtils.Log("Calls to CreateCategories redirected");

				DebugUtils.Log("Redirecting Calls to SetModString");

				sourceMethodInfo = typeof(WorkshopAdPanel).GetMethod("SetModsString", BindingFlags.NonPublic | BindingFlags.Instance);
				targetMethodInfo = typeof(EnableAchievementsBootstrap).GetMethod("WorkshopAdPanelSetModsString", BindingFlags.NonPublic | BindingFlags.Static);

				RedirectionHelper.RedirectCalls(sourceMethodInfo, targetMethodInfo);

				DebugUtils.Log("Calls to SetModString redirected");
			}
			catch(Exception ex)
			{
				DebugUtils.Log(ex.Message);
			}
		}


		public static void UpdateActiveModCount()
		{
			if(m_Plugins == null)
			{
				try
				{
					DebugUtils.Log("Getting Plugin Dictionary");
					var mPluginsFieldInfo = typeof(PluginManager).GetField("m_Plugins", BindingFlags.Instance | BindingFlags.NonPublic);
					m_Plugins = mPluginsFieldInfo.GetValue(Singleton<PluginManager>.instance) as Dictionary<string, PluginInfo>;
					DebugUtils.Log("Plugin Dictionary: " + m_Plugins);
				}
				catch(Exception ex) { DebugUtils.Log(ex.Message); }
			}

			int previousModCount = m_ActiveModCount;

			m_ActiveModCount = 0;
			if(m_Plugins != null)
			{
				foreach(var modPair in m_Plugins)
				{
					if(modPair.Value.isEnabled)
					{
						m_ActiveModCount++;
					}
				}
				DebugUtils.Log("Actual Active Mods: " + m_ActiveModCount);
			}

			if(m_ActiveModCount != previousModCount)
			{
				UpdateOptionsPanelContent();
			}
		}

		public static void UpdateOptionsPanelContent()
		{
			OptionsMainPanelCreateCategories();
		}

		private static int PluginManagerEnabledModCount
		{
			get
			{
				UpdateActiveModCount();
				DebugUtils.Log("Returned 0 Mods Active");
				return 0;
			}
		}

		private static void WorkshopAdPanelSetModsString()
		{
			if(m_ModsButton == null)
			{
				try
				{
					DebugUtils.Log("Getting MenuContainer");
					var menuContainerObject = GameObject.Find("MenuContainer");
					DebugUtils.Log("MenuContainer: " + menuContainerObject);
					var menuContainer = menuContainerObject.GetComponent<UIPanel>();
					DebugUtils.Log("Finding Mods Button");
					m_ModsButton = menuContainer.Find<UIButton>("Mods");

					if(m_ActiveSprite == null)
					{
						m_ActiveSprite = new GameObject("EA_ActiveSprite") { transform = { parent = m_ModsButton.transform } }.AddComponent<UISprite>();
						m_ActiveSprite.spriteName = "ThumbnailTrophy";
						m_ActiveSprite.size = new Vector2(29f, 20f);
						m_ActiveSprite.relativePosition = new Vector3(225f, 3f);
					}
				}
				catch(Exception ex) { DebugUtils.Log(ex.Message); }
			}

			if(m_ModsButton != null && m_ActiveSprite == null)
			{
				try
				{
					m_ActiveSprite = new GameObject("EA_ActiveSprite") { transform = { parent = m_ModsButton.transform } }.AddComponent<UISprite>();
					m_ActiveSprite.spriteName = "ThumbnailTrophy";
					m_ActiveSprite.size = new Vector2(29f, 20f);
					m_ActiveSprite.relativePosition = new Vector3(225f, 3f);
				}
				catch(Exception ex) { DebugUtils.Log(ex.Message); }
			}

			UpdateActiveModCount();

			var text = LocaleFormatter.FormatGeneric("MOD_ENABLED_STATUS", new object[] { m_ActiveModCount, Singleton<PluginManager>.instance.modCount });
			DebugUtils.Log(text);

			if(m_ModsButton != null)
			{
				m_ModsButton.text = text;
			}

			if(m_ActiveSprite != null)
			{
				m_ActiveSprite.isVisible = (m_ActiveModCount == 0 || m_ActiveModCount > Singleton<PluginManager>.instance.enabledModCount);
			}
		}

		private static void TelemetryManagerCustomContentInfo(int buildingsCount, int propsCount, int treeCount, int vehicleCount)
		{
			UpdateActiveModCount();

			try
			{
				Telemetry telemetry = new Telemetry();
				Telemetry.Pair[] infoPair = new Telemetry.Pair[] { new Telemetry.Pair("buildings", buildingsCount), new Telemetry.Pair("props", propsCount), new Telemetry.Pair("trees", treeCount), new Telemetry.Pair("vehicles", vehicleCount) };
				telemetry.AddEvent("custom_content", infoPair);
				DebugUtils.Log("Sending telemetry with " + m_ActiveModCount + " mods active");
				Telemetry.Pair[] pairArray2 = new Telemetry.Pair[] { new Telemetry.Pair("enabledModCount", m_ActiveModCount), new Telemetry.Pair("modCount", Singleton<PluginManager>.instance.modCount) };
				telemetry.AddEvent("custom_mods", pairArray2);
				telemetry.Push();
				IEnumerator<PluginManager.PluginInfo> enumerator = Singleton<PluginManager>.instance.GetPluginsInfo().GetEnumerator();
				try
				{
					while(enumerator.MoveNext())
					{
						PluginManager.PluginInfo current = enumerator.Current;
						if(current.isEnabled)
						{
							Telemetry telemetry2 = new Telemetry();
							Telemetry.Pair[] pairArray3 = new Telemetry.Pair[] { new Telemetry.Pair("modName", current.name), new Telemetry.Pair("modWorkshopID", !(current.publishedFileID != PublishedFileId.invalid) ? "none" : current.publishedFileID.ToString()), new Telemetry.Pair("assemblyInfo", current.assembliesString) };
							telemetry2.AddEvent("mod_used", pairArray3);
							telemetry2.Push();
						}
					}
				}
				finally
				{
					if(enumerator == null)
					{
					}
					enumerator.Dispose();
				}
			}
			catch(Exception ex)
			{
				DebugUtils.Log(ex.Message);
				//CODebugBase<LogChannel>.Warn(LogChannel.HTTP, exception.GetType() + ": Telemetry event failed " + exception.Message);
			}
		}

		private static void OptionsMainPanelCreateCategories()
		{
			DebugUtils.Log("Updating Categories");
			try
			{
				Type optionsMainPanelType = typeof(OptionsMainPanel);

				var optionsPanel = UIView.library.Get<OptionsMainPanel>("OptionsPanel");
				DebugUtils.Log("" + optionsPanel);

				var m_Dummies = optionsMainPanelType.GetField("m_Dummies", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(optionsPanel) as List<UIComponent>;
				for(int i = 0; i < m_Dummies.Count; i++)
				{
					if(m_Dummies[i].parent != null)
					{
						m_Dummies[i].parent.RemoveUIComponent(m_Dummies[i]);
					}
					UnityEngine.Object.DestroyImmediate(m_Dummies[i]);
				}
				m_Dummies.Clear();

				var m_Categories = optionsMainPanelType.GetField("m_Categories", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(optionsPanel) as UIListBox;
				var m_CategoriesContainer = optionsMainPanelType.GetField("m_CategoriesContainer", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(optionsPanel) as UITabContainer;

				var onCategoryChanged = optionsMainPanelType.GetMethod("OnCategoryChanged", BindingFlags.Instance | BindingFlags.NonPublic);
				var addGroupCategory = optionsMainPanelType.GetMethod("AddGroupCategory", BindingFlags.Instance | BindingFlags.NonPublic);
				var addSpace = optionsMainPanelType.GetMethod("AddSpace", BindingFlags.Instance | BindingFlags.NonPublic);
				var addCategory = optionsMainPanelType.GetMethod("AddCategory", BindingFlags.Instance | BindingFlags.NonPublic);
				var probeUISettingsMods = optionsMainPanelType.GetMethod("ProbeUISettingsMods", BindingFlags.Instance | BindingFlags.NonPublic);
				var addUserMods = optionsMainPanelType.GetMethod("AddUserMods", BindingFlags.Instance | BindingFlags.NonPublic);

				m_Categories.eventSelectedIndexChanged -= delegate (UIComponent component, int value)
				{
					onCategoryChanged.Invoke(optionsPanel, new object[] { component, value });
				};

				m_Categories.items = new string[0];
				int selectedIndex = m_CategoriesContainer.selectedIndex;
				m_CategoriesContainer.selectedIndex = -1;

				addGroupCategory.Invoke(optionsPanel, new object[] { Locale.Get("OPTIONS_SYSTEMSETTINGS") });
				addSpace.Invoke(optionsPanel, new object[] { });
				addCategory.Invoke(optionsPanel, new object[] { Locale.Get("OPTIONS_GRAPHICS"), optionsPanel.Find("Graphics") });
				addCategory.Invoke(optionsPanel, new object[] { Locale.Get("OPTIONS_GAMEPLAY"), optionsPanel.Find("Gameplay") });
				addCategory.Invoke(optionsPanel, new object[] { Locale.Get("OPTIONS_KEYMAPPING"), optionsPanel.Find("Keymapping") });
				addCategory.Invoke(optionsPanel, new object[] { Locale.Get("OPTIONS_AUDIO"), optionsPanel.Find("Audio") });
				addCategory.Invoke(optionsPanel, new object[] { Locale.Get("OPTIONS_MISC"), optionsPanel.Find("Misc") });

				if((m_ActiveModCount > 0) && (bool)probeUISettingsMods.Invoke(optionsPanel, new object[] { }))
				{
					addSpace.Invoke(optionsPanel, new object[] { });
					addGroupCategory.Invoke(optionsPanel, new object[] { Locale.Get("OPTIONS_MODSSETTINGS") });
					addSpace.Invoke(optionsPanel, new object[] { });
					addUserMods.Invoke(optionsPanel, new object[] { });
				}

				m_Categories.filteredItems = new int[] { 0, 1, 7, 8, 9 };
				m_Categories.eventSelectedIndexChanged += delegate (UIComponent component, int value)
				{
					onCategoryChanged.Invoke(optionsPanel, new object[] { component, value });
				};

				m_Categories.selectedIndex = selectedIndex;
				m_CategoriesContainer.selectedIndex = selectedIndex;
			}
			catch(Exception ex) { DebugUtils.Log(ex.Message); }
		}
	}
}