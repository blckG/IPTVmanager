﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Threading;
using System.Diagnostics;

namespace IPTVman.ViewModel
{
    /// <summary>
    /// Логика взаимодействия для WindowWAIT.xaml
    /// </summary>
    public partial class WindowWAIT : Window
    {
        System.Timers.Timer Timer1;

        public void CreateTimer1(int ms)
        {
            if (Timer1 == null)
            {
                Timer1 = new System.Timers.Timer();
                //Timer1.AutoReset = false; //
                Timer1.Interval = ms;
                Timer1.Elapsed += Timer1Tick;
                Timer1.Enabled = true;
                Timer1.Start();
            }
        }

        private void Timer1Tick(object source, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                ProgressBar1.Dispatcher.Invoke(new Action(() =>
                {
                    ProgressBar1.Maximum = Model.GUI.progressbar_max;
                    ProgressBar1.Value = Model.GUI.progressbar;
                    ProgressBar1.IsIndeterminate = Model.GUI.dynamic_progressbar;
                    
                }));

                double proc = 100 * (Model.GUI.progressbar / Model.GUI.progressbar_max);
                if (proc > 100) proc = 100;

                txtMessage.Dispatcher.Invoke(new Action(() =>
                {
                    if (!Model.GUI.dynamic_progressbar)
                    {
                        txtMessage.Text = Wait.message + " " +
                        String.Format("{0:f1}%", proc);
                    }
                    else
                    {
                        txtMessage.Text = Wait.message;
                    }
                }));

                //BindingExpression be = ProgressBar1.GetBindingExpression(ProgressBar.ValueProperty);
                //if (be != null) be.UpdateSource();

            }
            catch { }

        }

        public WindowWAIT()
        {
            InitializeComponent();
            txtMessage.Text = Wait.message;
            CreateTimer1(500);
            this.KeyDown += new System.Windows.Input.KeyEventHandler(Window1_KeyDown);
        }

        void Window1_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter) this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {  
          if (Wait.open) e.Cancel = true;//запрет закрытия
        }
 
    }



    public static class Wait
    {
        public static bool open = false;//state window
        public static Window wait = null;

        public static bool need_close = false;
        public static bool create = false;
        public static string message = "";
        public static bool en_progress = false;

        static byte delay_open = 0;

        public static void Create(string mes, bool en_dynamic_progressbar)
        {
            message = mes;
            en_progress = en_dynamic_progressbar;
            create = true;
        }

            private static void Create_on_timer()
            {
                if (IsOpen) Close_on_timer();
                create = false;

                Model.GUI.progressbar = 0;
                if (en_progress) Model.GUI.dynamic_progressbar = false;
                     else Model.GUI.dynamic_progressbar = true;
                Model.GUI.longtaskSTRING = message;

                wait = new WindowWAIT()
                {
                    Title = "",
                    Topmost = true,
                    WindowStyle = WindowStyle.ThreeDBorderWindow,
                    Name = "winwait"
                };

                wait.Show();
                wait.Owner = MainWindow.header;
                open = true;
            }

        public static bool IsOpen
        {
            get
            {
                if (open) return true;
                else return false;
            }
        }

        private static void Close_on_timer()
        {
            if (wait == null) return;
            open = false;
            need_close = false;
            create = false;
            wait.Close();
        }
        public static void Close()
        {
            need_close = true; 
        }

        public static void manager()//работа из UI потока
        {
            if (open)
            {
                if (create) create = false;
                if (need_close) { delay_open = 0;  Close_on_timer(); return; }
            }
            else
            {
                if (need_close) { create = false; need_close = false; delay_open = 0; return; }
                if (create)
                {
                    delay_open++;
                    if (delay_open > 1)  Create_on_timer(); 
                }
            }
        }

    }



    public static class LongtaskPingCANCELING
    {
        static PING _ping;
        static PING_prepare _ping_prepare;

        static bool en;
        public static bool isENABLE()
        {

            if (_ping_prepare == null) return en;
            else
            {
                if (_ping_prepare.stateTASKisCanceled()) { stop(); return false; }
                else return true;
            }
        }

        public static void enable(PING p, PING_prepare pp)
        {
            _ping = p;
            _ping_prepare = pp;
            en = true;
        }

        public static void enable()
        {
            en = true;
        }

        public static void stop()
        {
            _ping = null;
            _ping_prepare = null;
            en = false;
            Wait.Close();
        }

        public static void analiz_closing_thread(PING p, PING_prepare prep)
        {

            if (p != null) p.stop();
            if (prep != null)
            {
                prep.stop();

                byte ct = 0;
                while (!prep.stateTASKisCanceled())
                {
                    ct++;
                    if (ct == 5)
                    {
                        LongtaskPingCANCELING.enable(); 
                        Wait.Create("Ждите идет прерывание пинга", false);
                        LongtaskPingCANCELING.enable(p, prep);
                        break;
                    }
                    Thread.Sleep(100);
                }

            }

            prep = null;
            p = null;

        }


    }

}
