﻿using System;
using System.Windows;

namespace BudgetAnalyser
{
    /// <summary>
    ///     Interaction logic for ShellWindow.xaml
    /// </summary>
    public partial class ShellWindow
    {
        private bool sizeHasBeenSet;

        public ShellWindow()
        {
            InitializeComponent();
        }

        private ShellController Controller
        {
            get { return (ShellController)DataContext; }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Controller.OnViewReady();

            // Data binding these properties doesnt seem to work so well. Desired values are overwritten with other values multiple times.
            Width = Controller.WindowSize.X;
            Height = Controller.WindowSize.Y;
            Left = Controller.WindowTopLeft.X;
            Top = Controller.WindowTopLeft.Y;

            // This is necessary because the controller must not be notified until the above sizes have been set, this doesnt happen immediately otherwise the desired value is overwritten.
            this.sizeHasBeenSet = true;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.sizeHasBeenSet)
            {
                Controller.NotifyOfWindowSizeChange(new Point(ActualWidth, ActualHeight));
            }
        }

        private void OnLocationChanged(object sender, EventArgs e)
        {
            if (this.sizeHasBeenSet)
            {
                Controller.NotifyOfWindowLocationChange(new Point(Left, Top));
            }
        }
    }
}