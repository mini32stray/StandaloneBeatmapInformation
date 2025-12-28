using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Zenject;

namespace StandaloneBeatmapInformation.Core.ComponentModel
{
	internal class HostBase : ITickable, INotifyPropertyChanged
	{
		protected ConcurrentQueue<string?> notifiedProperties = new();
		public event PropertyChangedEventHandler? PropertyChanged;

		public void Tick()
		{
			if (!notifiedProperties.IsEmpty)
			{
				if (notifiedProperties.TryDequeue(out var prop))
				{
					OnPropertyChanged(new PropertyChangedEventArgs(prop));
				}
			}
		}

		protected void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
		{
			notifiedProperties.Enqueue(propertyName);
		}

		protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
		{
			PropertyChanged?.Invoke(this, args);
		}
	}
}
