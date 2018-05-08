using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace VideoTrimmer.Droid.View
{
    [Register("VideoTrimmer.Droid.View.TimelineView")]
    public class TimelineView : Android.Views.View
    {
        private Android.Net.Uri mVideoUri;
        private int mHeightView;
        private List<Bitmap> mBitmapList = null;

        public TimelineView(Context context) : base(context)
        {
            Init();
        }

        public TimelineView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Init();
        }

        public TimelineView(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            Init();
        }

        protected TimelineView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            Init();
        }

        public TimelineView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            Init();
        }

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);
            if (mBitmapList != null)
            {
                canvas.Save();
                int x = 0;

                foreach (var bitmap in mBitmapList)
                {
                    if (bitmap != null)
                    {
                        canvas.DrawBitmap(bitmap, x, 0, null);
                        x = x + bitmap.Width;
                    }
                }
            }
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            int minW = PaddingLeft + PaddingRight + SuggestedMinimumWidth;
            int w = ResolveSizeAndState(minW, widthMeasureSpec, 1);

            int minH = PaddingBottom + PaddingTop + mHeightView;
            int h = ResolveSizeAndState(minH, heightMeasureSpec, 1);

            SetMeasuredDimension(w, h);
        }

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);

            if (w != oldw)
            {
                GetVideoFrames(w);
            }
        }

        public void SetVideo(Android.Net.Uri data)
        {
            mVideoUri = data;
        }

        private void Init()
        {
            mBitmapList = new List<Bitmap>();
            mHeightView = Resources.GetDimensionPixelSize(Resource.Dimension.frames_video_maxsize);
        }

        private void GetVideoFrames(int viewWidth)
        {
            MediaMetadataRetriever mediaMetadataRetriever = new MediaMetadataRetriever();

            try
            {
                mediaMetadataRetriever.SetDataSource(mVideoUri.ToString(), new Dictionary<string, string>());

                // Retrieve media data
                long videoLengthInMs = Convert.ToInt64(mediaMetadataRetriever.ExtractMetadata(MetadataKey.Duration)) * 1000;

                // Set thumbnail properties (Thumbs are squares)
                int thumbWidth = mHeightView;
                int thumbHeight = mHeightView;

                int numThumbs = (int)Math.Ceiling(((float)viewWidth) / thumbWidth);

                long interval = videoLengthInMs / numThumbs;

                for (int i = 0; i < numThumbs; ++i)
                {
                    Bitmap bitmap = mediaMetadataRetriever.GetFrameAtTime(i * interval, Android.Media.Option.ClosestSync);
                    bitmap = Bitmap.CreateScaledBitmap(bitmap, thumbWidth, thumbHeight, false);
                    mBitmapList.Add(bitmap);
                }
            }
            catch (Exception ex) {
                Log.Error("Error", ex.ToString());
            }
            finally
            {
                mediaMetadataRetriever.Release();
            }
        }
    }
}