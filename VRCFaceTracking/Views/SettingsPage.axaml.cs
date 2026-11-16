using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.DependencyInjection;
using FluentAvalonia.Styling;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.ViewModels;

namespace VRCFaceTracking.Views;

public partial class SettingsPage : UserControl
{
    private SettingsViewModel ViewModel => (SettingsViewModel)DataContext!;

    private readonly ILocalSettingsService _settingsService;
    private bool _suppressLanguageChange;

    public SettingsPage()
    {
        InitializeComponent();
        DataContext = Ioc.Default.GetRequiredService<SettingsViewModel>();
        _settingsService = Ioc.Default.GetRequiredService<ILocalSettingsService>();

        // Show current version
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        VersionText.Text = $"Version {version?.ToString(3) ?? "Unknown"}";

        var faTheme = Application.Current?.Styles.OfType<FluentAvaloniaTheme>().FirstOrDefault();
        if (faTheme != null)
        {
            ThemeCombo.SelectedIndex = Application.Current?.RequestedThemeVariant?.Key?.ToString() switch
            {
                "Light" => 0,
                "Dark" => 1,
                _ => 2
            };
        }

        _ = LoadLanguageAsync();
    }

    private async Task LoadLanguageAsync()
    {
        var saved = await _settingsService.ReadSettingAsync(VRCFaceTracking.Core.Utils.LanguageSettingsKey, string.Empty);

        _suppressLanguageChange = true;
        LanguageCombo.SelectedIndex = LanguageCodeToIndex(saved);
        _suppressLanguageChange = false;
    }

    private static int LanguageCodeToIndex(string? code) => code switch
    {
        "en-US" => 1,
        "zh-CN" => 2,
        "es-ES" => 3,
        "ja-JP" => 4,
        "pl-PL" => 5,
        "ko-KR" => 6,
        _ => 0,
    };

    private async void LanguageCombo_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_suppressLanguageChange || LanguageCombo.SelectedItem is not ComboBoxItem item)
        {
            return;
        }

        var code = item.Tag?.ToString() ?? string.Empty;
        await _settingsService.SaveSettingAsync(VRCFaceTracking.Core.Utils.LanguageSettingsKey, code);
        LanguageRestartHint.IsVisible = true;
    }

    private void ThemeCombo_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (ThemeCombo.SelectedItem is not ComboBoxItem item) return;

        Application.Current!.RequestedThemeVariant = item.Tag?.ToString() switch
        {
            "Light" => ThemeVariant.Light,
            "Dark" => ThemeVariant.Dark,
            _ => ThemeVariant.Default
        };
    }

    private void ForceReInit_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ViewModel.RiskySettings.ForceReInit();
    }

    private void ResetVRCFT_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ViewModel.RiskySettings.ResetVRCFT();
    }

    private void ResetAvatarConfig_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ViewModel.RiskySettings.ResetAvatarOscManifests();
    }

    private async void ContributorButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is Button { Tag: string url } && !string.IsNullOrEmpty(url))
        {
            try
            {
                var launcher = TopLevel.GetTopLevel(this)?.Launcher;
                if (launcher != null)
                    await launcher.LaunchUriAsync(new Uri(url));
            }
            catch { }
        }
    }
}
