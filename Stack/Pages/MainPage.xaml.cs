using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Stack.ViewModels;
using System;
using System.Collections.Generic;
using System.Timers;

namespace Stack.Pages
{
    public partial class MainPage : ContentPage
    {
        private PartidaVM ViewModel;

        public MainPage()
        {
            InitializeComponent();
            ViewModel = new PartidaVM(RomboGraphicsView);
            BindingContext = ViewModel;  // Enlazar la vista con el ViewModel
        }
    }
}
