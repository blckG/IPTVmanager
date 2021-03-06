﻿using System.Windows;
using System.Windows.Threading;
using System;
using System.Threading;

namespace IPTVman.ViewModel
{
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
            //Configure the ProgressBar
            ProgressBar1.Minimum = 0;
            ProgressBar1.Maximum = 255;
            ProgressBar1.Value = 0;
            textBoxPING.Text="";
            textBoxPING2.Text = "";
            if (IPTVman.Model.data.type_player == 1) ViewModelWindow1._ch1 = true;
            else if (IPTVman.Model.data.type_player == 2) ViewModelWindow1._ch2 = true;
            else { ViewModelWindow1._ch1 = false; ViewModelWindow1._ch2 = false; }
            //use a timer to periodically update the memory usage
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += timer_Tick;
            timer.Start();
        }


        private void timer_Tick(object sender, EventArgs e)
        {

            if (ViewModelWindow1._ping == null) return;

            if (ViewModelWindow1.edit.ping!="")
            {
                textBoxPING.Text = ViewModelWindow1._ping.result;
                ProgressBar1.Value = 0;
            }
            else ProgressBar1.Value += 3;
           
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (ViewModelWindow1._ping == null) return;
            LongtaskPingCANCELING.analiz_closing_thread(ViewModelWindow1._ping, ViewModelWindow1._pingPREPARE);
        }


        //key PLAY
        private void Button_Copy_Click(object sender, RoutedEventArgs e)
        {
            Model.play.URLPLAY = urlTEXT.Text;
            Model.play.name = textBox.Text;
            exit();
        }

        private void ButtonBEST_Click(object sender, RoutedEventArgs e)
        {
            if (!Model.loc.keyadd) WinPOP.Create("Добавлено",2, this);
        }

        void exit()
        {

            if (ViewModelWindow1._pingPREPARE != null) ViewModelWindow1._pingPREPARE.stop();
            if (ViewModelWindow1._ping != null) ViewModelWindow1._ping.stop();
            this.Close();
        }

        //key SAVE
        private void exit_Copy_Click(object sender, RoutedEventArgs e)
        {
            exit();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
