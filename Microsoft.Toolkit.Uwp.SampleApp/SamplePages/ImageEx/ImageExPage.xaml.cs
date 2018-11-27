// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.SampleApp.Data;
using Microsoft.Toolkit.Uwp.UI;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace Microsoft.Toolkit.Uwp.SampleApp.SamplePages
{
    public sealed partial class ImageExPage : IXamlRenderListener
    {
        private ObservableCollection<PhotoDataItem> photos;
        private int imageIndex;
        private StackPanel container;
        private ResourceDictionary resources;
        private Border lazyLoadingControlHost;

        public ImageExPage()
        {
            InitializeComponent();
            Load();
        }

        public void OnXamlRendered(FrameworkElement control)
        {
            // Need to use logical tree here as scrollviewer hasn't initialized yet even with dispatch.
            container = control.FindChildByName("Container") as StackPanel;
            resources = control.Resources;
            lazyLoadingControlHost = control.FindChildByName("LazyLoadingControlHost") as Border;
        }

        private async void Load()
        {
            SampleController.Current.RegisterNewCommand("Image with placeholder", (sender, args) =>
            {
                AddImage(false, true);
            });

            SampleController.Current.RegisterNewCommand("Image with placeholder (invalid link or offline)", (sender, args) =>
            {
                AddImage(true, true);
            });

            SampleController.Current.RegisterNewCommand("Image without placeholder", (sender, args) =>
            {
                AddImage(false, false);
            });

            SampleController.Current.RegisterNewCommand("Round Image with placeholder", (sender, args) =>
            {
                AddImage(false, true, true);
            });

            SampleController.Current.RegisterNewCommand("Round Image with placeholder (invalid link or offline)", (sender, args) =>
            {
                AddImage(true, true, true);
            });

            SampleController.Current.RegisterNewCommand("Round Image without placeholder", (sender, args) =>
            {
                AddImage(false, false, true);
            });

            SampleController.Current.RegisterNewCommand("Lazy loading sample (17763 or higher supported)", (sender, args) =>
            {
                var imageExLazyLoadingControl = new ImageExLazyLoadingControl();
                imageExLazyLoadingControl.CloseButtonClick += (_, __) =>
                {
                    if (lazyLoadingControlHost != null)
                    {
                        lazyLoadingControlHost.Child = null;
                    }
                };

                if (lazyLoadingControlHost != null)
                {
                    lazyLoadingControlHost.Child = imageExLazyLoadingControl;
                }
            });

            SampleController.Current.RegisterNewCommand("Clear image cache", async (sender, args) =>
            {
                container?.Children?.Clear();
                GC.Collect(); // Force GC to free file locks
                await ImageCache.Instance.ClearAsync();
            });

            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            photos = await new PhotosDataSource().GetItemsAsync(true);
        }

        private void AddImage(bool broken, bool placeholder, bool round = false)
        {
            ImageEx newImage = new ImageEx();
            newImage.Style = resources["BaseStyle"] as Style;

            if (round)
            {
                newImage.CornerRadius = new CornerRadius(999);
            }

            newImage.Source = broken ? photos[imageIndex].Thumbnail + "broken" : photos[imageIndex].Thumbnail;

            if (placeholder)
            {
                newImage.PlaceholderSource = new BitmapImage(new Uri("ms-appx:///Assets/Photos/ImageExPlaceholder.jpg"));
            }

            container?.Children?.Add(newImage);

            // Move to next image
            imageIndex++;
            if (imageIndex >= photos.Count)
            {
                imageIndex = 0;
            }
        }
    }
}