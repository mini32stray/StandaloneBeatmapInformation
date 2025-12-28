using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Settings;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StandaloneBeatmapInformation.Core;
using StandaloneBeatmapInformation.Core.ComponentModel;
using Zenject;
using IPA.Logging;

namespace StandaloneBeatmapInformation.View
{
	internal class MenuViewController : IInitializable, IDisposable, ITickable
	{
		internal class MenuHost : HostBase
		{
			private readonly IPA.Logging.Logger? logger;
			private readonly MainContext context;

			[UIValue("enabled")]
			public bool Enabled
			{
				get { return context.Config.Enabled; }
				set
				{
					context.Config.Enabled = value;
					RaisePropertyChanged();
				}
			}

			[UIValue("enable-handle")]
			public bool EnableHandle
			{
				get { return context.Config.EnableHandle; }
				set
				{
					context.Config.EnableHandle = value;
					RaisePropertyChanged();
				}
			}

			[UIValue("show-title")]
			public bool ShowTitle
			{
				get { return context.Config.ShowTitle; }
				set
				{
					context.Config.ShowTitle = value;
					RaisePropertyChanged();
				}
			}

			[UIValue("show-song-author")]
			public bool ShowSongAuthor
			{
				get { return context.Config.ShowSongAuthor; }
				set
				{
					context.Config.ShowSongAuthor = value;
					RaisePropertyChanged();
				}
			}

			[UIValue("show-mapper")]
			public bool ShowMapper
			{
				get { return context.Config.ShowMapper; }
				set
				{
					context.Config.ShowMapper = value;
					RaisePropertyChanged();
				}
			}

			[UIValue("show-map-id")]
			public bool ShowMapId
			{
				get { return context.Config.ShowMapId; }
				set
				{
					context.Config.ShowMapId = value;
					RaisePropertyChanged();
				}
			}

			[UIValue("show-star")]
			public bool ShowStar
			{
				get { return context.Config.ShowStar; }
				set
				{
					context.Config.ShowStar = value;
					RaisePropertyChanged();
				}
			}

			[UIValue("show-characteristic")]
			public bool ShowCharacteristic
			{
				get { return context.Config.ShowCharacteristic; }
				set
				{
					context.Config.ShowCharacteristic = value;
					RaisePropertyChanged();
				}
			}

			[UIValue("show-difficulty")]
			public bool ShowDifficulty
			{
				get { return context.Config.ShowDifficulty; }
				set
				{
					context.Config.ShowDifficulty = value;
					RaisePropertyChanged();
				}
			}

			[UIValue("show-jd")]
			public bool ShowJD
			{
				get { return context.Config.ShowJD; }
				set
				{
					context.Config.ShowJD = value;
					RaisePropertyChanged();
				}
			}

			[UIValue("enable-additional")]
			public bool EnableAdditional
			{
				get { return context.Config.EnableAdditional; }
				set
				{
					context.Config.EnableAdditional = value;
					RaisePropertyChanged();
				}
			}

			public MenuHost(
				IPA.Logging.Logger? logger,
				MainContext context)
			{
				this.logger = logger;
				this.context = context;
			}
		}

		protected const string tabName = "Sta-BeatmapInfo";
		protected const string resourceMenuBsml = "StandaloneBeatmapInformation.View.Menu.bsml";
		protected readonly SiraLog? logger;
		protected readonly MainContext context;
		protected MenuHost? host;

		public MenuViewController(
			SiraLog? logger,
			MainContext context)
		{
			this.logger = logger;
			this.context = context;
		}

		public void Initialize()
		{
			host = new(logger?.Logger?.GetChildLogger(nameof(MenuHost)), context);
			BSMLSettings.Instance.AddSettingsMenu(tabName, resourceMenuBsml, host);
		}

		public void Dispose()
		{
			BSMLSettings.Instance.RemoveSettingsMenu(host);
			host = null;
		}

		public void Tick()
		{
			host?.Tick();
		}
	}
}
