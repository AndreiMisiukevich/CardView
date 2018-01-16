using Xamarin.Forms;
using System.Threading.Tasks;

namespace PanCardView.Processors
{
    public interface ICardProcessor
    {
        void InitView(View view);
        void HandlePanChanged(View view, double xPos);
        Task HandlePanReset(View view);
        Task HandlePanApply(View view);
    }
}
