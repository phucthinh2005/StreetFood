using System;
using System.Collections.Generic;
using System.Text;

using MauiApp1.ViewModels;

namespace MauiApp1.Views;

[QueryProperty(nameof(ViewModel), "ViewModel")]
public partial class ListPage : ContentPage
{
    public MapViewModel ViewModel
    {
        set => BindingContext = value;
    }

    public ListPage()
    {
        InitializeComponent();
    }
}