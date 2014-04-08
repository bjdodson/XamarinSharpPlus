using System;
using MonoTouch.UIKit;
using System.Drawing;
using System.Threading;
using MonoTouch.ObjCRuntime;
using System.Runtime.InteropServices;
using MonoTouch.Foundation;

namespace Core.Util
{

    /// <summary>
    /// iOS "Toast" library, ported from: 
    /// https://github.com/scalessec/toast
    /// </summary>
    public static class Toast
    {
        /*
         *  CONFIGURE THESE VALUES TO ADJUST LOOK & FEEL,
         *  DISPLAY DURATION, ETC.
         */
        // general appearance
        const float CSToastMaxWidth = 0.8f; // 80% of parent view width
        const float CSToastMaxHeight = 0.8f; // 80% of parent view height
        const float CSToastHorizontalPadding = 10.0f;
		const float CSToastVerticalPadding = 10.0f;
        const float CSToastCornerRadius = 10.0f;
        const float CSToastOpacity = 0.8f;
        const float CSToastFontSize = 16.0f;
        const int CSToastMaxTitleLines = 0;
        const int CSToastMaxMessageLines = 0;
        const double CSToastFadeDuration = 0.3;

        // shadow appearance
        const float CSToastShadowOpacity = 0.8f;
        const float CSToastShadowRadius = 6.0f;
        static readonly SizeF CSToastShadowOffset = new SizeF (4.0f, 4.0f);
        const bool CSToastDisplayShadow = true;

        // display duration and position
        const string CSToastDefaultPosition = "bottom";
        const double CSToastDefaultDuration  = 3.5;

        // image view size
        const float CSToastImageViewWidth = 80.0f;
        const float CSToastImageViewHeight = 80.0f;

        // activity
        const float CSToastActivityWidth = 100.0f;
        const float CSToastActivityHeight = 100.0f;
        const string CSToastActivityDefaultPosition = "center";


        // interaction
        const bool CSToastHidesOnTap = true; // excludes activity views

        // associative reference keys
        const string CSToastTimerKey = "CSToastTimerKey";
        const string CSToastActivityViewKey = "CSToastActivityViewKey";

        #region Toast Methods

        public static void MakeToast (this UIView context, string message)
        {
            MakeToast (context, message, CSToastDefaultDuration, CSToastDefaultPosition, null, null);
        }

        public static void MakeToast (this UIView context, string message, double duration, object position, string title, UIImage image)
        {
            UIView toast = ViewForMessage (context, message, title, image);
            ShowToast (context, toast, duration, position);
        }

        public static void ShowToast (this UIView context, UIView toast)
        {
            ShowToast (context, toast, CSToastDefaultDuration, CSToastDefaultPosition);
        }

        public static void ShowToast (this UIView context, UIView toast, double duration, object point)
        {
            toast.Center = CenterPointForPosition (context, point, toast);
            toast.Alpha = 0.0f;

            if (CSToastHidesOnTap) {
                UITapGestureRecognizer recognizer = new UITapGestureRecognizer (() => HandleToastTapped (toast));
                toast.AddGestureRecognizer (recognizer);
                toast.UserInteractionEnabled = true;
                toast.ExclusiveTouch = true;
            }

            context.AddSubview (toast);
            UIView.Animate (CSToastFadeDuration, 0.0, UIViewAnimationOptions.CurveEaseOut | UIViewAnimationOptions.AllowUserInteraction,
                () => toast.Alpha = 1.0f,
                () => {
                    Timer timer = new Timer (t => ToastTimerDidFinish (toast));
                    timer.Change ((int)(duration*1000), Timeout.Infinite);
                }
            );
        }

        static void HideToast (UIView toast)
        {
            if (toast.Superview == null)
                return;
            UIView.Animate (CSToastFadeDuration, 0.0, UIViewAnimationOptions.CurveEaseIn | UIViewAnimationOptions.BeginFromCurrentState,
                () => toast.Alpha = 0.0f, toast.RemoveFromSuperview);
        }

        #endregion

        #region Events

        static void HandleToastTapped (UIView toast)
        {
            HideToast (toast);
        }

        static void ToastTimerDidFinish (UIView toast)
        {
            new DispatchAdapter ().Invoke (() => HideToast (toast));
        }

        #endregion

        #region Toast Activity Methods

        static UIView _ExistingActivityView;

        public static void MakeToastActivity (this UIView context)
        {
            MakeToastActivity (context, CSToastActivityDefaultPosition);
        }

