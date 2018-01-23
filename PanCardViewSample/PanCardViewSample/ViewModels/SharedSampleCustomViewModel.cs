using System.Collections.ObjectModel;
using System.ComponentModel;
using Xamarin.Forms;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Linq;

namespace PanCardViewSample.ViewModels
{
    public sealed class SharedSampleCustomViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int _ImageCount = 500;

        private Stack<object> _contextStack = new Stack<object>();

        public SharedSampleCustomViewModel()
        {
            PrevContext = CreateContext();
            CurrentContext = CreateContext();
            NextContext = CreateContext();

            _contextStack.Push(PrevContext);

            PanStartedCommand = new Command(() =>
            {
                if (_contextStack.Any() && PrevContext == null)
                {
                    PrevContext = _contextStack.Peek();
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PrevContext)));
                }
                if (NextContext == null)
                {
                    NextContext = new { Source = CreateSource(), Color = CreateColor() };
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NextContext)));
                }

            });

            PanPositionChangedCommand = new Command((p) =>
            {
                var isNext = (bool)p;
                if(isNext)
                {
                    _contextStack.Push(CurrentContext);
                    PrevContext = CurrentContext;
                    CurrentContext = NextContext;
                    NextContext = CreateContext();
                }
                else
                {
                    NextContext = CurrentContext;
                    CurrentContext = PrevContext;
                    _contextStack.Pop();
                    PrevContext = _contextStack.Any() ? _contextStack.Peek() : null;
                }

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentContext)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NextContext)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PrevContext)));
            });
        }

        public object CurrentContext { get; set; }
        public object NextContext { get; set; }
        public object PrevContext { get; set; }

        public ICommand PanStartedCommand { get; } 
        public ICommand PanPositionChangedCommand { get; } 


        private object CreateContext()
        {
            return new { Source = CreateSource(), Color = CreateColor() };
        }

        private string CreateSource()
        {
            return $"http://lorempixel.com/300/300/animals/text{++_ImageCount}/";
        }
            
        private Color CreateColor()
        {
            var rnd = new Random();
            return Color.FromRgb(rnd.Next(0, 255), rnd.Next(0, 255), rnd.Next(0, 255));
        }
    }
}
