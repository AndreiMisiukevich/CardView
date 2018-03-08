using Android.Graphics;
using Xamarin.Forms.Platform.Android;
using Android.Content;
using XFrameRenderer = Xamarin.Forms.Platform.Android.AppCompat.FrameRenderer;
using Xamarin.Forms;
using PanCardView.Controls;
using PanCardView.Droid;
using Android.Runtime;

[assembly: ExportRenderer(typeof(IndicatorItemView), typeof(IndicatorItemViewRenderer))]
namespace PanCardView.Droid
{
    [Preserve(AllMembers = true)]
    public class IndicatorItemViewRenderer : XFrameRenderer
    {
        public IndicatorItemViewRenderer(Context context) : base(context)
        {
        }

		public override void Draw(Canvas canvas)
		{
			base.Draw(canvas);

			if (Element == null || Element.OutlineColor.A <= 0)
			{
				return;
			}

			using (var paint = new Paint { AntiAlias = true })
			using (var path = new Path())
			using (var direction = Path.Direction.Cw)
			using (var style = Paint.Style.Stroke)
			using (var rect = new RectF(0, 0, canvas.Width, canvas.Height))
			{
				var rx = Android.App.Application.Context.ToPixels(Element.CornerRadius);
				var ry = Android.App.Application.Context.ToPixels(Element.CornerRadius);
				path.AddRoundRect(rect, rx, ry, direction);
				paint.StrokeWidth = Context.Resources.DisplayMetrics.Density;
				paint.SetStyle(style);
				paint.Color = Element.OutlineColor.ToAndroid();
				canvas.DrawPath(path, paint);
			}
		}
    }
}
