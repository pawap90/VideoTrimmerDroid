using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace VideoTrimmer.Droid.Helpers
{
    public static class ImageHelper
    {
        public static Bitmap ScaleDown(Bitmap realImage, float maxImageSize, bool filter)
        {
            float ratio = Math.Min(maxImageSize / realImage.Width, maxImageSize / realImage.Height);

            int width = Convert.ToInt32(Math.Round(ratio * realImage.Width));
            int height = Convert.ToInt32(Math.Round(ratio * realImage.Height));

            Bitmap newBitmap = Bitmap.CreateScaledBitmap(realImage, width, height, filter);
            return newBitmap;
        }
    }
}