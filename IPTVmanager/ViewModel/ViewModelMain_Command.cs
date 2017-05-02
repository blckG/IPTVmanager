﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using IPTVman.Model;
using IPTVman.Helpers;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using System.Threading;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using System.Net.Sockets;


namespace IPTVman.ViewModel
{
    partial class ViewModelMain : ViewModelBase
    {

        public RelayCommand key_ADDCommand { get; set; }
        public RelayCommand key_OPENCommand { get; set; }
        public RelayCommand key_SAVECommand { get; set; }
        public RelayCommand key_delCommand { get; set; }

        public RelayCommand key_DelFILTER { get; set; }

        public RelayCommand key_DelALLkromeBEST { get; set; }
        public RelayCommand key_FILTERCommand { get; set; }

        public RelayCommand key_FilterOnlyBESTCommand { get; set; }

        void ini_command()
        {

            key_ADDCommand = new RelayCommand(key_ADD);
            key_OPENCommand = new RelayCommand(key_OPEN);
            key_SAVECommand = new RelayCommand(key_SAVE);
            key_delCommand = new RelayCommand(key_del);
            key_DelFILTER = new RelayCommand(key_delFILTER);
            key_FILTERCommand = new RelayCommand(key_FILTER);
            key_FilterOnlyBESTCommand = new RelayCommand(key_FILTERbest);
            key_DelALLkromeBEST = new RelayCommand(key_delALLkromeBEST);
        }


        //******************************* ADD **************
        void key_ADD(object parameter)
        {
            CollectionisCreate();
            if (parameter == null) return;
            myLISTfull.Add(new ParamCanal
            { name = parameter.ToString(), ExtFilter = parameter.ToString(), group_title = "" });
            
            RaisePropertyChanged("mycol");
            
        }
        

