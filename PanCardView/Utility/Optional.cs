
namespace PanCardView.Utility
{
    public sealed class Optional<TValue> where TValue : class
    {
        public Optional(TValue value)
            => Value = value;

        public TValue Value { get; }
    }
}
