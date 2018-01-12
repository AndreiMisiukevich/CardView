// 21(c) Andrei Misiukevich
using System;
namespace PanCardView
{
    public abstract class CardViewItemFactory
    {
        public abstract CardViewFactoryRule GetRule(object context);
    }
}
