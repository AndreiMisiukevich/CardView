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

		public InteractionItem GetFirstItem(InteractionType type = InteractionType.User | InteractionType.Auto, InteractionState state = InteractionState.Regular | InteractionState.Removing)
		{
			lock(_queueLocker)
			{
				return _queue.FirstOrDefault(i => type.HasFlag(i.Type) && state.HasFlag(i.State));
			}
		}

		public void Add(Guid id, InteractionType type, InteractionState state = InteractionState.Regular)
		{
			lock (_queueLocker)
			{
				_queue.Add(new InteractionItem
				{
					Id = id,
					Type = type,
                    State = state
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

		public bool CheckLastId(Guid id, InteractionType type = InteractionType.User | InteractionType.Auto)
		{
			lock(_queueLocker)
			{
				return _queue.LastOrDefault(i => type.HasFlag(i.Type)).Id == id;
			}
		}
	}
}

