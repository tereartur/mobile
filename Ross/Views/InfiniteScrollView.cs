﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MonoTouch.CoreGraphics;
using MonoTouch.UIKit;

namespace Toggl.Ross.Views
{
    public class InfiniteScrollView<TView> : UIScrollView where TView : UIView
    {
        public event EventHandler OnChangePage;

        private int _pageIndex;

        public int PageIndex
        {
            get {
                return _pageIndex + (Convert.ToInt32 ( ContentOffset.X / PageWidth) - tmpOffset);
            }
        }

        private TView currentPage;

        public TView CurrentPage
        {
            get {
                var pos = ConvertPointToView ( ContentOffset, _containerView).X;
                foreach (var view in pages)
                    if ( Math.Abs (pos - view.Frame.X) <= PageWidth / 2) {
                        currentPage = view;
                    }

                return currentPage;
            }
        }

        public List<TView> Pages
        {
            get {
                return pages;
            }
        }

        public InfiniteScrollView ( IInfiniteScrollViewSource viewSource)
        {
            this.viewSource = viewSource;
            pages = new List<TView> ();
            _containerView = new UIView ();
            Add (_containerView);

            ShowsHorizontalScrollIndicator = false;
            PagingEnabled = true;
        }

        List<TView> pages;
        UIView _containerView;
        IInfiniteScrollViewSource viewSource;

        public const float PageWidth = 320;
        private int tmpOffset;
        private int prevPageIndex = 5000;

        private void RecenterIfNeeded()
        {
            PointF currentOffset = ContentOffset;
            float contentWidth = ContentSize.Width;
            float centerOffsetX = (contentWidth - Bounds.Width) / 2.0f;
            float distanceFromCenter = Math.Abs ( currentOffset.X - centerOffsetX);

            if (distanceFromCenter > (contentWidth / 4.0f) && (distanceFromCenter - PageWidth/2) % PageWidth == 0) {
                _pageIndex += Convert.ToInt32 ( ContentOffset.X / PageWidth) - tmpOffset;
                ContentOffset = new PointF (centerOffsetX - PageWidth/2, currentOffset.Y);
                foreach (var item in pages) {
                    PointF center = _containerView.ConvertPointToView (item.Center, this);
                    center.X += centerOffsetX - currentOffset.X - PageWidth/2;
                    item.Center = ConvertPointToView (center, _containerView);
                }
            }
        }

        public override void LayoutSubviews ()
        {
            base.LayoutSubviews ();

            ContentSize = new SizeF ( PageWidth * 20, Bounds.Height);
            _containerView.Frame = new RectangleF (0, 0, ContentSize.Width, ContentSize.Height);
            RecenterIfNeeded ();

            // tile content in visible bounds
            RectangleF visibleBounds = ConvertRectToView (Bounds, _containerView);
            float minimumVisibleX = CGRectGetMinX ( visibleBounds);
            float maximumVisibleX = CGRectGetMaxX ( visibleBounds);
            TileViews ( minimumVisibleX, maximumVisibleX);

            if (prevPageIndex != PageIndex) {
                prevPageIndex = PageIndex;
                if (OnChangePage != null) {
                    OnChangePage.Invoke (this, new EventArgs ());
                }
            }
        }

        public void SetPageIndex ( int offSet, bool animated)
        {
            var currentCOffset = ContentOffset;
            if (currentCOffset.X % PageWidth == 0) {
                currentCOffset.X += PageWidth * offSet;
                SetContentOffset (currentCOffset, animated);
            }
        }

