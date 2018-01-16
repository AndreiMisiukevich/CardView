## CardsView
This plugin provides opportunity to create swipeable CardsView in Xamarin.Forms applications like in Tinder app has
Just add nuget package into your PCL project

something like this

[![Sample GIF](https://media.giphy.com/media/3oFzlV5tQhF1udDxIY/giphy.gif)

## Android additional setup
Write next line of code in your MainActivity

-> CardsViewRenderer.Init();

## NuGet
* [CardsView](http://www.nuget.org/packages/CardsView) [![NuGet](https://img.shields.io/nuget/v/CardsView.svg?label=NuGet)](https://www.nuget.org/packages/CardsView)

## Samples
The sample you can find here https://github.com/AndreiMisiukevich/CardView/tree/master/PanCardViewSample

-> Create CardsView and setup it
```csharp
var cardsView = new CardsView
{
    ItemViewFactory = new CardViewItemFactory(RuleHolder.Rule)
};
cardsView.SetBinding(CardsView.ItemsProperty, nameof(PanCardSampleViewModel.Items));
cardsView.SetBinding(CardsView.CurrentIndexProperty, nameof(PanCardSampleViewModel.CurrentIndex));
```

-> Create ViewRule and setup CardViewItemFactory (something like DataTemplate), or create subclass from CardViewItemFactory and manage several rules (like DataTemplateSelector)

```csharp
public static class RuleHolder
{
    public static CardViewFactoryRule Rule { get; } = new CardViewFactoryRule
    {
        Creator = () =>
        {
            var content = new AbsoluteLayout();
            var frame = new Frame
            {
                Padding = 0,
                HasShadow = false,
                CornerRadius = 10,
                IsClippedToBounds = true
            };
            content.Children.Add(frame, new Rectangle(.5, .5, 300, 300), AbsoluteLayoutFlags.PositionProportional);

            var image = new CachedImage
            {
                Aspect = Aspect.AspectFill
            };
            image.SetBinding(CachedImage.SourceProperty, "Source");

            frame.Content = image;
            return content;
        }
    };
}
```

-> Optionaly create ViewModel =)

## License
The MIT License (MIT) see [License file](LICENSE)

## Contribution
Feel free to create issues and PRs 😃