        public static void MakeToastActivity (this UIView context, object position)
        {
            // sanity
            if (_ExistingActivityView != null)
                return;

            var activityView = new UIView (new RectangleF (0f, 0f, CSToastActivityWidth, CSToastActivityHeight));
            activityView.Center = CenterPointForPosition (context, position, activityView);
            activityView.BackgroundColor = UIColor.Black.ColorWithAlpha (CSToastOpacity);
            activityView.Alpha = 0f;
            activityView.AutoresizingMask = UIViewAutoresizing.FlexibleLeftMargin | UIViewAutoresizing.FlexibleRightMargin | UIViewAutoresizing.FlexibleTopMargin | UIViewAutoresizing.FlexibleBottomMargin;
            activityView.Layer.CornerRadius = CSToastCornerRadius;
            if (CSToastDisplayShadow) {
                activityView.Layer.ShadowColor = UIColor.Black.CGColor;
                activityView.Layer.ShadowOpacity = CSToastShadowOpacity;
                activityView.Layer.ShadowRadius = CSToastShadowRadius;
                activityView.Layer.ShadowOffset = CSToastShadowOffset;
            }

            UIActivityIndicatorView activityIndicatorView = new UIActivityIndicatorView (UIActivityIndicatorViewStyle.WhiteLarge);
            activityIndicatorView.Center = new PointF (activityView.Bounds.Size.Width/2, activityView.Bounds.Size.Height/2);
            activityView.AddSubview (activityIndicatorView);
            activityIndicatorView.StartAnimating ();

            // associate the activity view with self
            _ExistingActivityView = activityView;
            context.AddSubview (activityView);

            UIView.Animate (CSToastFadeDuration, 0.0, UIViewAnimationOptions.CurveEaseOut,
                () => activityView.Alpha = 1.0f,
                null);
        }

        public static void HideToastActivity (this UIView context)
        {
            if (_ExistingActivityView != null) {
                UIView.Animate (CSToastFadeDuration, 0.0f, UIViewAnimationOptions.CurveEaseIn | UIViewAnimationOptions.BeginFromCurrentState,
                    () => _ExistingActivityView.Alpha = 0.0f,
                    () => {
                        _ExistingActivityView.RemoveFromSuperview ();
                        _ExistingActivityView = null;
                    }
                );
            }
        }

        #endregion

        #region Helpers

        static PointF CenterPointForPosition (UIView view, object point, UIView toast)
        {
            if (point is string) {
                // convert string literals @"top", @"bottom", @"center", or any point wrapped in an NSValue object into a CGPoint
                if (String.Equals ((string)point, "top", StringComparison.OrdinalIgnoreCase)) {
                    return new PointF (view.Bounds.Width / 2, (view.Bounds.Height / 2) + CSToastVerticalPadding);
                } else if (String.Equals ((string)point, "bottom", StringComparison.OrdinalIgnoreCase)) {
                    return new PointF (view.Bounds.Width / 2, (view.Bounds.Size.Height - (toast.Frame.Height / 2)) - CSToastVerticalPadding);
                } else if (String.Equals ((string)point, "center", StringComparison.OrdinalIgnoreCase)) {
                    return new PointF (view.Bounds.Width / 2, view.Bounds.Height / 2);
                }
            } else if (point is PointF) {
                return (PointF)point;
            }

            Logger.WriteLine ("Warning: Invalid position for toast.");
            return CenterPointForPosition (view, CSToastDefaultPosition, toast);
        }

        static SizeF SizeForString (string message, UIFont font, SizeF size, UILineBreakMode lineBreakMode)
        {

            /*if ([string respondsToSelector:@selector(boundingRectWithSize:options:attributes:context:)]) {
                NSMutableParagraphStyle *paragraphStyle = [[NSMutableParagraphStyle alloc] init];
                paragraphStyle.lineBreakMode = lineBreakMode;
                NSDictionary *attributes = @{NSFontAttributeName:font, NSParagraphStyleAttributeName:paragraphStyle};
                CGRect boundingRect = [string boundingRectWithSize:constrainedSize options:NSStringDrawingUsesLineFragmentOrigin attributes:attributes context:nil];
                return CGSizeMake(ceilf(boundingRect.size.width), ceilf(boundingRect.size.height));
            }

            return [string sizeWithFont:font constrainedToSize:constrainedSize lineBreakMode:lineBreakMode];*/

            return TextUtils.SizeWithFont (message, font, size, lineBreakMode);
        }

