using System;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using PanCardView.Controls;
using PanCardView.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using XFrameRenderer = Xamarin.Forms.Platform.Android.AppCompat.FrameRenderer;

[assembly: ExportRenderer(typeof(IndicatorItemView), typeof(IndicatorItemViewRenderer))]
namespace PanCardView.Droid
{
	[Preserve(AllMembers = true)]
	public class IndicatorItemViewRenderer : XFrameRenderer
	{

		[Obsolete("For Forms <= 2.4")]
		public IndicatorItemViewRenderer()
		{
		}

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
				var raduis = Android.App.Application.Context.ToPixels(Element.CornerRadius);
				path.AddRoundRect(rect, raduis, raduis, direction);
				paint.StrokeWidth = Context.Resources.DisplayMetrics.Density * 2;
				paint.SetStyle(style);
				paint.Color = Element.OutlineColor.ToAndroid();
				canvas.DrawPath(path, paint);
			}
		}
	}
}