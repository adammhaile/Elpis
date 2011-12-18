/*
 * Copyright 2012 - Adam Haile
 * http://adamhaile.net
 *
 * This file is part of Elpis.
 * Elpis is free software: you can redistribute it and/or modify 
 * it under the terms of the GNU General Public License as published by 
 * the Free Software Foundation, either version 3 of the License, or 
 * (at your option) any later version.
 * 
 * Elpis is distributed in the hope that it will be useful, 
 * but WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License 
 * along with Elpis. If not, see http://www.gnu.org/licenses/.
*/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Elpis.Controls
{
    /// <summary>
    /// Simple control providing content spinning capability
    /// </summary>
    public class ContentSpinner : ContentControl
    {
        private const string ANIMATION = "AnimatedRotateTransform";

        #region Fields

        private FrameworkElement _content;
        private bool _running;
        private Storyboard _storyboard;

        #endregion

        #region Dependency properties

        public static DependencyProperty NumberOfFramesProperty =
            DependencyProperty.Register("NumberOfFrames",
                                        typeof (int),
                                        typeof (ContentSpinner),
                                        new FrameworkPropertyMetadata(16, OnPropertyChange),
                                        ValidateNumberOfFrames);

        public static DependencyProperty RevolutionsPerSecondProperty =
            DependencyProperty.Register("RevolutionsPerSecond",
                                        typeof (double),
                                        typeof (ContentSpinner),
                                        new PropertyMetadata(1.0, OnPropertyChange),
                                        ValidateRevolutionsPerSecond);

        public static DependencyProperty ContentScaleProperty =
            DependencyProperty.Register("ContentScale",
                                        typeof (double),
                                        typeof (ContentSpinner),
                                        new PropertyMetadata(1.0, OnPropertyChange),
                                        ValidateContentScale);

        public static DependencyProperty AutoStartProperty =
            DependencyProperty.Register("AutoStart",
                                        typeof (bool),
                                        typeof (ContentSpinner),
                                        new PropertyMetadata(true, OnPropertyChange));

        #endregion

        static ContentSpinner()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (ContentSpinner),
                                                     new FrameworkPropertyMetadata(typeof (ContentSpinner)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentSpinner"/> class.
        /// </summary>
        public ContentSpinner()
        {
            Loaded += (o, args) =>
                          {
                              if (AutoStart)
                                  StartAnimation();
                          };
            SizeChanged += (o, args) => RestartAnimation();
            Unloaded += (o, args) => StopAnimation();
        }

        /// <summary>
        /// Gets or sets the number of revolutions per second.
        /// </summary>
        public double RevolutionsPerSecond
        {
            get { return (double) GetValue(RevolutionsPerSecondProperty); }
            set { SetValue(RevolutionsPerSecondProperty, value); }
        }

        /// <summary>
        /// Gets or sets the number of frames per rotation.
        /// </summary>
        public int NumberOfFrames
        {
            get { return (int) GetValue(NumberOfFramesProperty); }
            set { SetValue(NumberOfFramesProperty, value); }
        }

        /// <summary>
        /// Gets or sets the content scale.
        /// </summary>
        public double ContentScale
        {
            get { return (double) GetValue(ContentScaleProperty); }
            set { SetValue(ContentScaleProperty, value); }
        }

        public bool AutoStart
        {
            get { return (bool) GetValue(AutoStartProperty); }
            set { SetValue(AutoStartProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _content = GetTemplateChild("PART_Content") as FrameworkElement;
        }

        public void StartAnimation()
        {
            if (_content == null)
                return;

            DoubleAnimationUsingKeyFrames animation = GetAnimation();

            _content.LayoutTransform = GetContentLayoutTransform();
            _content.RenderTransform = GetContentRenderTransform();

            _storyboard = new Storyboard();
            _storyboard.Children.Add(animation);

            _storyboard.Begin(this, true);

            _running = true;
        }

        public void StopAnimation()
        {
            if (_storyboard != null)
            {
                _storyboard.Stop();
                _storyboard.Remove(this);
                _storyboard = null;
                _running = false;
            }
        }

        private void RestartAnimation()
        {
            if (AutoStart || (!AutoStart && _running))
            {
                StopAnimation();
                StartAnimation();
            }
        }

        private Transform GetContentLayoutTransform()
        {
            return new ScaleTransform(ContentScale, ContentScale);
        }

        private Transform GetContentRenderTransform()
        {
            var rotateTransform = new RotateTransform(0, _content.ActualWidth/2*ContentScale,
                                                      _content.ActualHeight/2*ContentScale);
            RegisterName(ANIMATION, rotateTransform);

            return rotateTransform;
        }

        private DoubleAnimationUsingKeyFrames GetAnimation()
        {
            NameScope.SetNameScope(this, new NameScope());

            var animation = new DoubleAnimationUsingKeyFrames();

            for (int i = 0; i < NumberOfFrames; i++)
            {
                double angle = i*360.0/NumberOfFrames;
                KeyTime time = KeyTime.FromPercent(((double) i)/NumberOfFrames);
                DoubleKeyFrame frame = new DiscreteDoubleKeyFrame(angle, time);
                animation.KeyFrames.Add(frame);
            }

            animation.Duration = TimeSpan.FromSeconds(1/RevolutionsPerSecond);
            animation.RepeatBehavior = RepeatBehavior.Forever;

            Storyboard.SetTargetName(animation, ANIMATION);
            Storyboard.SetTargetProperty(animation, new PropertyPath(RotateTransform.AngleProperty));

            return animation;
        }

        #region Validation and prop change methods

        private static bool ValidateNumberOfFrames(object value)
        {
            var frames = (int) value;
            return frames > 0;
        }

        private static bool ValidateContentScale(object value)
        {
            var scale = (double) value;
            return scale > 0.0;
        }

        private static bool ValidateRevolutionsPerSecond(object value)
        {
            var rps = (double) value;
            return rps > 0.0;
        }

        private static void OnPropertyChange(DependencyObject target, DependencyPropertyChangedEventArgs args)
        {
            var spinner = (ContentSpinner) target;
            spinner.RestartAnimation();
        }

        #endregion
    }
}