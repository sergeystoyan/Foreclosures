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
using System.Threading;
using System.Net.Http;
using System.IO;
using LiteDB;
using System.Reflection;

namespace Cliver.Probidder
{
    public partial class Db
    {
        public static bool RefreshRuns
        {
            get
            {
                return (refresh_t != null && refresh_t.IsAlive);
            }
        }

        public delegate void OnRefreshStateChanged();
        public static event OnRefreshStateChanged RefreshStateChanged = null;

        public static Thread BeginRefresh(bool show_start_notification)
        {
            lock (o)
            {
                if (refresh_t != null && refresh_t.IsAlive)
                    return refresh_t;

                DateTime refresh_started = DateTime.Now;
                refresh_t = ThreadRoutines.StartTry(() => { refresh(show_start_notification); });
                return refresh_t;
            }
        }
        static Thread refresh_t = null;
        static readonly object o = new object();

        static bool refresh(bool show_start_notification)
        {
            lock (o1)
            {
                DateTime refresh_started = DateTime.Now;
                Wpf.MessageWindow mw = null;

                try
                {
                    RefreshStateChanged?.BeginInvoke(null, null);
                    Log.Main.Inform("Refreshing database.");

                    if (show_start_notification)
                    {
                        ThreadRoutines.StartTrySta(() =>
                        {
                            mw = new Wpf.MessageWindow(System.Windows.Forms.Application.ProductName, System.Drawing.SystemIcons.Exclamation, "Getting data from the net. Please wait...", new string[1] { "OK" }, 0, null);
                            mw.ShowDialog();
                        });
                        if (SleepRoutines.WaitForObject(() => { return mw; }, 10000) == null)
                            Log.Main.Exit("SleepRoutines.WaitForObject got null");
                        //InfoWindow iw = InfoWindow.Create("Foreclosures", "Refreshing database... Please wait for completion.", null, "OK", null);
                    }

                    HttpClientHandler handler = new HttpClientHandler();
                    HttpClient http_client = new HttpClient(handler);

                    List<Task> tasks = new List<Task>();
                    var t = new Task(() =>
                    {
                        Plaintiffs.RefreshFile();
                    });
                    t.Start();
                    tasks.Add(t);
                    t = new Task(() =>
                    {
                        ForeclosureCaseNumbers.RefreshFile();
                    });
                    t.Start();
                    tasks.Add(t);
                    t = new Task(() =>
                    {
                        ProbateCaseNumbers.RefreshFile();
                    });
                    t.Start();
                    tasks.Add(t);
                    t = new Task(() =>
                    {
                        ProbateAttorneyPhones.RefreshFile();
                    });
                    t.Start();
                    tasks.Add(t);
                    t = new Task(() =>
                    {
                        ProbateAttorneys.RefreshFile();
                    });
                    t.Start();
                    tasks.Add(t);
                    t = new Task(() =>
                    {
                        ForeclosureAttorneyPhones.RefreshFile();
                    });
                    t.Start();
                    tasks.Add(t);
                    t = new Task(() =>
                    {
                        ForeclosureAttorneys.RefreshFile();
                    });
                    t.Start();
                    tasks.Add(t);
                    t = new Task(() =>
                    {
                        MortgageTypes.RefreshFile();
                    });
                    t.Start();
                    tasks.Add(t);
                    t = new Task(() =>
                    {
                        Cities.RefreshFile();
                    });
                    t.Start();
                    tasks.Add(t);
                    t = new Task(() =>
                    {
                        Zips.RefreshFile();
                    });
                    t.Start();
                    tasks.Add(t);

                    Counties.RefreshFile();
                    OwnerRoles.RefreshFile();
                    PropertyCodes.RefreshFile();

                    Task.WaitAll(tasks.ToArray());
                    //Log.Inform("Db has been refreshed.");
                    //iw.Dispatcher.Invoke(iw.Close);
                    Settings.Database.LastRefreshTime = DateTime.Now;
                    Settings.Database.Save();
                    if (Settings.Database.RefreshPeriodInSecs > 0)
                        Settings.Database.NextRefreshTime = refresh_started.AddSeconds(Settings.Database.RefreshPeriodInSecs);
                    Db.Reopen();
                    try
                    {
                        mw?.Close();
                    }
                    catch { }
                    InfoWindow.Create(ProgramRoutines.GetAppName(), "Database has been refreshed successfully.", null, "OK", null, System.Windows.Media.Brushes.White, System.Windows.Media.Brushes.Green);
                    return true;
                }
                catch (Exception e)
                {
                    Log.Main.Error("Could not refresh database.", e);
                    if (Settings.Database.RefreshRetryPeriodInSecs > 0)
                        Settings.Database.NextRefreshTime = refresh_started.AddSeconds(Settings.Database.RefreshRetryPeriodInSecs);

                    try
                    {
                        mw?.Close();
                    }
                    catch { }
                    InfoWindow.Create(ProgramRoutines.GetAppName() + ": database could not be refreshed. Check connection to the internet.", Log.GetExceptionMessage(e), null, "OK", null, System.Windows.Media.Brushes.WhiteSmoke, System.Windows.Media.Brushes.Red);
                    return false;
                }
                finally
                {
                    Settings.Database.Save();
                    ThreadRoutines.StartTry(() => {
                        if (refresh_t != null)
                            refresh_t.Join();
                        RefreshStateChanged?.BeginInvoke(null, null);
                    });
                }
            }
        }
        static readonly object o1 = new object();
    }
}