// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using SimpleModManager.Models;
using SimpleModManager.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json;
using System.Text.Json.Serialization;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SimpleModManager.UI
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EditGame : ContentDialog
    {
        public SettingsModel.Game Game;
        private Window _parent;
        public EditGame(Window parent, SettingsModel.Game game)
        {
            this.InitializeComponent();
            Game = game;
            _parent = parent;

            TextGameName.Text = Game.Name;
            TextGameDirectory.Text = Game.GameDirectory;
            TextModDirectory.Text = Game.ModDirectory;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Ensure the user name and password fields aren't empty. If a required field
            // is empty, set args.Cancel = true to keep the dialog open.
            if (string.IsNullOrEmpty(TextGameName.Text))
            {
                args.Cancel = true;
                return;
            }
            if (string.IsNullOrEmpty(TextGameDirectory.Text))
            {
                args.Cancel = true;
                return;
            }
            if (string.IsNullOrEmpty(TextModDirectory.Text))
            {
                args.Cancel = true;
                return;
            }

            // If you're performing async operations in the button click handler,
            // get a deferral before you await the operation. Then, complete the
            // deferral when the async operation is complete.
            Game.Name = TextGameName.Text;
            Game.GameDirectory = TextGameDirectory.Text;
            Game.ModDirectory = TextModDirectory.Text;
        }

        private async void ButtonGameDirectoryFolder_Click(object sender, RoutedEventArgs e)
        {
            var folderPicker = new FolderPicker();
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, WinRT.Interop.WindowNative.GetWindowHandle(_parent));
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");
            Windows.Storage.StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                // Application now has read/write access to all contents in the picked folder
                // (including other sub-folder contents)
                TextGameDirectory.Text = folder.Path;
            }
        }

        private async void ButtonModDirectoryFolder_Click(object sender, RoutedEventArgs e)
        {
            var folderPicker = new FolderPicker();
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, WinRT.Interop.WindowNative.GetWindowHandle(_parent));
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");
            Windows.Storage.StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                // Application now has read/write access to all contents in the picked folder
                // (including other sub-folder contents)
                TextModDirectory.Text = folder.Path;
            }
        }
    }
}
