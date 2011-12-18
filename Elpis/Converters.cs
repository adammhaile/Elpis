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
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace GUI.Converters
{
    public class BinaryImageConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value.GetType() != typeof (byte[]))
            {
                if (parameter == null)
                    return new BitmapImage();
                
                return parameter;
            }

            var data = (byte[]) value;
            var result = new BitmapImage();
            result.BeginInit();
            result.StreamSource = new MemoryStream(data);
            result.EndInit();
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class BooleanToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
                              CultureInfo culture)
        {
            bool state = true;
            if (parameter != null)
                state = (bool) parameter;

            if (value is Boolean)
            {
                return ((bool) value)
                           ? (state ? Visibility.Visible : Visibility.Hidden)
                           : (state ? Visibility.Hidden : Visibility.Visible);
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class WindowStateToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
                              CultureInfo culture)
        {
            if (value is WindowState)
            {
                bool state = ((WindowState) value) == WindowState.Maximized;
                if (parameter != null && !(bool.Parse((string) parameter)))
                    state = !state;

                return state ? Visibility.Visible : Visibility.Collapsed;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class WindowStateToThicknessConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
                              CultureInfo culture)
        {
            if (value is WindowState)
            {
                bool state = ((WindowState) value) == WindowState.Maximized;
                int margin = 0;

                if (parameter != null && !state)
                    margin = (int.Parse((string) parameter));

                return new Thickness(margin);
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class AssemblyVersionConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
                              CultureInfo culture)
        {
            try
            {
                Version ver = Assembly.GetEntryAssembly().GetName().Version;
                return ver.ToString();
            }
            catch (Exception)
            {
                return "0.0.0.0";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}