// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using SimpleModManager.Models;
using SimpleModManager.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json;
using System.Text.Json.Serialization;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SimpleModManager.UI
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private SettingsService _settingsService;
        private SettingsModel _settings;
        private int _selectedGame;
        public int SelectedGame
        {
            get { return _selectedGame; }
            set
            {
                ListModsLoaded.Items.Clear();
                ListModsUnloaded.Items.Clear();
                ButtonEdit.IsEnabled = false;
                ButtonDelete.IsEnabled = false;
                ButtonOpenGameDirectory.IsEnabled = false;
                ButtonOpenModDirectory.IsEnabled = false;
                ButtonRefresh.IsEnabled = false;

                if (value >= StackGames.Children.Count)
                    return;

                foreach (Control item in StackGames.Children)
                {
                    item.IsEnabled = true;
                }

                (StackGames.Children[value] as Control).IsEnabled = false;

                if (!Directory.Exists(_settings.Games[value].GameDirectory))
                    return;
                if (!Directory.Exists(_settings.Games[value].ModDirectory))
                    return;

                var modDirs = Directory.GetDirectories(_settings.Games[value].ModDirectory);
                foreach (var modDir in modDirs)
                {
                    var files = Directory.EnumerateFiles(modDir, "*.*", SearchOption.AllDirectories);
                    bool loaded = true;
                    foreach (var file in files)
                    {
                        var relative = System.IO.Path.GetRelativePath(modDir, file);
                        var combined = System.IO.Path.Combine(_settings.Games[value].GameDirectory, relative);
                        if (!File.Exists(combined))
                        {
                            loaded = false;
                            break;
                        }
                    }

                    var listItem = new ListBoxItem()
                    {
                        Content = System.IO.Path.GetFileName(modDir),
                        Tag = modDir,
                    };

                    if (loaded)
                        ListModsLoaded.Items.Add(listItem);
                    else
                        ListModsUnloaded.Items.Add(listItem);
                }
                _selectedGame = value;

                ButtonEdit.IsEnabled = true;
                ButtonDelete.IsEnabled = true;
                ButtonOpenGameDirectory.IsEnabled = true;
                ButtonOpenModDirectory.IsEnabled = true;
                ButtonRefresh.IsEnabled = true;
            }
        }

        public MainWindow(SettingsService settingsService)
        {
            this.InitializeComponent();
            this.Title = "Simple Mod Manager";
            _settingsService = settingsService;
            LoadGames();
        }

        private void LoadGames()
        {
            _settings = _settingsService.Read();
            StackGames.Children.Clear();
            for (int i = 0; i < _settings.Games.Count; i++)
            {
                var btn = new Button()
                {
                    Content = _settings.Games[i].Name,
                    Tag = i,
                };
                btn.Click += (sender, e) =>
                {
                    SelectedGame = (int)(sender as Button).Tag;
                };
                StackGames.Children.Add(btn);
            }

            SelectedGame = 0;
        }

        private void LoadMod(string gameDirectory, string sourceDirectory)
        {
            var allDirectories = Directory.GetDirectories(sourceDirectory, "*", SearchOption.AllDirectories);
            foreach (string directory in allDirectories)
            {
                var relative = System.IO.Path.GetRelativePath(sourceDirectory, directory);
                var combined = System.IO.Path.Combine(gameDirectory, relative);
                if(Directory.Exists(directory))
                    Directory.CreateDirectory(combined);
            }
            var allFiles = Directory.GetFiles(sourceDirectory, "*.*", SearchOption.AllDirectories);
            foreach (string file in allFiles)
            {
                var relative = System.IO.Path.GetRelativePath(sourceDirectory, file);
                var combined = System.IO.Path.Combine(gameDirectory, relative);
                if(File.Exists(file))
                    File.Copy(file, combined, true);
            }
        }

        private void UnloadMod(string gameDirectory, string sourceDirectory)
        {
            var allFiles = Directory.GetFiles(sourceDirectory, "*.*", SearchOption.AllDirectories);
            foreach (string file in allFiles)
            {
                var relative = System.IO.Path.GetRelativePath(sourceDirectory, file);
                var combined = System.IO.Path.Combine(gameDirectory, relative);
                if(File.Exists(combined))
                    File.Delete(combined);
            }
            var allDirectories = Directory.GetDirectories(sourceDirectory, "*", SearchOption.AllDirectories);
            foreach (string directory in allDirectories)
            {
                var relative = System.IO.Path.GetRelativePath(sourceDirectory, directory);
                var combined = System.IO.Path.Combine(gameDirectory, relative);
                if(Directory.Exists(combined))
                    Directory.Delete(combined, true);
            }
        }

        private async void ButtonAddGame_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new EditGame(this, new SettingsModel.Game()) { Title = "Add Game" };
            // Use this code to associate the dialog to the appropriate AppWindow by setting
            // the dialog's XamlRoot to the same XamlRoot as an element that is already present in the AppWindow.
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 8))
            {
                dialog.XamlRoot = (sender as Control).XamlRoot;
            }
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                _settings.Games.Add(dialog.Game);
                _settingsService.Write(_settings);
            }
            LoadGames();
        }

        private void ButtonModsLoad_Click(object sender, RoutedEventArgs e)
        {
            foreach(var selected in ListModsUnloaded.SelectedItems)
            {
                var path = (string)(selected as Control).Tag;
                LoadMod(_settings.Games[SelectedGame].GameDirectory, path);
            }
            //Reload
            SelectedGame = SelectedGame;
        }

        private void ButtonModsUnload_Click(object sender, RoutedEventArgs e)
        {
            var unselected = ListModsLoaded.Items.Where(x => !ListModsLoaded.SelectedItems.Contains(x));
            foreach (var item in ListModsLoaded.Items)
            {
                var path = (string)(item as Control).Tag;
                UnloadMod(_settings.Games[SelectedGame].GameDirectory, path);
            }
            foreach (var item in unselected)
            {
                var path = (string)(item as Control).Tag;
                LoadMod(_settings.Games[SelectedGame].GameDirectory, path);
            }
            //Reload
            SelectedGame = SelectedGame;
        }

        private void ListModsUnloaded_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(ListModsUnloaded.SelectedItems.Count < 1)
                ButtonModsLoad.IsEnabled = false;
            else
                ButtonModsLoad.IsEnabled = true;
        }

        private void ListModsLoaded_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListModsLoaded.SelectedItems.Count < 1)
                ButtonModsUnload.IsEnabled = false;
            else
                ButtonModsUnload.IsEnabled = true;
        }

        private void ButtonOpenGameDirectory_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(_settings.Games[SelectedGame].GameDirectory))
                return;

            ProcessStartInfo processInfo = new ProcessStartInfo()
            {
                Arguments = _settings.Games[SelectedGame].GameDirectory,
                FileName = "explorer.exe",
            };
            Process.Start(processInfo);
        }

        private void ButtonOpenModDirectory_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(_settings.Games[SelectedGame].ModDirectory))
                return;

            ProcessStartInfo processInfo = new ProcessStartInfo()
            {
                Arguments = _settings.Games[SelectedGame].ModDirectory,
                FileName = "explorer.exe",
            };
            Process.Start(processInfo);
        }

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            SelectedGame = SelectedGame;
        }

        private async void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new EditGame(this, _settings.Games[SelectedGame]);
            // Use this code to associate the dialog to the appropriate AppWindow by setting
            // the dialog's XamlRoot to the same XamlRoot as an element that is already present in the AppWindow.
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 8))
            {
                dialog.XamlRoot = (sender as Control).XamlRoot;
            }
            var result = await dialog.ShowAsync();
            if(result == ContentDialogResult.Primary)
            {
                _settings.Games[SelectedGame] = dialog.Game;
                _settingsService.Write(_settings);
            }
            LoadGames();
        }

        private async void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Delete game?",
                Content = $"Are you sure you want to delete '{_settings.Games[SelectedGame].Name}'?",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel"
            };
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 8))
            {
                dialog.XamlRoot = (sender as Control).XamlRoot;
            }

            ContentDialogResult result = await dialog.ShowAsync();

            // Delete the file if the user clicked the primary button.
            /// Otherwise, do nothing.
            if (result != ContentDialogResult.Primary)
                return;

            _settings.Games.RemoveAt(SelectedGame);
            _settingsService.Write(_settings);
            LoadGames();
        }
    }
}