        static UIView ViewForMessage (UIView context, string message, string title, UIImage image)
        {
            // sanity
            if ((message == null) && (title == null) && (image == null)) return null;

            // dynamically build a toast view with any combination of message, title, & image.
            UILabel messageLabel = null;
            UILabel titleLabel = null;
            UIImageView imageView = null;

            // create the parent view
            UIView wrapperView = new UIView ();
            wrapperView.AutoresizingMask = UIViewAutoresizing.FlexibleLeftMargin | UIViewAutoresizing.FlexibleRightMargin | UIViewAutoresizing.FlexibleTopMargin | UIViewAutoresizing.FlexibleBottomMargin;
            wrapperView.Layer.CornerRadius = CSToastCornerRadius;

            if (CSToastDisplayShadow) {
                wrapperView.Layer.ShadowColor = UIColor.Black.CGColor;
                wrapperView.Layer.ShadowOpacity = CSToastShadowOpacity;
                wrapperView.Layer.ShadowRadius = CSToastShadowRadius;
                wrapperView.Layer.ShadowOffset = CSToastShadowOffset;
            }

            wrapperView.BackgroundColor = UIColor.Black.ColorWithAlpha (CSToastOpacity);
            if (image != null) {
                imageView = new UIImageView (image);
                imageView.ContentMode = UIViewContentMode.ScaleAspectFit;
                imageView.Frame = new RectangleF (CSToastHorizontalPadding, CSToastVerticalPadding, CSToastImageViewWidth, CSToastImageViewHeight);
            }

            float imageWidth, imageHeight, imageLeft;
            if (imageView != null) {
                imageWidth = imageView.Bounds.Width;
                imageHeight = imageView.Bounds.Height;
                imageLeft = CSToastHorizontalPadding; 
            } else {
                imageWidth = imageHeight = imageLeft = 0.0f;
            }

            if (title != null) {
                titleLabel = new UILabel ();
                titleLabel.Lines = CSToastMaxTitleLines;
                titleLabel.Font = UIFont.BoldSystemFontOfSize (CSToastFontSize);
                titleLabel.TextAlignment = UITextAlignment.Left;
                titleLabel.LineBreakMode = UILineBreakMode.WordWrap;
                titleLabel.TextColor = UIColor.White;
                titleLabel.BackgroundColor = UIColor.Clear;
                titleLabel.Alpha = 1.0f;
                titleLabel.Text = title;

                // size the title label according to the length of the text
                SizeF maxSizeTitle = new SizeF ((context.Bounds.Size.Width * CSToastMaxWidth) - imageWidth, context.Bounds.Size.Height * CSToastMaxHeight);
                SizeF expectedSizeTitle = SizeForString (title, titleLabel.Font, maxSizeTitle, titleLabel.LineBreakMode);
                titleLabel.Frame = new RectangleF (0.0f, 0.0f, expectedSizeTitle.Width, expectedSizeTitle.Height);
            }

            if (message != null) {
                messageLabel = new UILabel ();
                messageLabel.Lines = CSToastMaxMessageLines;
                messageLabel.Font = UIFont.SystemFontOfSize (CSToastFontSize);
                messageLabel.LineBreakMode = UILineBreakMode.WordWrap;
                messageLabel.TextColor = UIColor.White;
                messageLabel.BackgroundColor = UIColor.Clear;
                messageLabel.Alpha = 1.0f;
                messageLabel.Text = message;

                // size the message label according to the length of the text
                SizeF maxSizeMessage = new SizeF((context.Bounds.Size.Width * CSToastMaxWidth) - imageWidth, context.Bounds.Size.Height * CSToastMaxHeight);
                SizeF expectedSizeMessage = SizeForString (message, messageLabel.Font, maxSizeMessage, messageLabel.LineBreakMode);
                messageLabel.Frame = new RectangleF (0.0f, 0.0f, expectedSizeMessage.Width, expectedSizeMessage.Height);
            }

            // titleLabel frame values
            float titleWidth, titleHeight, titleTop, titleLeft;

            if (titleLabel != null) {
                titleWidth = titleLabel.Bounds.Size.Width;
                titleHeight = titleLabel.Bounds.Size.Height;
                titleTop = CSToastVerticalPadding;
                titleLeft = imageLeft + imageWidth + CSToastHorizontalPadding;
            } else {
                titleWidth = titleHeight = titleTop = titleLeft = 0.0f;
            }

            // messageLabel frame values
            float messageWidth, messageHeight, messageLeft, messageTop;

            if (messageLabel != null) {
                messageWidth = messageLabel.Bounds.Size.Width;
                messageHeight = messageLabel.Bounds.Size.Height;
                messageLeft = imageLeft + imageWidth + CSToastHorizontalPadding;
                messageTop = titleTop + titleHeight + CSToastVerticalPadding;
            } else {
                messageWidth = messageHeight = messageLeft = messageTop = 0.0f;
            }

            float longerWidth = Math.Max (titleWidth, messageWidth);
            float longerLeft = Math.Max (titleLeft, messageLeft);

            // wrapper width uses the longerWidth or the image width, whatever is larger. same logic applies to the wrapper height
            float wrapperWidth = Math.Max((imageWidth + (CSToastHorizontalPadding * 2)), (longerLeft + longerWidth + CSToastHorizontalPadding));    
            float wrapperHeight = Math.Max((messageTop + messageHeight + CSToastVerticalPadding), (imageHeight + (CSToastVerticalPadding * 2)));

            wrapperView.Frame = new RectangleF(0.0f, 0.0f, wrapperWidth, wrapperHeight);

            if (titleLabel != null) {
                titleLabel.Frame = new RectangleF (titleLeft, titleTop, titleWidth, titleHeight);
                wrapperView.AddSubview (titleLabel);
            }

            if (messageLabel != null) {
                messageLabel.Frame = new RectangleF(messageLeft, messageTop, messageWidth, messageHeight);
                wrapperView.AddSubview (messageLabel);
            }

            if (imageView != null) {
                wrapperView.AddSubview (imageView);
            }

            return wrapperView;
        }

        #endregion
    }
}