# CardsView and CarouselView controls for Xamarin Forms (based on Xamarin Forms AbsoluteLayout)

## Setup
* Available on NuGet: [CardsView](http://www.nuget.org/packages/CardsView) [![NuGet](https://img.shields.io/nuget/v/CardsView.svg?label=NuGet)](https://www.nuget.org/packages/CardsView)
* Add nuget package to your Xamarin.Forms .netStandard/PCL project and to your platform-specific projects

|Platform|Version|
| ------------------- | ------------------- |
|Xamarin.iOS|8.0+|
|Xamarin.Android|15+|
|Windows 10 UWP|10.0.10240+|

## CardsView
This plugin provides opportunity to create swipeable CardsView in Xamarin.Forms applications like Tinder app has.

![Sample GIF](https://media.giphy.com/media/3oFzlV5tQhF1udDxIY/giphy.gif)

## CarouselView
You are able to setup CarouselView control, that is based on CardsView

![Sample GIF](https://media.giphy.com/media/du0akXCuO8BTHzBuat/giphy.gif)

## Custom Animations
You are able to create custom animations, just implement ICardProcessor or extend created processors (change animation speed or type)
https://github.com/AndreiMisiukevich/CardView/tree/master/PanCardView/Processors

## Samples
The sample you can find here https://github.com/AndreiMisiukevich/CardView/tree/master/PanCardViewSample

**C#:**

-> Create CardsView and setup it
```csharp
var cardsView = new CardsView
{
    ItemTemplate = new DataTemplate(() => new ContentView()) //your template
};
cardsView.SetBinding(CardsView.ItemsProperty, nameof(PanCardSampleViewModel.Items));
cardsView.SetBinding(CardsView.CurrentIndexProperty, nameof(PanCardSampleViewModel.CurrentIndex));
```
-> Optionaly you can create ViewModel... or not... as you wish

-> Indicators bar (For CarouselView, perhaps). It's easy to add indicators
-> Just add IndicatorsControl into your carouselView as a child view.

```csharp
carouselView.Children.Add(new IndicatorsControl());
```

**XAML:**
```xml
<cards:CarouselView 
    Items="{Binding Items}"
    CurrentIndex="{Binding CurrentIndex}">
    <cards:CarouselView.ItemTemplate>
        <DataTemplate>
            <ContentView>
                <Frame 
                    VerticalOptions="Center"
                    HorizontalOptions="Center"
                    HeightRequest="300"
                    WidthRequest="300"
                    Padding="0" 
                    HasShadow="false"
                    IsClippedToBounds="true"
                    CornerRadius="10"
                    BackgroundColor="{Binding Color}">
                    
                    <Image Source="{Binding Source}"/> 
                    
                </Frame>
            </ContentView>
        </DataTemplate>
    </cards:CarouselView.ItemTemplate>

    <controls:IndicatorsControl/>
</cards:CarouselView>
```
Also you are able to manage IndicatorsControl appearing/disappearing. For example if user doesn't select new page during N miliseconds, the indicators will disappear. Just set ToFadeDuration = 2000 (2 seconds delay before disappearing)

if you want to add items directly through xaml

``` xml
...
    <cards:CarouselView.Items>
            <x:Array Type="{x:Type View}">
                <ContentView>
                    <Image Source="yourImage.png"/>
                </ContentView>
                <RelativeLayout>
                    <Button Text="Click" />
                </RelativeLayout>
                <StackLayout>
                    <Label Text="any text"/>
                </StackLayout>
            </x:Array>
    </cards:CarouselView.Items>
...
```


-> If you want to customize indicators, you need set *SelectedIndicatorStyle* and/or *UnselectedIndicatorStyle*, or you are able to extend this class and override several methods.
Also you can customize position of indicators (You need to set Rotation / AbsoluteLayout Flags and Bounds etc.)

This class is describing default indicators styles (each default indicator item is Frame)
https://github.com/AndreiMisiukevich/CardView/blob/master/PanCardView/Styles/DefaultIndicatorItemStyles.cs

## Workarounds

-> If you want to put your cardsView/carouselView INTO scroll view, you should to use *ParentScrollView* instead of Xamarin.Forms.ScrollView.

-> If you want to put cardsView/carouselView INTO ListView or INTO any another scrollable view you should follow these steps
1) Create your own class and implement IOrdinateHandlerParentView interface (It's needed only for iOS, but do it into shared project)
2) Create the renderer for this class (For Android)

Check these classes (I implemented it for ParentScrollView. You can use it as example, nothing difficult :))
https://github.com/AndreiMisiukevich/CardView/blob/master/PanCardView/Controls/ParentScrollView.cs
https://github.com/AndreiMisiukevich/CardView/blob/master/PanCardView.Droid/ParentScrollViewRenderer.cs

Check source code for more info, or 🇧🇾 ***just ask me =)*** 🇧🇾

## License
The MIT License (MIT) see [License file](LICENSE)

## Contribution
Feel free to create issues and PRs 😃

