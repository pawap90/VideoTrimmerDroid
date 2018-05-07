using Android.App;
using Android.Widget;
using Android.OS;
using Android.Views;
using System;
using Android.Graphics;
using Android.Media;
using System.Collections.Generic;
using Android.Util;
using VideoTrimmer.Droid.View;
using System.Threading;
using Java.Lang;

namespace VideoTrimmer.Droid
{
    [Activity(Label = "VideoTrimmer.Droid", MainLauncher = true)]
    public class MainActivity : Activity, Android.Views.View.IOnTouchListener
    {
        VideoView videoView;
        RelativeLayout videoViewLayout;
        TimelineView timelineView;
        Android.Net.Uri uri;

        private ImageView imageViewArrowLeft;
        private ImageView imageViewArrowRight;
        private ImageView imageViewArrowPosition;
        private ViewGroup mRootLayout;

        private int videoHeight;
        private int videoWidth;
        private long videoDuration;
        private int timelineWidth;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main2);

            Initialize();
        }

        private void Initialize()
        {
            // Video view.
            videoView = FindViewById<VideoView>(Resource.Id.videoView);
            videoViewLayout = FindViewById<RelativeLayout>(Resource.Id.layout_video_view);

            uri = Android.Net.Uri.Parse("https://raw.githubusercontent.com/mediaelement/mediaelement-files/master/big_buck_bunny.mp4");

            videoView.SetVideoURI(uri);
            videoView.Visibility = ViewStates.Visible;

            var btnStart = FindViewById<Button>(Resource.Id.btn_start);
            btnStart.Click += VideoStart;

            var btnPause = FindViewById<Button>(Resource.Id.btn_pause);
            btnPause.Click += VideoPause;

            var btnStop = FindViewById<Button>(Resource.Id.btn_stop);
            btnStop.Click += VideoStop;

            // Timeline view
            timelineView = FindViewById<TimelineView>(Resource.Id.timeline_video);

            // Range
            mRootLayout = FindViewById<ViewGroup>(Resource.Id.layout_range);
            imageViewArrowLeft = mRootLayout.FindViewById<ImageView>(Resource.Id.range_arrow_left);
            imageViewArrowRight = mRootLayout.FindViewById<ImageView>(Resource.Id.range_arrow_right);
            imageViewArrowPosition = mRootLayout.FindViewById<ImageView>(Resource.Id.position_arrow);

            MediaMetadataRetriever mediaMetadataRetriever = new MediaMetadataRetriever();
            mediaMetadataRetriever.SetDataSource(uri.ToString(), new Dictionary<string, string>());
            this.videoHeight = Convert.ToInt32(mediaMetadataRetriever.ExtractMetadata(MetadataKey.VideoHeight));
            this.videoWidth = Convert.ToInt32(mediaMetadataRetriever.ExtractMetadata(MetadataKey.VideoWidth));
            this.videoDuration = Convert.ToInt64(mediaMetadataRetriever.ExtractMetadata(MetadataKey.Duration)) * 1000;
        }

        #region VideoControls
        private async void VideoStart(object sender, EventArgs args)
        {
            videoView.Start();

            while (videoView.IsPlaying)
            {
                await System.Threading.Tasks.Task.Delay(500);
                RunOnUiThread(() =>
                {
                    UpdateArrowPosition(imageViewArrowPosition, TimeToPosition(videoView.CurrentPosition));
                });
            }

        }

        private void VideoPause(object sender, EventArgs args)
        {
            videoView.Pause();
        }

        private void VideoStop(object sender, EventArgs args)
        {
            videoView.SeekTo(0);
            videoView.Pause();
        }
        #endregion

        private void SetupLayout()
        {
            
            RelativeLayout.LayoutParams layoutParamsPositionArrow = new RelativeLayout.LayoutParams(75, 75);
            layoutParamsPositionArrow.AddRule(LayoutRules.Below, Resource.Id.layout_video_view);
            imageViewArrowPosition.LayoutParameters = layoutParamsPositionArrow;
            imageViewArrowPosition.SetOnTouchListener(this);

            var layoutParamsTimeline = (RelativeLayout.LayoutParams)timelineView.LayoutParameters;
            layoutParamsTimeline.SetMargins(10, 10, 10, 10);
            layoutParamsTimeline.AddRule(LayoutRules.Below, Resource.Id.position_arrow);
            timelineView.LayoutParameters = layoutParamsTimeline;

            int timelineHeight = 0;
            if (this.videoHeight < this.videoWidth)
                timelineHeight = (this.videoHeight * Resources.GetDimensionPixelOffset(Resource.Dimension.frames_video_maxsize)) / this.videoWidth;
            else
                timelineHeight = (this.videoWidth * Resources.GetDimensionPixelOffset(Resource.Dimension.frames_video_maxsize)) / this.videoHeight;

            this.timelineWidth = timelineView.Width - (layoutParamsTimeline.LeftMargin + layoutParamsTimeline.RightMargin);

            RelativeLayout.LayoutParams layoutParamsLeftArrow = new RelativeLayout.LayoutParams(75, 75);
            layoutParamsLeftArrow.AddRule(LayoutRules.Below, Resource.Id.layout_video_view);
            layoutParamsLeftArrow.TopMargin = layoutParamsPositionArrow.Height + timelineHeight + layoutParamsTimeline.TopMargin + layoutParamsTimeline.BottomMargin;
            imageViewArrowLeft.LayoutParameters = layoutParamsLeftArrow;
            imageViewArrowLeft.SetOnTouchListener(this);

            RelativeLayout.LayoutParams layoutParamsRightArrow = new RelativeLayout.LayoutParams(75, 75);
            layoutParamsRightArrow.LeftMargin = (imageViewArrowRight.Parent as RelativeLayout).Width - imageViewArrowRight.Width;
            layoutParamsRightArrow.AddRule(LayoutRules.Below, Resource.Id.layout_video_view);
            layoutParamsRightArrow.TopMargin = layoutParamsPositionArrow.Height + timelineHeight + layoutParamsTimeline.TopMargin + layoutParamsTimeline.BottomMargin;
            imageViewArrowRight.LayoutParameters = layoutParamsRightArrow;
            imageViewArrowRight.SetOnTouchListener(this);

            timelineView.SetVideo(uri);
        }

        public override void OnWindowFocusChanged(bool hasFocus)
        {
            SetupLayout();
        }

        public bool OnTouch(Android.Views.View v, MotionEvent e)
        {
            var imageView = (ImageView)v;
            var layoutParams = (ViewGroup.MarginLayoutParams)v.LayoutParameters;

            switch (e.Action)
            {
                case MotionEventActions.Move:
                    int x_cord = (int)e.RawX;

                    if (!CheckCollision(imageView, x_cord))
                        MoveArrows(imageView, x_cord);
                    break;
            }

            mRootLayout.Invalidate();
            return true;
        }

        private bool CheckCollision(ImageView imageView, int newX)
        {
            if (newX >= mRootLayout.Width - imageView.Width)
                return true;
            switch (imageView.Id)
            {
                case Resource.Id.range_arrow_left:
                    if (newX > imageViewArrowRight.Left)
                        return true;
                    break;
                case Resource.Id.range_arrow_right:
                    if (newX < imageViewArrowLeft.Left)
                        return true;
                    break;
                case Resource.Id.position_arrow:
                    if (newX < imageViewArrowLeft.Left || newX > imageViewArrowRight.Left)
                        return true;
                    break;
            }

            return false;
        }

        private void MoveArrows(ImageView imageView, int newX)
        {
            var currentLayoutParams = (ViewGroup.MarginLayoutParams)imageView.LayoutParameters;

            switch (imageView.Id)
            {
                case Resource.Id.range_arrow_left:
                    if (newX > imageViewArrowPosition.Left)
                    {
                        var positionLayoutParams = (ViewGroup.MarginLayoutParams)imageViewArrowPosition.LayoutParameters;
                        positionLayoutParams.LeftMargin = newX;
                        imageViewArrowPosition.LayoutParameters = positionLayoutParams;
                        UpdateVideoPosition(newX);
                    }

                    break;
                case Resource.Id.range_arrow_right:
                    if (newX < imageViewArrowPosition.Left)
                    {
                        var positionLayoutParams = (ViewGroup.MarginLayoutParams)imageViewArrowPosition.LayoutParameters;
                        positionLayoutParams.LeftMargin = newX;
                        imageViewArrowPosition.LayoutParameters = positionLayoutParams;
                        UpdateVideoPosition(newX);
                    }
                    break;
                case Resource.Id.position_arrow:
                    UpdateVideoPosition(newX);
                    break;
            }

            currentLayoutParams.LeftMargin = newX;
            imageView.LayoutParameters = currentLayoutParams;
        }

        private void UpdateArrowPosition(ImageView imageView, int newPositionX)
        {
            var currentLayoutParams = (ViewGroup.MarginLayoutParams)imageView.LayoutParameters;
            currentLayoutParams.LeftMargin = newPositionX;
            imageView.LayoutParameters = currentLayoutParams;
        }

        private int TimeToPosition(int currentTime)
        {
            float timePercentage = (float)currentTime / (float)(this.videoDuration/1000);
            return Convert.ToInt32(this.timelineWidth * timePercentage);
        }

        private float PositionPercentage(int newPosition)
        {
            return (float)newPosition / (float)this.timelineWidth;
        }

        private void UpdateVideoPosition(int newPosition)
        {
            videoView.SeekTo(Convert.ToInt32((this.videoDuration/1000) * PositionPercentage(newPosition)));
        }

    }

    
}

