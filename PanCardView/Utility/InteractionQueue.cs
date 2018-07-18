using System;
using PanCardView.Enums;
using System.Collections.Generic;
using System.Linq;

namespace PanCardView.Utility
{
	public sealed class InteractionQueue 
	{
		private readonly List<InteractionItem> _queue = new List<InteractionItem>();
		private readonly object _queueLocker = new object();

		public Guid GetFirstId(InteractionType type = InteractionType.Gesture | InteractionType.Animation)
		{
			lock(_queueLocker)
			{
				return _queue.FirstOrDefault(i => type.HasFlag(i.Type)).Id;
			}
		}

		public void Add(Guid id, InteractionType type)
		{
			lock (_queueLocker)
			{
				_queue.Add(new InteractionItem
				{
					Id = id,
					Type = type
				});
			}
		}

		public void Remove(Guid id)
		{
			lock (_queueLocker)
			{
				var item = _queue.FirstOrDefault(i => i.Id == id);
				_queue.Remove(item);
			}
		}

		public bool CheckLastId(Guid id, InteractionType type = InteractionType.Gesture | InteractionType.Animation)
		{
			lock(_queueLocker)
			{
				return _queue.LastOrDefault(i => type.HasFlag(i.Type)).Id == id;
			}
		}
	}
}

