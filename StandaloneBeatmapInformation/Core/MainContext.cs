using IPA.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StandaloneBeatmapInformation.Configuration;
using SiraUtil.Logging;

namespace StandaloneBeatmapInformation.Core
{
	internal record AdditionalText(
		string RequestState,
		string Requester
		)
	{
		public static AdditionalText Empty { get; } = new AdditionalText(string.Empty, string.Empty);
	}
	internal class MainContext
	{
		private Logger? logger;
		public PluginConfig Config { get; init; }

		public MainContext(SiraLog logger, PluginConfig config)
		{
			this.logger = logger.Logger;
			Config = config;
		}

		public async Task<AdditionalText> GetCurrentAdditionalTextFromFile(string songHash)
		{
			var info = await Task.Run(() =>
			{
				Task.Delay(54);
				var conf = new BS_Utils.Utilities.Config("StandaloneBeatmapInformation/additional.ini");
				var section = "Additional";
				var state = conf.GetString(section, "RequestState");
				var targetSongHash = conf.GetString(section, "TargetSongHash");
				var rawRequester = conf.GetString(section, "TargetSongRequester");
				var requester = GetTextIfMatched(songHash, targetSongHash, rawRequester) ?? string.Empty;
				return new AdditionalText(state, requester);
			});
			return info;
		}

		public AdditionalText GetCurrentAdditionalTextFromInterface(string songHash)
		{
			var state = AdditionalInformationInterface.RequestState;
			var requester = GetTextIfMatched(
				songHash,
				AdditionalInformationInterface.TargetSongHash,
				AdditionalInformationInterface.TargetSongRequester) ?? string.Empty;
			return new AdditionalText(state, requester);
		}

		private string? GetTextIfMatched(string currentSongHash, string targetSongHash, string rawText)
		{
			if (string.IsNullOrWhiteSpace(targetSongHash))
			{
				logger?.Debug($"Registered song hash ({targetSongHash}) not valid.");
				return null;
			}
			if (currentSongHash.ToLower() != targetSongHash.ToLower())
			{
				logger?.Info($"Current song hash ({currentSongHash}) and registered ({targetSongHash}) not matched.");
				return null;
			}
			return rawText;
		}
	}
}
