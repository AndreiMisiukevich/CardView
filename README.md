## NuGet
* [CardsView](http://www.nuget.org/packages/CardsView) [![NuGet](https://img.shields.io/nuget/v/CardsView.svg?label=NuGet)](https://www.nuget.org/packages/CardsView)

Add nuget package to your Xamarin.Forms PCL project and to your platform-specific projects.

|Platform|
| ------------------- |
|Xamarin.iOS|
|Xamarin.Android|
|Windows 10 UWP|

## CardsView
This plugin provides opportunity to create swipeable CardsView in Xamarin.Forms applications like Tinder app has
Just add nuget package into your PCL project

![Sample GIF](https://media.giphy.com/media/3oFzlV5tQhF1udDxIY/giphy.gif)

## CarouselView
You are able to setup CarouselView control, that is based on CardsView

![Sample GIF](https://media.giphy.com/media/du0akXCuO8BTHzBuat/giphy.gif)

## Samples
The sample you can find here https://github.com/AndreiMisiukevich/CardView/tree/master/PanCardViewSample

-> Create CardsView and setup it
```csharp
var cardsView = new CardsView
{
    DataTemplate = new DataTemplate(() => new ContentView()) //your template
};
cardsView.SetBinding(CardsView.ItemsProperty, nameof(PanCardSampleViewModel.Items));
cardsView.SetBinding(CardsView.CurrentIndexProperty, nameof(PanCardSampleViewModel.CurrentIndex));
```
-> Optionaly you can create ViewModel... or not... as you wish

-> Indicators bar (For CarouselView, perhaps). It's easy to add indicators
-> Just add IndicatorsControl into your view as a child view.

```csharp
cardsView.Children.Add(new IndicatorsControl());
```
-> If you want to customize indicators, you need to extend IndicatorsView class and override a few methods. 
Also you can choose position for indicators (You need to set Rotation / AbsoluteLayout Flags and Bounds etc.)
Check source code for more info, or just ask me =)

## License
The MIT License (MIT) see [License file](LICENSE)

## Contribution
Feel free to create issues and PRs 😃

