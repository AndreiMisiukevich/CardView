
namespace PanCardView.Factory
{
    public class CardViewItemFactory
    {
        private readonly CardViewFactoryRule _defaultRule;

        public CardViewItemFactory() : this(null)
        {
        }

        public CardViewItemFactory(CardViewFactoryRule defaultRule)
        => _defaultRule = defaultRule;

        public virtual CardViewFactoryRule GetRule(object context) 
        => _defaultRule;
    }
}
