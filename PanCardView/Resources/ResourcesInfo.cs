using Xamarin.Forms;
using System.Reflection;

namespace PanCardView.Resources
{
    public static class ResourcesInfo
    {
        private const string ResourcesPath = "PanCardView.Resources";
        private const string WhiteRightArrowResourceName = "rightArrow.png";
        private const string WhiteLeftArrowResourceName = "leftArrow.png";
        private const string BlackRightArrowResourceName = "rightArrowBlack.png";
        private const string BlackLeftArrowResourceName = "leftArrowBlack.png";

        private static Assembly ResourceAssembly => typeof(ResourcesInfo).Assembly;

        public static ImageSource WhiteRightArrowImageSource
            => ImageSource.FromResource($"{ResourcesPath}.{WhiteRightArrowResourceName}", ResourceAssembly);

        public static ImageSource WhiteLeftArrowImageSource
            => ImageSource.FromResource($"{ResourcesPath}.{WhiteLeftArrowResourceName}", ResourceAssembly);

        public static ImageSource BlackRightArrowImageSource
            => ImageSource.FromResource($"{ResourcesPath}.{BlackRightArrowResourceName}", ResourceAssembly);

        public static ImageSource BlackLeftArrowImageSource
            => ImageSource.FromResource($"{ResourcesPath}.{BlackLeftArrowResourceName}", ResourceAssembly);
    }
}