        void key_del(object parameter)
        {
            //if (parameter == null || !data.delete) return;
            if (myLISTfull == null) return;
            if (data.edit.name=="") return;

            MessageBoxResult result = MessageBox.Show("  УДАЛЕНИЕ " + data.edit.name + "\n" + data.edit.http, "  УДАЛЕНИЕ",
                                MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;


            var item = ViewModelMain.myLISTfull.Find(x =>
                  (x.http == data.edit.http && x.ExtFilter == data.edit.ExtFilter 
                      && x.group_title == data.edit.group_title));

            myLISTfull.Remove(item);

            RaisePropertyChanged("mycol");
        }


        void key_delFILTER(object parameter)
        {

            if (myLISTfull == null) return;
            if (data.edit.name=="") return;

            MessageBoxResult result = MessageBox.Show("  УДАЛЕНИЕ ВСЕХ ПО ФИЛЬТРУ !!!", "  УДАЛЕНИЕ",
                                MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            uint ct = 0;

            foreach (var obj in myLISTbase)
            {
                var item = ViewModelMain.myLISTfull.Find(x =>
                 (x.http == obj.http && x.ExtFilter == obj.ExtFilter && x.group_title == obj.group_title));

                if (item != null) { myLISTfull.Remove(item);  ct++; }

            }
         

            RaisePropertyChanged("mycol");
            MessageBox.Show("  УДАЛЕНО "+ct.ToString()+ " Каналов", " ",
                               MessageBoxButton.OK, MessageBoxImage.Information);

        }

        void key_delALLkromeBEST(object parameter)
        {
            if (myLISTfull == null) return;

            MessageBoxResult result = MessageBox.Show("  УДАЛЕНИЕ ВСЕХ КРОМЕ ИЗБРАННЫХ(ExtFilter)!!!", "  УДАЛЕНИЕ",
                                MessageBoxButton.YesNo, MessageBoxImage.Asterisk);
            if (result != MessageBoxResult.Yes) return;

            uint ct = 0;

            try
            {
                int i;
                for (i = 0; i < myLISTfull.Count; i++)
                {
                    if (myLISTfull[i].ExtFilter != data.favorite1_1 &&
                        myLISTfull[i].ExtFilter != data.favorite2_1 &&
                        myLISTfull[i].ExtFilter != data.favorite3_1
                        /*|| myLISTfull[i].group_title != data.best2*/)
                    {  myLISTfull.RemoveAt(i); ct++; i--; }

                }
            }
            catch (Exception ex)
            {

                MessageBox.Show("Ошибка удаления = "+ex.Message.ToString(),"",
                               MessageBoxButton.OK, MessageBoxImage.Information);
            }

            RaisePropertyChanged("mycol");
            MessageBox.Show("  УДАЛЕНО " + ct.ToString() + " Каналов", " ",
                               MessageBoxButton.OK, MessageBoxImage.Information);

        }

        
        void key_SAVE(object parameter)
        {

            SaveFileDialog openFileDialog = new SaveFileDialog();

            if (openFileDialog.ShowDialog() == true)
            {
                using (StreamWriter sr = new StreamWriter(openFileDialog.FileName))
                {
                    sr.Write(text_title +'\n');

                    string n = "";
                    foreach (var obj in myLISTfull)
                    {
                        n = "";
                       if (text_title == @"#EXTM3U $BorpasFileFormat="+'"'+'1'+'"') n += "#EXTINF:-1";
                       else n += "#EXTINF:0";

                        if (obj.ExtFilter != "") n += " $ExtFilter=" + '"' + obj.ExtFilter + '"';
                        if (obj.group_title != "") n += " group-title=" + '"' + obj.group_title + '"';
                        if (obj.logo != "") n+= " tvg-logo=" + '"' + obj.logo + '"';
                        if (obj.tvg_name != "") n += " tvg-name=" + '"' + obj.tvg_name + '"';

                        n += "," +obj.name + '\n';
                        sr.Write(n+ obj.http + '\n');
                    } 
                }

            }
        }

        void key_FILTER(object parameter)
        {
   
            UPDATE_FILTER("");
            RaisePropertyChanged("mycol");
           
        }

        void key_FILTERbest(object parameter)
        {

            UPDATE_FILTER("best");
            RaisePropertyChanged("mycol");
        }

      

        void key_OPEN(object parameter)
        {
            CollectionisCreate();
            Open();
        }

        public Task<string> AsyncTaskGet()
        {

            return Task.Run(() =>
            {
                //----------------

                PARSING_FILE();
                return "";

                //----------------
            });
        }


        async void Open()
        {
            string rez = await AsyncTaskGet();
        }

        void PARSING_FILE()
        {

            uint ct_dublicat = 0;
            string line = null;
            string newname = "";

            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();

                if (openFileDialog.ShowDialog() == true)
                {
                    Regex regex1 = new Regex("#EXTINF");
                    Regex regex2 = new Regex("#EXTM3U");
                    Match match = null;


                    win_loading = true;
                    //ПОИСК заголовка
                    using (StreamReader sr = new StreamReader(openFileDialog.FileName))
                    {
                        string header = "";
                        while (true)
                        {
                            try
                            {
                                header = sr.ReadLine();
                            }

                            catch
                            {

                                MessageBox.Show("Ошибка m3u", "нет заголовка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            }




                            if (header == null) header = "";
                            if (sr.EndOfStream) break;

                            match = regex2.Match(header);
                            if (match.Success) break;
                        }
                        text_title = header;
                    }



                    //ПОИСК каналов
                    using (StreamReader sr = new StreamReader(openFileDialog.FileName))
                    {

                        string str_ex = "", str_name = "", str_http = "", str_gt = "", str_logo = "", str_tvg = "";
                        bool yes = false;

                        while (!sr.EndOfStream)
                        {
                            try
                            {
                                line = sr.ReadLine();
                            }
                            catch { }

                            if (line == null || line == "") continue;


                            match = regex1.Match(line);
                            if (match.Success) yes = true; else yes = false;


                            ///========== разбор EXINF
                            if (yes)
                            {

                                Regex regex3 = new Regex("ExtFilter=", RegexOptions.IgnoreCase);
                                Regex regex4 = new Regex("group-title=", RegexOptions.IgnoreCase);
                                Regex regex5 = new Regex("logo=", RegexOptions.IgnoreCase);
                                Regex regex6 = new Regex("tvg-name=", RegexOptions.IgnoreCase);

                                string[] split = line.Split(new Char[] { '"' });


                                str_ex = ""; str_name = ""; str_http = ""; str_gt = ""; str_logo = ""; str_tvg = "";

                                if (split.Length <= 1)
                                {
                                    split = line.Split(new Char[] { ',' });
                                    str_name = split[split.Length - 1];
                                    newname = str_name;
                                }
                                else
                                {


                                    int i = 0;
                                    for (i = 0; i < split.Length; i++)
                                    {
                                        string s = split[i];
                                        //------------- разбор строки EXINF
                                        if (str_ex == "!") str_ex = s;
                                        if (str_gt == "!") str_gt = s;
                                        if (str_logo == "!") str_logo = s;
                                        if (str_tvg == "!") str_tvg = s;


                                        if (i >= split.Length - 1) { str_name = s; }

                                        match = regex3.Match(s);
                                        if (match.Success)
                                        {
                                            str_ex = "!";
                                        }

                                        match = regex4.Match(s);
                                        if (match.Success)
                                        {
                                            str_gt = "!";
                                        }

                                        match = regex5.Match(s);
                                        if (match.Success)
                                        {
                                            str_logo = "!";
                                        }

                                        match = regex6.Match(s);
                                        if (match.Success)
                                        {
                                            str_tvg = "!";
                                        }
                                    }//foreach

                                    newname = "";
                                    if (str_name != "") newname = str_name.Remove(0, 1);

                                }
                                str_http = sr.ReadLine();

                            }


                            var item = ViewModelMain.myLISTfull.Find(x =>
                            (x.http == str_http && x.ExtFilter == str_ex && x.group_title == str_gt));


                            if (newname.Trim() != "" && str_http.Trim() != "")
                            {
                                if (item == null)
                                {
                                    ViewModelMain.myLISTfull.Add(new ParamCanal
                                    {
                                        name = newname.Trim(),
                                        ExtFilter = str_ex.Trim(),
                                        http = str_http.Trim(),
                                        group_title = str_gt.Trim(),
                                        logo = str_logo.Trim(),
                                        tvg_name = str_tvg.Trim()

                                    });
                                }
                                else ct_dublicat++;
                            }

                        }///чтение фала


                    }

                }

                RaisePropertyChanged("mycol");///updte LIST!!
                RaisePropertyChanged("numberCANALS");
            }


            catch { }


            if (ct_dublicat != 0) MessageBox.Show("ПРОПУЩЕНО ДУБЛИРОВАННЫХ ССЫЛОК " + ct_dublicat.ToString(), " ",
                                MessageBoxButton.OK, MessageBoxImage.None);
           // if (Event_WIN_WAIT != null) Event_WIN_WAIT(2);
        }


    }//class

}//namespace
