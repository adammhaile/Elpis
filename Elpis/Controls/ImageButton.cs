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
    public class ImageButton : Button
    {
        public static readonly DependencyProperty ActiveImageUriProperty =
            DependencyProperty.RegisterAttached("ActiveImageUri", typeof (Uri), typeof (ImageButton),
                                                new PropertyMetadata(null));

        public static readonly DependencyProperty InactiveImageUriProperty =
            DependencyProperty.RegisterAttached("InactiveImageUri", typeof (Uri), typeof (ImageButton),
                                                new PropertyMetadata(null));

        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.RegisterAttached("IsActive", typeof (bool), typeof (ImageButton),
                                                new PropertyMetadata(false));

        public ImageButton() : base()
        {
            //Do not want any of these buttons to respond to Space select, do this for all
            this.PreviewKeyDown += ((o, e) => e.Handled = true);
        }

        public Uri ActiveImageUri
        {
            get { return (Uri) GetValue(ActiveImageUriProperty); }
            set { SetValue(ActiveImageUriProperty, value); }
        }

        public Uri InactiveImageUri
        {
            get { return (Uri) GetValue(InactiveImageUriProperty); }
            set { SetValue(InactiveImageUriProperty, value); }
        }

        public bool IsActive
        {
            get { return (bool) GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }
    }
}