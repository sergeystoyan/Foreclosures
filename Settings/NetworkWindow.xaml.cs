﻿/********************************************************************************************
        Author: Sergey Stoyan
        sergey.stoyan@gmail.com
        http://www.cliversoft.com
********************************************************************************************/
using System;
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
using System.Windows.Navigation;
//using System.Windows.Shapes;
using System.Net.Http;
using System.Net;
using System.IO;
using System.Management;
using System.Threading;

namespace Cliver.Foreclosures
{
    public partial class NetworkWindow : Window
    {
        public static void OpenDialog()
        {
            var w = new NetworkWindow();
            System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(w);
            w.ShowDialog();
        }

        NetworkWindow()
        {
            InitializeComponent();

            Icon = AssemblyRoutines.GetAppIconImageSource();

            Closing += delegate (object sender, System.ComponentModel.CancelEventArgs e)
            {
            };

            Closed += delegate
            {
            };

            ContentRendered += delegate
            {
                WpfRoutines.TrimWindowSize(this);
            };

            ContentRendered += delegate
            {
                WpfRoutines.TrimWindowSize(this);
            };

            UserName.Text = Settings.Network.UserName;
            if (!string.IsNullOrWhiteSpace(Settings.Network.EncryptedPassword))
                Password.Password = Settings.Network.Password();
            ExportUrl.Text = Settings.Network.ExportUrl;
        }

        private void close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(UserName.Text))
                    throw new Exception("User Name is not set.");
                Settings.Network.UserName = UserName.Text;
                if (string.IsNullOrWhiteSpace(Password.Password))
                    throw new Exception("Password is not set.");
                Settings.Network.EncryptedPassword = Settings.Network.Encrypt(Password.Password);
                if (string.IsNullOrWhiteSpace(ExportUrl.Text))
                    throw new Exception("Export Url is not set.");
                Settings.Network.ExportUrl = ExportUrl.Text;

                Settings.Network.Save();
                Config.Reload();

                Close();
            }
            catch (Exception ex)
            {
                Message.Exclaim(ex.Message);
            }
        }
    }
}