        public void RefreshVisibleView()
        {
            if (Dragging) {
                return;
            }

            var currentView = pages.Find (v => v.Frame.X.CompareTo ( ContentOffset.X) == 0);
            var center = currentView.Center;
            TView newView = InsertView ();
            var offSetY = ContentSize.Height;
            var frame = currentView.Frame;

            frame.Y += offSetY;
            newView.Frame = frame;

            UIView.Animate (0.6, 0.4, UIViewAnimationOptions.CurveEaseIn, () => { currentView.Alpha = 0.25f; }, null);

            UIView.Animate (0.7, 0.5, UIViewAnimationOptions.CurveEaseIn,
            () => {
                currentView.Transform = CGAffineTransform.MakeScale ( 0.75f, 0.75f);
                currentView.Center = new PointF ( center.X, center.Y + 105);
            }, null);

            UIView.Animate (0.7, 0.6, UIViewAnimationOptions.CurveEaseInOut,
            () => {
                newView.Center = center;
            },() => {
                foreach (var item in pages) {
                    viewSource.Dispose ( item);
                    item.RemoveFromSuperview();
                }
                pages.Clear();
                pages.Add (newView);
                if (OnChangePage != null) {
                    OnChangePage.Invoke (this, new EventArgs ());
                }
            });
        }

        public override bool GestureRecognizerShouldBegin (UIGestureRecognizer gestureRecognizer)
        {
            return viewSource.ShouldStartScroll ();
        }

        private TView InsertView()
        {
            TView view = viewSource.CreateView ();
            view.Frame = new RectangleF (0, 0, PageWidth, Bounds.Height);
            _containerView.Add (view);
            return view;
        }

        private float PlaceNewViewOnRight ( float rightEdge)
        {
            TView view = InsertView ();
            pages.Add (view); // add rightmost label at the end of the array

            RectangleF viewFrame = view.Frame;
            viewFrame.X = rightEdge;
            view.Frame = viewFrame;
            return CGRectGetMaxX ( viewFrame);
        }

        private float PlaceNewViewOnLeft ( float leftEdge)
        {
            TView view = InsertView ();
            pages.Insert ( 0, view); // add leftmost label at the beginning of the array

            RectangleF viewFrame = view.Frame;
            viewFrame.X = leftEdge - viewFrame.Width;
            view.Frame = viewFrame;
            return CGRectGetMinX ( viewFrame);
        }

        private void TileViews ( float minX, float maxX)
        {
            // the upcoming tiling logic depends on there already being at least one label in the visibleLabels array, so
            // to kick off the tiling we need to make sure there's at least one label
            if (pages.Count == 0) {
                tmpOffset = Convert.ToInt32 (ContentOffset.X / PageWidth);
                PlaceNewViewOnRight (minX);
                currentPage = pages [0];
            }

            // add views that are missing on right side
            TView lastView = pages [pages.Count - 1];
            float rightEdge = CGRectGetMaxX ( lastView.Frame);
            while ( rightEdge < maxX) {
                rightEdge = PlaceNewViewOnRight (rightEdge);
            }

            // add views that are missing on left side
            TView firstView = pages [0];
            float leftEdge = CGRectGetMinX ( firstView.Frame);
            while ( leftEdge > minX) {
                leftEdge = PlaceNewViewOnLeft (leftEdge);
            }

            // remove views that have fallen off right edge
            lastView = pages.Last();
            while (lastView.Frame.X > maxX) {
                lastView.RemoveFromSuperview ();
                viewSource.Dispose (lastView);
                pages.Remove (lastView);
                lastView = pages.Last();
            }

            // remove views that have fallen off left edge
            firstView = pages.First();
            while ( CGRectGetMaxX ( firstView.Frame) < minX) {
                firstView.RemoveFromSuperview ();
                viewSource.Dispose (lastView);
                pages.Remove (firstView);
                firstView = pages.First();
            }
        }

        private float CGRectGetMinX ( RectangleF rect)
        {
            return rect.X;
        }

        private float CGRectGetMaxX ( RectangleF rect)
        {
            return rect.X + rect.Width;
        }

        public interface IInfiniteScrollViewSource
        {
            TView CreateView ();

            void Dispose (TView view);

            bool ShouldStartScroll();
        }
    }


}