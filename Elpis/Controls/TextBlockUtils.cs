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

namespace Elpis.Controls
{
    public class TextBlockUtils
    {
        public static readonly DependencyProperty AutoTooltipProperty =
            DependencyProperty.RegisterAttached("AutoTooltip",
                                                typeof (bool), typeof (TextBlockUtils),
                                                new PropertyMetadata(false, OnAutoTooltipPropertyChanged));

        public static readonly DependencyProperty AutoTooltipFontSizeProperty =
            DependencyProperty.RegisterAttached("AutoTooltipFontSize",
                                                typeof (double), typeof (TextBlockUtils),
                                                new PropertyMetadata(0.0, OnAutoTooltipFontSizePropertyChanged));

        public static bool GetAutoTooltip(DependencyObject obj)
        {
            return (bool) obj.GetValue(AutoTooltipProperty);
        }

        public static void SetAutoTooltip(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoTooltipProperty, value);
        }

        private static void OnAutoTooltipPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textBlock = (TextBlock) d;
            if (textBlock == null)
                return;

            if (e.NewValue.Equals(true))
            {
                textBlock.TextTrimming = TextTrimming.CharacterEllipsis;
                ComputeAutoTooltip(textBlock);
                textBlock.SizeChanged += TextBlock_SizeChanged;
            }
            else
            {
                textBlock.SizeChanged -= TextBlock_SizeChanged;
            }
        }

        public static double GetAutoTooltipFontSize(DependencyObject obj)
        {
            var result = (double) obj.GetValue(AutoTooltipFontSizeProperty);
            if (result.Equals(0.0))
                return ((TextBlock) obj).FontSize;

            return result;
        }

        public static void SetAutoTooltipFontSize(DependencyObject obj, double value)
        {
            if (value.Equals(0.0))
                value = ((TextBlock) obj).FontSize;
            obj.SetValue(AutoTooltipFontSizeProperty, value);
        }

        private static void OnAutoTooltipFontSizePropertyChanged(DependencyObject d,
                                                                 DependencyPropertyChangedEventArgs e)
        {
        }

        private static void TextBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var textBlock = (TextBlock) sender;
            ComputeAutoTooltip(textBlock);
        }

        private static void ComputeAutoTooltip(TextBlock textBlock)
        {
            textBlock.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            double width = textBlock.DesiredSize.Width;

            if (textBlock.ActualWidth < width)
            {
                var toolBlock = new TextBlock()
                                    {
                                        Text = textBlock.Text,
                                        FontSize = GetAutoTooltipFontSize(textBlock)
                                    };
                ToolTipService.SetToolTip(textBlock, toolBlock);
            }
            else
            {
                ToolTipService.SetToolTip(textBlock, null);
            }
        }
    }
}