using IPA;
using IPA.Config;
using IPA.Config.Stores;
using SiraUtil.Zenject;
using StandaloneBeatmapInformation.Core;
using IPALogger = IPA.Logging.Logger;

namespace StandaloneBeatmapInformation
{
	[Plugin(RuntimeOptions.DynamicInit), NoEnableDisable]
	public class Plugin
	{
		[Init]
		public void Init(IPALogger logger, Config conf, Zenjector zenjector)
		{
			var pluginConfig = conf.Generated<Configuration.PluginConfig>();
			zenjector.UseMetadataBinder<Plugin>();
			zenjector.UseLogger(logger);
			zenjector.UseHttpService();
			zenjector.Install(Location.App, container =>
			{
				container.BindInstance(pluginConfig);
				container.BindInterfacesAndSelfTo<MainContext>().AsSingle();
			});
			zenjector.Install(Location.Menu, container =>
			{
				container.BindInterfacesTo<View.MenuViewController>().AsSingle();
			});
			zenjector.Install(Location.Player, container =>
			{
				container.BindInterfacesTo<View.InformationPanel>().AsCached();
			});
		}
	}
}
