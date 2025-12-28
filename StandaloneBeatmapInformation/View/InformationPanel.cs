using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.FloatingScreen;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using IPA.Logging;
using SiraUtil.Logging;
using SongDetailsCache.Structs;
using StandaloneBeatmapInformation.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace StandaloneBeatmapInformation.View
{
	internal class InformationPanel : IDisposable, IInitializable, ITickable
	{
		internal record InformationDifficulty(
			bool IsActive = false,
			string DifficultyLabel = "",
			bool IsSelected = false)
		{
			public static InformationDifficulty Empty { get; } = new();
		};

		internal record InformationCharacteristic(
			bool IsActive = false,
			Sprite? Icon = null,
			bool IsSelected = false);

		internal record Information(
			string SongTitle,
			string SongSubtitle,
			string SongAuthor,
			string Mapper,
			string MapId,
			string Stars,
			IEnumerable<InformationDifficulty> Difficulties,
			IEnumerable<InformationCharacteristic> Characteristics,
			string jumpDistance,
			AdditionalText Additional);

		internal class InformationPanelViewController : BSMLResourceViewController
		{
			internal record CharacteristicHost
			{
				[UIValue("is-active")]
				public bool IsActive { get; }

				[UIValue("background-color")]
				public string BackgroundColor { get; }

				[UIValue("image-color")]
				public string ImageColor { get; }

				[UIComponent("image-component")]
				public ImageView? CharaImage;

				public int Num { get; }
				private Sprite? icon;

				public CharacteristicHost(int num, InformationCharacteristic characteristic)
				{
					Num = num;
					IsActive = characteristic.IsActive;
					BackgroundColor = characteristic.IsSelected ? "white" : "#00000000";
					ImageColor = characteristic.IsSelected ? "black" : "white";
					icon = characteristic.Icon;
				}

				public bool SetIcon()
				{
					if (CharaImage is null)
					{
						return false;
					}
					if (icon == null)
					{
						CharaImage.sprite = BeatSaberMarkupLanguage.Utilities.ImageResources.BlankSprite;
					}
					else
					{
						CharaImage.sprite = icon;
					}
					return true;
				}
			}

			internal record DifficultyHost
			{
				[UIValue("is-active")]
				public bool IsActive { get; }

				[UIValue("label")]
				public string Label { get; }

				[UIValue("background-color")]
				public string BackgroundColor { get; }

				[UIValue("body-color")]
				public string BodyColor { get; }

				public DifficultyHost(InformationDifficulty difficulty)
				{
					IsActive = difficulty.IsActive;
					Label = difficulty.DifficultyLabel;
					BackgroundColor = difficulty.IsSelected ? "white" : "#00000000";
					BodyColor = difficulty.IsSelected ? "black" : "white";
				}
			};

			public override string ResourceName => "StandaloneBeatmapInformation.View.InformationPanel.bsml";
			private IPA.Logging.Logger? logger;

			public void InitializeBeforeSet(IPA.Logging.Logger? logger, Information info)
			{
				this.logger = logger;
				SongTitle = info.SongTitle;
				SongSubtitle = info.SongSubtitle;
				SongAuthor = info.SongAuthor;
				Mapper = info.Mapper;
				MapId = info.MapId;
				Stars = info.Stars;
				Diffis = info.Difficulties
					.Select(x => new DifficultyHost(x))
					.ToList();
				Charas = info.Characteristics
					.Select((x, i) => new CharacteristicHost(i, x))
					.ToList();
				JumpDistance = info.jumpDistance;
				RequestState = info.Additional.RequestState;
				Requester = info.Additional.Requester;
			}

			public void InitializeLater()
			{
				Charas.ForEach(x => {
					var result = x.SetIcon();
					if (!result)
					{
						logger?.Error($"failed to set characteristic image {x.Num}. maybe null.");
					}
				});
			}

			public void Tick()
			{

			}

			[UIValue("song-title")]
			public string SongTitle { get; set; } = string.Empty;

			[UIValue("song-subtitle")]
			public string SongSubtitle { get; set; } = string.Empty;

			[UIValue("song-author")]
			public string SongAuthor { get; set; } = string.Empty;

			[UIValue("mapper")]
			public string Mapper { get; set; } = string.Empty;

			[UIValue("map-id")]
			public string MapId { get; set; } = string.Empty;

			[UIValue("stars")]
			public string Stars { get; set; } = string.Empty;

			[UIValue("diffis")]
			public List<DifficultyHost> Diffis { get; private set; } = new List<DifficultyHost>();

			[UIValue("charas")]
			public List<CharacteristicHost> Charas { get; private set; } = new List<CharacteristicHost>();

			[UIValue("jump-distance")]
			public string JumpDistance { get; set; } = string.Empty;

			[UIValue("request-state")]
			public string RequestState { get; set; } = string.Empty;

			[UIValue("requester")]
			public string Requester { get; set; } = string.Empty;
		}

		private readonly SiraLog? logger;
		private readonly MainContext context;
		private readonly GameplayCoreSceneSetupData? gameplayCoreSceneSetupData;
		private readonly AudioTimeSyncController? audioTimeSyncController;
		private readonly VariableMovementDataProvider? variableMovementDataProvider;
		private FloatingScreen? screen;
		private InformationPanelViewController? viewController;

		public InformationPanel(
			SiraLog? logger,
			MainContext context,
			GameplayCoreSceneSetupData? gameplayCoreSceneSetupData,
			AudioTimeSyncController? audioTimeSyncController,
			VariableMovementDataProvider? variableMovementDataProvider)
		{
			this.logger = logger;
			this.context = context;
			this.gameplayCoreSceneSetupData = gameplayCoreSceneSetupData;
			this.audioTimeSyncController = audioTimeSyncController;
			this.variableMovementDataProvider = variableMovementDataProvider;
		}

		public void Initialize()
		{
			if (!context.Config.Enabled)
			{
				logger?.Info($"Skipped initialize: Disabled");
				return;
			}
			var stopwatch = Stopwatch.StartNew();

			_ = InitializeAsync(CancellationToken.None);

			stopwatch.Stop();
			logger?.Info($"Initialize in first frame exited in {stopwatch.ElapsedMilliseconds} ms.");
		}

		private async Task InitializeAsync(CancellationToken token)
		{
			if (gameplayCoreSceneSetupData is null)
			{
				logger?.Error($"Skipped initialize: GameplayCoreSceneSetupData null.");
				return;
			}
			if (audioTimeSyncController is null)
			{
				logger?.Error($"Skipped initialize: AudioTimeSyncController null.");
				return;
			}
			var startSongTime = audioTimeSyncController.songTime;

			var levelId = gameplayCoreSceneSetupData.beatmapLevel.levelID;
			var songHash = SongCore.Collections.GetCustomLevelHash(levelId);
			if (string.IsNullOrEmpty(songHash))
			{
				logger?.Warn($"Skipped initialize: Failed to get song hash from level ID [{levelId}].");
				return;
			}

			//var difficulty = gameplayCoreSceneSetupData.difficultyBeatmap.difficulty;
			//var characteristic = gameplayCoreSceneSetupData.difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic;
			//var preview = gameplayCoreSceneSetupData.previewBeatmapLevel;
			var currentDifficulty = gameplayCoreSceneSetupData.beatmapKey.difficulty;
			var currentCharacteristic = gameplayCoreSceneSetupData.beatmapKey.beatmapCharacteristic;
			var level = gameplayCoreSceneSetupData.beatmapLevel;
			var mapperText = string.Join(",", level.allMappers);
			mapperText += level.allLighters.Length > 0 ? " / " + string.Join(",", level.allLighters) : string.Empty;

			var songTitle = context.Config.ShowTitle? level.songName: string.Empty;
			var songSubTitle = context.Config.ShowTitle ? level.songSubName : string.Empty;
			var songAuthor = context.Config.ShowSongAuthor ? level.songAuthorName : string.Empty;
			var mapper = context.Config.ShowMapper ? mapperText : string.Empty;

			var iDiffs = new InformationDifficulty[5]
			{
				InformationDifficulty.Empty,
				InformationDifficulty.Empty,
				InformationDifficulty.Empty,
				InformationDifficulty.Empty,
				InformationDifficulty.Empty,
			};
			if (context.Config.ShowDifficulty)
			{
				var songData = SongCore.Collections.GetCustomLevelSongData(levelId);
				if (songData is null)
				{
					logger?.Warn($"No song data retrieved for [{levelId}]. skipped.");
					return;
				}
				var difficulties = songData._difficulties
					.Where(x => x._beatmapCharacteristicName == currentCharacteristic.serializedName);
				foreach (var d in difficulties)
				{
					var label = !string.IsNullOrEmpty(d._difficultyLabel)
						? d._difficultyLabel
						: d._difficulty.ToString();
					var fixLabel = label != "ExpertPlus" ? label : "Expert+";
					var index = (int)d._difficulty;
					if (index < 0 || index > 4)
					{
						logger?.Error($"Invalid index {index} from {d._difficulty} of {d._beatmapCharacteristicName}. song hash: ${songHash}");
						continue;
					}
					iDiffs[index] = new InformationDifficulty(true, fixLabel, d._difficulty == currentDifficulty);
				}
			}

			var iChars = new List<InformationCharacteristic>();
			if (context.Config.ShowCharacteristic)
			{
				var characteristics = level.GetCharacteristics();
				foreach (var c in characteristics)
				{
					iChars.Add(new(true, c.icon, c.serializedName == currentCharacteristic.serializedName));
				}
			}

			var mapId = "";
			var stars = "";
			if (context.Config.ShowMapId || context.Config.ShowStar)
			{
				var sdc = await GetSongDifficulty(songHash, currentDifficulty, currentCharacteristic);
				if (sdc.HasValue)
				{
					var sd = sdc.Value;
					mapId = $"!bsr {sd.song.key}";
					if (sd.stars > 0.001)
					{
						stars = "★" + sd.stars.ToString("F2");
					}
				}
				else
				{
					logger?.Warn($"Failed to get song difficulty for song [{songHash}].");
				}
			}

			string jumpDistanceText = "";
			if (context.Config.ShowJD)
			{
				if (variableMovementDataProvider != null)
				{
					await WaitUntilStarted(startSongTime, token);
					var jumpDistance = variableMovementDataProvider.jumpDistance;
					var noteJumpMovementSpeed = gameplayCoreSceneSetupData.beatmapBasicData.noteJumpMovementSpeed;
					logger?.Debug($"jumpDistance: {jumpDistance}, noteJumpMovementSpeed: {noteJumpMovementSpeed}");
					var reactionTime = noteJumpMovementSpeed > 0
						? (jumpDistance * 500 / noteJumpMovementSpeed)
						: 0;
					jumpDistanceText = $"JD: {jumpDistance:F2} ({reactionTime:F0} ms)";
				}
			}

			var additional = context.Config.EnableAdditional
				? context.GetCurrentAdditionalTextFromInterface(songHash)
				: AdditionalText.Empty;

			var info = new Information(
				songTitle,
				songSubTitle,
				songAuthor,
				mapper,
				mapId,
				stars,
				iDiffs,
				iChars,
				jumpDistanceText,
				additional);

			var posture = context.Config.Posture;
			screen = FloatingScreen.CreateFloatingScreen(
				new Vector2(60f, 60f),
				context.Config.EnableHandle,
				new Vector3(posture.PosX, posture.PosY, posture.PosZ),
				Quaternion.Euler(posture.EulerX, posture.EulerY, posture.EulerZ));
			screen.HandleReleased += Screen_HandleReleased;
			viewController = BeatSaberMarkupLanguage.BeatSaberUI.CreateViewController<InformationPanelViewController>();
			viewController.InitializeBeforeSet(logger?.Logger?.GetChildLogger(nameof(InformationPanelViewController)), info);
			screen.SetRootViewController(viewController, ViewController.AnimationType.Out);
			viewController.InitializeLater();

			logger?.Info("Initialized");
		}

		private void Screen_HandleReleased(object sender, FloatingScreenHandleEventArgs e)
		{
			var newPosture = new Configuration.PluginConfig.PosRot()
			{
				PosX = e.Position.x,
				PosY = e.Position.y,
				PosZ = e.Position.z,
				EulerX = e.Rotation.eulerAngles.x,
				EulerY = e.Rotation.eulerAngles.y,
				EulerZ = e.Rotation.eulerAngles.z,
			};
			context.Config.Posture = newPosture;
		}

		private async Task<SongDifficulty?> GetSongDifficulty(string songHash, BeatmapDifficulty difficulty, BeatmapCharacteristicSO characteristic)
		{
			var details = await SongDetailsCache.SongDetails.Init(12);
			if (details.songs.FindByHash(songHash, out var song))
			{
				var d = ToMapDifficulty(difficulty);
				var c = ToMapCharacteristic(characteristic);
				if (song.GetDifficulty(out var songDifficulty, d, c))
				{
					return songDifficulty;
				}
			}
			return null;
		}

		private static MapDifficulty ToMapDifficulty(BeatmapDifficulty difficulty)
		{
			var i = (int)difficulty;
			return (MapDifficulty)i;
		}

		private static MapCharacteristic ToMapCharacteristic(BeatmapCharacteristicSO characteristic)
		{
			return (MapCharacteristic)Enum.Parse(typeof(MapCharacteristic), characteristic.serializedName);
		}

		private async Task WaitUntilStarted(float startSongTime, CancellationToken token)
		{
			var startTime = DateTime.Now;
			if (audioTimeSyncController is null)
			{
				return;
			}
			while (audioTimeSyncController.songTime <= startSongTime)
			{
				if (token.IsCancellationRequested)
				{
					return;
				}
				if ((DateTime.Now - startTime).TotalSeconds > 10)
				{
					logger?.Warn($"WaitUntilStarted timeout after 10 seconds.");
					return;
				}
				await Task.Delay(33, token);
			}
		}

		public void Tick()
		{
			viewController?.Tick();
		}

		public void Dispose()
		{
			if (screen != null)
			{
				screen.HandleReleased -= Screen_HandleReleased;
				UnityEngine.Object.Destroy(screen);
				screen = null;
			}
			if (viewController != null)
			{
				UnityEngine.Object.Destroy(viewController);
				viewController = null;
			}
		}
	}
}
