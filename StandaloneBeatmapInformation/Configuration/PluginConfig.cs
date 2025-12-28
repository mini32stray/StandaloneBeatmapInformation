
using System.Runtime.CompilerServices;
using IPA.Config.Stores;
using IPA.Config.Stores.Attributes;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace StandaloneBeatmapInformation.Configuration
{
	internal class PluginConfig
	{
		public virtual bool Enabled { get; set; } = true;
		public virtual bool EnableHandle { get; set; } = false;
		public virtual bool ShowTitle { get; set; } = true;
		public virtual bool ShowSongAuthor { get; set; } = true;
		public virtual bool ShowMapper { get; set; } = true;
		public virtual bool ShowMapId { get; set; } = true;
		public virtual bool ShowStar { get; set; } = true;
		public virtual bool ShowCharacteristic { get; set; } = true;
		public virtual bool ShowDifficulty { get; set; } = true;
		public virtual bool ShowJD { get; set; } = true;
		public virtual bool EnableAdditional { get; set; } = false;

		public class PosRot
		{
			public PosRot()
			{
				PosX = 0f;
				PosY = 1f;
				PosZ = 0f;
				EulerX = 30f;
				EulerY = 0f;
				EulerZ = 0f;
			}

			public float PosX { get; set; }
			public float PosY { get; set; }
			public float PosZ { get; set; }
			public float EulerX { get; set; }
			public float EulerY { get; set; }
			public float EulerZ { get; set; }
		}

		[NonNullable]
		public virtual PosRot Posture { get; set; } = new();

		/// <summary>
		/// This is called whenever BSIPA reads the config from disk (including when file changes are detected).
		/// </summary>
		public virtual void OnReload()
		{
			// Do stuff after config is read from disk.
		}

		/// <summary>
		/// Call this to force BSIPA to update the config file. This is also called by BSIPA if it detects the file was modified.
		/// </summary>
		public virtual void Changed()
		{
			// Do stuff when the config is changed.
		}

		/// <summary>
		/// Call this to have BSIPA copy the values from <paramref name="other"/> into this config.
		/// </summary>
		public virtual void CopyFrom(PluginConfig other)
		{
			// This instance's members populated from other
		}
	}
}
