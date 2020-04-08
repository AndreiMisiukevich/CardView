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

        private static Assembly ResourceAssembly => typeof(ResourcesInfo).GetTypeInfo().Assembly;

        public static ImageSource WhiteRightArrowImageSource => FromResource(WhiteRightArrowResourceName);

        public static ImageSource WhiteLeftArrowImageSource => FromResource(WhiteLeftArrowResourceName);

        public static ImageSource BlackRightArrowImageSource => FromResource(BlackRightArrowResourceName);

        public static ImageSource BlackLeftArrowImageSource => FromResource(BlackLeftArrowResourceName);

        private static ImageSource FromResource(string resourceName)
            => ImageSource.FromResource($"{ResourcesPath}.{resourceName}", ResourceAssembly);
    }
}
