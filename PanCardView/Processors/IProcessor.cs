using System.Threading.Tasks;
using PanCardView.Utility;

namespace PanCardView.Processors
{
    public interface IProcessor
    {
        void Init(CardsView cardsView, params ProcessorItem[] items);
        void Change(CardsView cardsView, double value, params ProcessorItem[] items);
        Task Navigate(CardsView cardsView, params ProcessorItem[] items);
        Task Reset(CardsView cardsView, params ProcessorItem[] items);
        Task Proceed(CardsView cardsView, params ProcessorItem[] items);
        void Clean(CardsView cardsView, params ProcessorItem[] items);
    }
}
