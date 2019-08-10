﻿using Mastonet.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Tooter.Model;
using Tooter.Services;
using Tooter.ViewModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Tooter.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TimelineView : Page, INotifyPropertyChanged
    {

        ScrollViewer _listViewScrollViewer = null;
        bool isListViewRefreshingEnabled = true;


        private TimelineViewModelBase _viewModel;

        public TimelineViewModelBase ViewModel
        {
            get { return _viewModel; }
            set
            {
                _viewModel = value;
                NotifyPropertyChanged();
            }
        }

        public TimelineView()
        {
            this.InitializeComponent();
            RegisterEvents();
        }

        public TimelineView(TimelineViewModelBase ViewModelToUse)
        {
            if (!this.IsLoaded)
            {
                this.InitializeComponent();
                RegisterEvents();
            }
            ViewModel = ViewModelToUse;
            RegisterViewModelEvents();
        }

        private void RegisterEvents()
        {
            TootsListView.PointerEntered += TootsListView_PointerEntered;
            CacheService.TimelineCacheRequested += CacheService_TimelineCacheRequested;
        }

        private async void CacheService_TimelineCacheRequested(object sender, EventArgs e)
        {
            var currentTopVisibleStatus = GetTopVisibleToot();
            if (currentTopVisibleStatus != null)
            {
               await ViewModel.CacheTimeline(currentTopVisibleStatus);
            }
        }

        private void ViewModel_TootsAdded(object sender, EventArgs e)
        {
            EnableListViewRefreshing();
        }

        private void EnableListViewRefreshing()
        {
            isListViewRefreshingEnabled = true;
        }

        private void TootsListView_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (_listViewScrollViewer == null)
            {
                _listViewScrollViewer = TryFindScrollViewer(TootsListView);
            }
        }

        private ScrollViewer TryFindScrollViewer(DependencyObject depObj)
        {
            ScrollViewer viewerToFind = GetScrollViewer(depObj);

            if (viewerToFind != null)
            {
                TootsListView.PointerEntered -= TootsListView_PointerEntered;
                viewerToFind.ViewChanging += ScrollViewerViewChanging;
            }

            return viewerToFind;
        }

       



        private async void ScrollViewerViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {

            if (isListViewRefreshingEnabled)
            {
                const double loadingRangeHeight = 100;
                if (e.NextView.VerticalOffset + loadingRangeHeight >= _listViewScrollViewer.ScrollableHeight)
                {
                    DisableListViewRefreshing();
                    await ViewModel.AddOlderContentToFeed();
                }
                else if (e.NextView.VerticalOffset < 100 && _listViewScrollViewer.VerticalOffset > 100)
                {
                    DisableListViewRefreshing();
                    await ViewModel.AddNewerContentToFeed();
                }

            }
        }

        private void DisableListViewRefreshing()
        {
            isListViewRefreshingEnabled = false;
        }

        private static ScrollViewer GetScrollViewer(DependencyObject depObj)
        {
            if (depObj is ScrollViewer) return depObj as ScrollViewer;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = GetScrollViewer(child);
                if (result != null) return result;
            }
            return null;
        }



        private void RegisterViewModelEvents()
        {
            ViewModel.TootsAdded += ViewModel_TootsAdded;
        }



        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is TimelineViewModelBase newViewModel)
            {
                ViewModel = newViewModel;
            }
            else if (e.Parameter is Type ViewModelType)
            {
                ViewModel = Activator.CreateInstance(ViewModelType) as TimelineViewModelBase;
            }
            RegisterViewModelEvents();
            await ViewModel.LoadFeedAsync();


        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private void TootSaveTestButton_Click(object sender, RoutedEventArgs e)
        {
            GetTopVisibleToot();
        }

        public Status GetTopVisibleToot()
        {
            Status topToot = null;
            ScrollViewer sv = _listViewScrollViewer;
            Rect svViewportBounds = new Rect(sv.HorizontalOffset, sv.VerticalOffset, sv.ViewportWidth, sv.ViewportHeight);

            for (int i = 0; i < TootsListView.Items.Count; ++i)
            {
                var container = TootsListView.ContainerFromIndex(i) as FrameworkElement;
                if (container != null)
                {
                    
                    var containerVisual = container.TransformToVisual(TootsListView);
                    var containerPosition = containerVisual.TransformPoint(new Point(0,0));

                    var bounds = new Rect(containerPosition.X, containerPosition.Y, container.ActualWidth, container.ActualHeight);

                    
                    if (RectHelper.Intersect(bounds, svViewportBounds) != Rect.Empty)
                    {
                        topToot = (Status)TootsListView.Items[i];
                        break;
                    }
                }
            }

            return topToot;
        }
    }
}
