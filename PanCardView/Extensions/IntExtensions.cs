using System;
namespace PanCardView.Extensions
{
    public static class IntExtensions
    {
        public static int ToRecycledIndex(this int index, int itemsCount)
        {
            if (itemsCount <= 0)
            {
                return -1;
            }

            if (index < 0)
            {
                while (index < 0)
                {
                    index += itemsCount;
                }
                return index;
            }

            while (index >= itemsCount)
            {
                index -= itemsCount;
            }
            return index;
        }
    }
}
