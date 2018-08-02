using System;
using PanCardView.Enums;
namespace PanCardView.Utility
{
	public struct InteractionItem
	{
		public Guid Id { get; set; }
		public InteractionType Type { get; set; }
		public InteractionState State { get; set; }
	}
}
