using PanCardView.Enums;

namespace PanCardView.EventArgs
{
	public class UserInteractedEventArgs
	{
		public UserInteractedEventArgs(int index, double diff, UserInteractionStatus status)
		{
			Index = index;
			Diff = diff;
			Status = status;
		}

		public int Index { get; }
		public double Diff { get; }
		public UserInteractionStatus Status { get; }
	}
}
