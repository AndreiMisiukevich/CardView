using PanCardView.Controls;
using Xamarin.Forms;
using System.Collections;
using System;

namespace PanCardView.Extensions
{
    public static class CardViewExtensions
    {
        public static double GetSize(this CardsView cardsView, View card = null)
        => cardsView.IsHorizontalOrientation ? (card ?? cardsView).Width : (card ?? cardsView).Height;

        public static CardsView AsCardsView(this BindableObject bindable)
        => bindable as CardsView;

        public static CarouselView AsCarouselView(this BindableObject bindable)
        => bindable as CarouselView;
            
        public static CoverFlowView AsCoverFlowView(this BindableObject bindable)
        => bindable as CoverFlowView;

        public static IndicatorsControl AsIndicatorsControl(this BindableObject bindable)
        => bindable as IndicatorsControl;

        public static CircleFrame AsCircleFrame(this BindableObject bindable)
        => bindable as CircleFrame;

        public static ArrowControl AsArrowControl(this BindableObject bindable)
        => bindable as ArrowControl;

        public static TabsControl AsTabsView(this BindableObject bindable)
        => bindable as TabsControl;

        public static View CreateView(this DataTemplate template)
        {
            var content = template.CreateContent();
            return content is ViewCell cell
                ? cell.View
                : content as View;
        }

        public static DataTemplate SelectTemplate(this DataTemplate template, object context)
        {
            while (template is DataTemplateSelector selector)
            {
                template = selector.SelectTemplate(context, null);
            }
            return template;
        }

        public static int ToCyclicalIndex(this int index, int itemsCount)
        {
            if (itemsCount <= 0)
            {
                return -1;
            }

            var reminder = index % itemsCount;
            return reminder >= 0
                ? reminder
                : reminder + itemsCount;
        }

        public static int FindIndex(this IEnumerable collection, object value)
        {
            if (collection is IList list)
            {
                return list.IndexOf(value);
            }
            var searchIndex = 0;
            foreach (var item in collection)
            {
                if (item == value)
                {
                    return searchIndex;
                }
                ++searchIndex;
            }
            return -1;
        }

        public static object FindValue(this IEnumerable collection, int index)
        {
            if (collection is IList list)
            {
                return list[index];
            }
            var searchIndex = 0;
            foreach (var item in collection)
            {
                if (searchIndex == index)
                {
                    return item;
                }
                ++searchIndex;
            }
            throw new IndexOutOfRangeException();
        }

        public static int Count(this IEnumerable collection)
        {
            if (collection is ICollection list)
            {
                return list.Count;
            }
            var searchIndex = 0;
            foreach (var item in collection)
            {
                ++searchIndex;
            }
            return searchIndex;
        }
    }
}
