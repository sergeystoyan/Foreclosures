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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;

namespace Cliver.Foreclosures
{
    /// <summary>
    /// Interaction logic for DatePickerControl.xaml
    /// </summary>
    public partial class DatePickerControl : DatePicker
    {
        public DatePickerControl()
        {
            InitializeComponent();

            GotKeyboardFocus += DatePickerControl_GotKeyboardFocus;
            LostKeyboardFocus += DatePickerControl_LostKeyboardFocus;

            Loaded += delegate
            {
                ThreadRoutines.StartTry(() =>
                {
                    DateTime end = DateTime.Now.AddMilliseconds(1000);
                    while (end > DateTime.Now)
                    {
                        TextBox tb1 = null;
                        Dispatcher.Invoke(() =>
                        {
                            tb1 = this.FindVisualChildrenOfType<TextBox>().Where(x => x.Name == "TextBox").FirstOrDefault();
                        });
                        if (tb1 != null)
                        {
                            tb = tb1;
                            Dispatcher.Invoke(() =>
                            {
                                DatePicker_SelectedDateChanged(null, null);
                            });
                            break;
                        }
                        System.Threading.Thread.Sleep(20);
                    }
                });

                tb = this.FindVisualChildrenOfType<TextBox>().FirstOrDefault();
            };
        }

        private void DatePickerControl_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            //focused = false;
            //TextBox_LostFocus(null, null);
        }

        private void DatePickerControl_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            //focused = true;
            ((DatePicker)sender).FocusOnText();
        }
        //bool focused = false;

        TextBox tb;
        readonly string mask = "__/__/__";

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime? dt = SelectedDate;
            if (dt == null)
            {
                if (tb.Text.Length < 1)
                    tb.Text = mask;
                return;
            }
            tb.Text = ((DateTime)dt).ToString("MM/dd/yyyy");
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            e.Handled = true;
            string t = tb.Text;
            if (t.Length < mask.Length)
            {
                int p = tb.SelectionStart;
                string s = mask.Substring(p, mask.Length - t.Length);
                tb.Text = t.Insert(p, s);
                tb.SelectionStart = p + s.Length;
                //for (int i = tb.SelectionStart; i < t.Length; i++)
                //{
                //    if (i == 2 || i == 5)
                //        t = t.Insert(i, "/");
                //    else if (t[i] == '/')
                //        t = t.Remove(i, 1);
                //}
                //string s = mask.Substring(t.Length, mask.Length - t.Length);
                //tb.Text = t + s;
                //tb.SelectionStart = p;
            }
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
            if (tb.SelectionStart >= mask.Length)
                return;
            int p = tb.SelectionStart;
            if (p == 2 || p == 5)
            {
                if (e.Text == "/")
                    return;
                p++;
            }
            if (Regex.IsMatch(e.Text, @"[^\d]"))
                return;
            string t = tb.Text.Remove(p, 1);
            tb.Text = t.Insert(p, e.Text);
            if (p == 1 || p == 4)
                p++;
            p++;
            tb.SelectionStart = p;
            //tb.SelectionLength = 1;
        }

        private DateTime? calendar_input(string text)
        {
            try
            {
                return DateTime.ParseExact(text, "MM/dd/yy", null);
            }
            catch
            {
            }
            try
            {
                return DateTime.ParseExact(text, "MM/dd/yyyy", null);
            }
            catch
            {
            }
            if (text.Length > 6 || Regex.IsMatch(text, @"[^\d]"))
                return null;
            Match m = Regex.Match(text, @"(\d{2})/(\d{2})/(\d{2})");
            if (!m.Success)
                return null;
            try
            {
                int y = int.Parse(m.Groups[3].Value);
                if (y < 30)
                    y += 2000;
                else
                    y += 1900;
                return new DateTime(y, int.Parse(m.Groups[1].Value), int.Parse(m.Groups[2].Value));
            }
            catch
            {
                return null;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            DateTime? dt = calendar_input(tb.Text);
            if (SelectedDate == null && dt == null
                || SelectedDate != null && dt != null && ((DateTime)SelectedDate).Date == ((DateTime)dt).Date && tb.Text.Length != 10)
                DatePicker_SelectedDateChanged(null, null);
            SelectedDate = dt;
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (tb.Name != "TextBox")
                tb = (TextBox)sender;
            if (SelectedDate == null)
            {
                if (tb.Text.Length < 1)
                    tb.Text = mask;
                return;
            }
            tb.Text = ((DateTime)SelectedDate).ToString("MM/dd/yy");
            tb.SelectionStart = 0;
            //tb.SelectionLength = 1;
        }
    }

    static public class WpfControlRoutines
    {
        public static void FocusOnText(this DatePicker datePicker)
        {
            if (datePicker == last_focused)
                return;
            last_focused = datePicker;
            Keyboard.Focus(datePicker);
            var eventArgs = new KeyEventArgs(Keyboard.PrimaryDevice,
                                             Keyboard.PrimaryDevice.ActiveSource,
                                             0,
                                             Key.Up);
            eventArgs.RoutedEvent = DatePicker.KeyDownEvent;
            datePicker.RaiseEvent(eventArgs);
        }
        static IInputElement last_focused = null;
    }
}