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
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace GUI.PageTransition
{
    public partial class PageTransition : UserControl
    {
        //Stack<UserControl> pages = new Stack<UserControl>();

        #region Delegates

        public delegate void CurrentPageSetHandler(UserControl page);

        #endregion

        public static readonly DependencyProperty TransitionTypeProperty = DependencyProperty.Register("TransitionType",
                                                                                                       typeof (
                                                                                                           PageTransitionType
                                                                                                           ),
                                                                                                       typeof (
                                                                                                           PageTransition
                                                                                                           ),
                                                                                                       new PropertyMetadata
                                                                                                           (PageTransitionType
                                                                                                                .Next));

        private readonly object _inTransitionLock = new object();

        private readonly List<UserControl> pages = new List<UserControl>();
        private ContentControl _currentContent;
        private bool _inTransition;
        private UserControl _loadingPage;

        private UserControl _nextPage;
        private PageTransitionType _nextTrasitionType = PageTransitionType.Auto;

        public PageTransition()
        {
            InitializeComponent();
            ClipToBounds = true;
        }

        public bool InTransition
        {
            get
            {
                lock (_inTransitionLock)
                {
                    return _inTransition;
                }
            }
            private set
            {
                lock (_inTransitionLock)
                {
                    _inTransition = value;
                }
            }
        }

        [DefaultValue(null)]
        public UserControl CurrentPage { get; set; }

        public List<UserControl> PageList
        {
            get { return pages; }
        }

        public PageTransitionType TransitionType
        {
            get { return (PageTransitionType) GetValue(TransitionTypeProperty); }
            set { SetValue(TransitionTypeProperty, value); }
        }

        public event CurrentPageSetHandler CurrentPageSet;

        public void AddPage(UserControl newPage, int index = -1)
        {
            if (index < 0)
                pages.Add(newPage);
            else
            {
                if (index > pages.Count - 1)
                    index = pages.Count;

                pages.Insert(index, newPage);
            }

            //Task.Factory.StartNew(() => ShowNewPage());
        }

        public void RemovePage(UserControl page)
        {
            if (pages.Contains(page))
                pages.Remove(page);
        }

        public void ShowPage(UserControl page, PageTransitionType type = PageTransitionType.Auto)
        {
            if (page == CurrentPage)
                return;

            Task.Factory.StartNew(() => Dispatcher.Invoke(new Action(() => loadPage(page, type))));
        }

        public void ShowNextPage()
        {
            UserControl page = null;
            if (CurrentPage == null)
                page = pages[0];

            int i = pages.IndexOf(CurrentPage);
            int next = (i == pages.Count - 1) ? 0 : i + 1;

            ShowPage(pages[next]);
        }

        public void ShowPrevPage()
        {
            UserControl page = null;
            if (CurrentPage == null)
                page = pages[pages.Count - 1];

            int i = pages.IndexOf(CurrentPage);
            int prev = (i == 0) ? pages.Count - 1 : i - 1;

            ShowPage(pages[prev]);
        }

        private void loadPage(UserControl page, PageTransitionType type)
        {
            if (page == null)
                return;

            //If already in a trasition, save it for next time
            //Overwrite to skip if it's already filled
            if (InTransition)
            {
                _nextPage = page;
                _nextTrasitionType = type;
                return;
            }

            int i = pages.IndexOf(page);
            if (i < 0)
                return;

            if (type == PageTransitionType.Auto)
            {
                if (CurrentPage == null)
                    type = PageTransitionType.Next;
                else
                {
                    int c = pages.IndexOf(CurrentPage);
                    if (c == (pages.Count - 1) && i == 0)
                        type = PageTransitionType.Next;
                    else if (c == 0 && i == (pages.Count - 1))
                        type = PageTransitionType.Previous;
                    else
                        type = (c < i) ? PageTransitionType.Next : PageTransitionType.Previous;
                }
            }

            _nextPage = null;
            _nextTrasitionType = PageTransitionType.Auto;

            InTransition = true;

            _loadingPage = page;
            TransitionType = type;

            _loadingPage.Loaded += newPage_Loaded;

            //switch active content control
            if (_currentContent == contentA)
                _currentContent = contentB;
            else
                _currentContent = contentA;

            _currentContent.Content = _loadingPage;
        }

        private void newPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (TransitionType == PageTransitionType.Auto)
                return;

            _loadingPage.Loaded -= newPage_Loaded;

            ContentControl _oldContent = null;
            if (_currentContent == contentA)
                _oldContent = contentB;
            else
                _oldContent = contentA;

            _currentContent.Visibility = Visibility.Visible;
            //_oldContent.Visibility = System.Windows.Visibility.Hidden;

            if (CurrentPage != null)
            {
                Storyboard hidePage =
                    (Resources[string.Format("{0}Out", TransitionType.ToString())] as Storyboard).Clone();
                var to = (Thickness) ((ThicknessAnimation) hidePage.Children[0]).To;
                ((ThicknessAnimation) hidePage.Children[0]).To =
                    new Thickness(to.Left*ActualWidth, to.Top*ActualHeight,
                                  to.Right*ActualWidth, to.Bottom*ActualHeight);

                hidePage.Completed += hidePage_Completed;

                Storyboard showNewPage =
                    (Resources[string.Format("{0}In", TransitionType.ToString())] as Storyboard).Clone();
                var from = (Thickness) ((ThicknessAnimation) showNewPage.Children[0]).From;
                ((ThicknessAnimation) showNewPage.Children[0]).From =
                    new Thickness(from.Left*ActualWidth, from.Top*ActualHeight,
                                  from.Right*ActualWidth, from.Bottom*ActualHeight);

                showNewPage.Completed += showNewPage_Completed;

                if (CurrentPage != null)
                    hidePage.Begin(_oldContent);
                showNewPage.Begin(_currentContent);
            }
            else
                InTransition = false;

            CurrentPage = (UserControl) sender;
        }

        private void showNewPage_Completed(object sender, EventArgs e)
        {
            InTransition = false;

            if (CurrentPageSet != null)
                CurrentPageSet(CurrentPage);

            if (_nextPage != null)
                loadPage(_nextPage, _nextTrasitionType);
        }

        private void hidePage_Completed(object sender, EventArgs e)
        {
            if (_currentContent == contentA)
            {
                contentB.Visibility = Visibility.Hidden;
                contentB.Content = null;
            }
            else
            {
                contentA.Visibility = Visibility.Hidden;
                contentA.Content = null;
            }
        }
    }
}