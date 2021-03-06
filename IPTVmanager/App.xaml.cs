﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace IPTVman
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App
    {
        void IconMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1)
                sender.ForWindowFromTemplate(w => SystemCommands.CloseWindow(w));
        }

        void IconMouseUp(object sender, MouseButtonEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element != null)
            {
                var point = element.PointToScreen(new Point(element.ActualWidth / 2, element.ActualHeight));
                sender.ForWindowFromTemplate(w => SystemCommands.ShowSystemMenu(w, point));
            }
        }

        void WindowLoaded(object sender, RoutedEventArgs e)
        {
            ((Window)sender).StateChanged += WindowStateChanged;
        }

        void WindowStateChanged(object sender, EventArgs e)
        {
            var w = ((Window)sender);
            var handle = w.GetWindowHandle();
            var containerBorder = (Border)w.Template.FindName("PART_Container", w);

            if (w.WindowState == WindowState.Maximized)
            {
                // Make sure window doesn't overlap with the taskbar.
                var screen = System.Windows.Forms.Screen.FromHandle(handle);

                containerBorder.Padding = new Thickness(
                    SystemParameters.WorkArea.Left + 7,
                    SystemParameters.WorkArea.Top + 7,
                    (SystemParameters.PrimaryScreenWidth - SystemParameters.WorkArea.Right) + 7,
                    (SystemParameters.PrimaryScreenHeight - SystemParameters.WorkArea.Bottom) + 5);

            }
            else
            {
                containerBorder.Padding = new Thickness(0, 0, 0, 0);
            }
        }

        void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            sender.ForWindowFromTemplate(w => SystemCommands.CloseWindow(w));
        }

        void MinButtonClick(object sender, RoutedEventArgs e)
        {

            sender.ForWindowFromTemplate(w => SystemCommands.MinimizeWindow(w));
        }

        void MaxButtonClick(object sender, RoutedEventArgs e)
        {
            sender.ForWindowFromTemplate(w =>
            {
                if (w.WindowState == WindowState.Maximized) SystemCommands.RestoreWindow(w);
                else SystemCommands.MaximizeWindow(w);
            });
        }

    }

    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (Model.loc.start_one) return;
            if (Model.data.arguments_startup == null) Model.data.arguments_startup = new string[100];

            try
            {
                byte i = 0;
                foreach (string arg in e.Args)
                {
                    Model.data.arguments_startup[i] = arg;
                    if (i > 90) break;
                    i++;
                }
                if (i > 0) Model.data.arguments_start = true;
            }
            catch { }

            //test
           // Model.data.arguments_startup[0] = "radio.m3u"; Model.data.arguments_start = true;

        }
    }

    internal static class LocalExtensions
    {
        public static void ForWindowFromTemplate(this object templateFrameworkElement, Action<Window> action)
        {
            Window window = ((FrameworkElement)templateFrameworkElement).TemplatedParent as Window;
            if (window != null) action(window);
        }

        public static IntPtr GetWindowHandle(this Window window)
        {
            WindowInteropHelper helper = new WindowInteropHelper(window);
            return helper.Handle;
        }
    }
}
