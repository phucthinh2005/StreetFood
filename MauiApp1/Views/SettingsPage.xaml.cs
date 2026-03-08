using System;
using System.Collections.Generic;
using System.Text;

namespace MauiApp1.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
    }

    void VolumeChanged(object sender, ValueChangedEventArgs e)
    {
        volumeLabel.Text = $"Âm lượng: {(int)e.NewValue}%";
    }

    void SpeedChanged(object sender, ValueChangedEventArgs e)
    {
        speedLabel.Text = $"Tốc độ giọng nói: {e.NewValue:F1}x";
    }

    void RadiusChanged(object sender, ValueChangedEventArgs e)
    {
        radiusLabel.Text = $"Bán kính thông báo mặc định: {(int)e.NewValue}m";
    }
}
