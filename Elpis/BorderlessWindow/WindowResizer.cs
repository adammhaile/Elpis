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
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using GUI.Interop;

namespace GUI.BorderlessWindow
{
    /// <summary>
    /// Determines the position of a window border.
    /// </summary>
    public enum BorderPosition
    {
        Left = 61441,
        Right = 61442,
        Top = 61443,
        TopLeft = 61444,
        TopRight = 61445,
        Bottom = 61446,
        BottomLeft = 61447,
        BottomRight = 61448
    }

    /// <summary>
    /// Represents a Framework element which is acting as a border for a window.
    /// </summary>
    public class WindowBorder
    {
        /// <summary>
        /// Creates a new window border using the specified element and position.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="element"></param>
        public WindowBorder(BorderPosition position, FrameworkElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            Position = position;
            Element = element;
        }

        /// <summary>
        /// The element which is acting as the border.
        /// </summary>
        public FrameworkElement Element { get; private set; }

        /// <summary>
        /// The position of the border.
        /// </summary>
        public BorderPosition Position { get; private set; }
    }

    /// <summary>
    /// Class which manages resizing of borderless windows.
    /// Based heavily on Kirupa Chinnathambi's code at http://blog.kirupa.com/?p=256.
    /// </summary>
    public class WindowResizer
    {
        private readonly Win32 _win32;

        /// <summary>
        /// The borders for the window.
        /// </summary>
        private readonly WindowBorder[] borders;

        /// <summary>
        /// Defines the cursors that should be used when the mouse is hovering
        /// over a border in each position.
        /// </summary>
        private readonly Dictionary<BorderPosition, Cursor> cursors = new Dictionary<BorderPosition, Cursor>
                                                                          {
                                                                              {BorderPosition.Left, Cursors.SizeWE},
                                                                              {BorderPosition.Right, Cursors.SizeWE},
                                                                              {BorderPosition.Top, Cursors.SizeNS},
                                                                              {BorderPosition.Bottom, Cursors.SizeNS},
                                                                              {
                                                                                  BorderPosition.BottomLeft,
                                                                                  Cursors.SizeNESW
                                                                                  },
                                                                              {
                                                                                  BorderPosition.TopRight, Cursors.SizeNESW
                                                                                  },
                                                                              {
                                                                                  BorderPosition.BottomRight,
                                                                                  Cursors.SizeNWSE
                                                                                  },
                                                                              {BorderPosition.TopLeft, Cursors.SizeNWSE}
                                                                          };

        /// <summary>
        /// The WPF window.
        /// </summary>
        private readonly Window window;

        /// <summary>
        /// The handle to the window.
        /// </summary>
        private HwndSource hwndSource;

        /// <summary>
        /// Creates a new WindowResizer for the specified Window using the
        /// specified border elements.
        /// </summary>
        /// <param name="window">The Window which should be resized.</param>
        /// <param name="borders">The elements which can be used to resize the window.</param>
        public WindowResizer(Window window, params WindowBorder[] borders)
        {
            if (window == null)
            {
                throw new ArgumentNullException("window");
            }
            if (borders == null)
            {
                throw new ArgumentNullException("borders");
            }

            this.window = window;
            this.borders = borders;

            foreach (WindowBorder border in borders)
            {
                border.Element.PreviewMouseLeftButtonDown += Resize;
                border.Element.MouseMove += DisplayResizeCursor;
                border.Element.MouseLeave += ResetCursor;
            }

            _win32 = new Win32(window);
            window.SourceInitialized += (o, e) => hwndSource = (HwndSource) PresentationSource.FromVisual((Visual) o);
            window.SourceInitialized += ((o, e) => _win32.SourceInitialized(window));
            // window.SizeChanged += (o, e) => ConfineMinMax();
        }

        private void ConfineMinMax()
        {
            if (window.ActualHeight <= window.MinHeight)
                window.Height = window.MinHeight;
            if (window.ActualHeight >= window.MaxHeight)
                window.Height = window.MaxHeight;

            if (window.ActualWidth <= window.MinWidth)
                window.Width = window.MinWidth;
            if (window.ActualWidth >= window.MaxWidth)
                window.Width = window.MaxWidth;
        }

        /// <summary>
        /// Puts a resize message on the message queue for the specified border position.
        /// </summary>
        /// <param name="direction"></param>
        private void ResizeWindow(BorderPosition direction)
        {
            _win32.SendWMMessage(hwndSource.Handle, 0x112, (IntPtr) direction, IntPtr.Zero);
        }

        /// <summary>
        /// Resets the cursor when the left mouse button is not pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResetCursor(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton != MouseButtonState.Pressed)
            {
                window.Cursor = Cursors.Arrow;
            }
        }

        /// <summary>
        /// Resizes the window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Resize(object sender, MouseButtonEventArgs e)
        {
            WindowBorder border = borders.Single(b => b.Element.Equals(sender));
            window.Cursor = cursors[border.Position];
            ResizeWindow(border.Position);
        }

        /// <summary>
        /// Ensures that the correct cursor is displayed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DisplayResizeCursor(object sender, MouseEventArgs e)
        {
            WindowBorder border = borders.Single(b => b.Element.Equals(sender));
            window.Cursor = cursors[border.Position];
        }
    }
}