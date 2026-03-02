using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StandaloneBeatmapInformation.Core
{
	public static class AdditionalInformationInterface
	{
		public static string RequestState { get; set; } = "!bsr state uncertain";
		public static string TargetSongHash { get; set; } = string.Empty;
		public static string TargetSongRequester { get; set; } = string.Empty;
		public static string CamScriptAuthor { get; set; } = string.Empty;
	}
}
