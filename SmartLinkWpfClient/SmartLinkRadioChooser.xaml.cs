// ****************************************************************************
///*!	\file SmartLinkRadioChooser.xaml.cs
// *	\brief 
// *
// *	\copyright	Copyright 2019 MKCM Software, portions FlexRadio Systems.  All Rights Reserved.
// *				MIT License
// *
// *	\date 2019-01-20
// *	\author Mark Hanson, W3II, based upon APIs from FlexRadio Systems
// */
// ****************************************************************************
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using Flex.Smoothlake.FlexLib;
using Flex.UiWpfFramework.Mvvm;
using Flex.UiWpfFramework.Utils;

namespace SmartLinkWfpClient
{
    public partial class SmartLinkRadioChooser : Window, IComponentConnector
    {
        public SmartLinkRadioChooser(RadioChooserData radioChooserData, WanServer wanserver)
        {
            InitializeComponent();
            _wanServer = wanserver;
            _wanServer.WanRadioConnectReady += this._wanServer_WanRadioConnectReady;
            Closing += this.RadioChooser_Closing;
            Deactivated += this.RadioView_Deactivated;
            _radioChooserData = radioChooserData;
            ListBox.Items.SortDescriptions.Add(new SortDescription("Serial", ListSortDirection.Ascending));
            ListBox.MouseDoubleClick += this.ListBox_MouseDoubleClick;
            ListBox.SelectionChanged += this.ListBox_SelectionChanged;
            SourceInitialized += this.RadioChooser_SourceInitialized;

            DataContext = _radioChooserData;
        }

        public bool CommandLineHasSerialNo()
        {
            string[] commandLineArgs = Environment.GetCommandLineArgs();
            if (commandLineArgs.Length > 1 && commandLineArgs[1].Contains("--serial="))
                return true;
            return false;
        }
        public void CheckCommandLine()
        {
            string[] commandLineArgs = Environment.GetCommandLineArgs();
            if (commandLineArgs.Length > 1 && commandLineArgs[1].Contains("--serial="))
            {
                string parameter = commandLineArgs[1].Replace("--serial=", string.Empty);
                new Thread(new ParameterizedThreadStart(this.AutostartRadioThread))
                {
                    Name = "Autostart Radio Thread",
                    IsBackground = true,
                    Priority = ThreadPriority.Normal
                }.Start(parameter);
            }
        }
        private void RadioChooser_SourceInitialized(object sender, EventArgs e)
        {
            this.SetPlacement(SmartLinkWpfClient.Properties.Settings.Default.ChooserLoc);
        }

        private void AutostartRadioThread(object obj)
        {
            int num = 0;
            while (num++ < 10)
            {
                string b = (string)obj;
                RadioViewData item = null;
                foreach (RadioViewData rvd in this._radioChooserData.RadioList.RadiosFound.SafeGetList())
                {
                    if (rvd.Radio.Serial == b)
                    {
                        item = rvd;
                    }
                }
                int index = this.ListBox.Items.IndexOf(item);
                if (index >= 0)
                {
                    base.Dispatcher.BeginInvoke(new Action(delegate
                    {
                        this.ListBox.SelectedIndex = index;
                        this.btnRadioConnect_Click(this, null);
                    }), new object[0]);
                    return;
                }
                Thread.Sleep(1000);
            }
        }

        private void RadioChooser_Closing(object sender, CancelEventArgs e)
        {
            SmartLinkWpfClient.Properties.Settings.Default.ChooserLoc = this.GetPlacement();
            SmartLinkWpfClient.Properties.Settings.Default.Save();
            this.ListBox.Focus();
            base.Visibility = Visibility.Collapsed;
            e.Cancel = true;
        }

        private void RadioView_Deactivated(object sender, EventArgs e)
        {
            this.ListBox.Focus();
        }

        public delegate void RadioChosenEventHandler(RadioViewData rvm);
        public event RadioChosenEventHandler RadioChosen;
        private RadioChooserData _radioChooserData;
        private void OnRadioChosen(RadioViewData rvm)
        {
            if (this.RadioChosen != null)
            {
                this.RadioChosen(rvm);
            }
        }

        private void btnRadioConnect_Click(object sender, RoutedEventArgs e)
        {
            if (this._radioChooserData.SelectedRadio == null)
            {
                return;
            }
            if (this.ListBox.SelectedIndex < 0)
            {
                return;
            }
            Button button = sender as Button;
            bool flag = false;
            if (button != null && button.Name.Contains("LowBw"))
            {
                flag = true;
            }
            RadioViewData s = this._radioChooserData.SelectedRadio;
            if (s == null)
            {
                return;
            }
            if (s.ConnectedState == "Update")
            {
                return;
            }
            if (s.Radio.RequiresAdditionalLicense)
            {
                return;
            }
            if (s.Radio.Status != "Available" && s.Radio.Status != "In_Use")
            {
                return;
            }
            this.OnRadioChosen(s);
            
            if (flag)
            {
                s.Radio.LowBandwidthConnect = flag;
            }
            if (s.Radio.IsWan)
            {
                Random random = new Random();
                if (s.Radio.RequiresHolePunch)
                {
                    s.Radio.NegotiatedHolePunchPort = random.Next(25000, 65000);
                }
                this._wanServer.SendConnectMessageToRadio(s.Radio.Serial, s.Radio.NegotiatedHolePunchPort);
                this._radioChooserData.ConnectedRadio = s;
                return;
            }
            s.ShouldConnect = true;
            this._radioChooserData.ConnectedRadio = s;
            base.Hide();
        }

        private RadioViewData GetSelectedRVMFromListBox()
        {
            return (RadioViewData)this.ListBox.SelectedItem;
        }

        private void _wanServer_WanRadioConnectReady(string wan_connectionhandle, string serial)
        {
            base.Dispatcher.BeginInvoke(new Action(delegate
            {
                base.Hide();
            }), new object[0]);
        }

        private void Radio_MessageReceived(MessageSeverity severity, string msg)
        {
            //var mbt = new MessageBoxTimed();
            MessageBox.Show(msg);
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.btnRadioConnect_Click(this, null);
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ListBox.SelectedIndex < 0)
            {
                return;
            }
            RadioViewData s = this.GetSelectedRVMFromListBox();
            this._radioChooserData.SelectedRadio = s;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if (propertyChanged != null)
            {
                propertyChanged(this, e);
            }
        }

        protected void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpresssion)
        {
            string propertyName = PropertySupport.ExtractPropertyName<T>(propertyExpresssion);
            this.RaisePropertyChanged(propertyName);
        }

        protected void RaisePropertyChanged(string propertyName)
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        private WanServer _wanServer;

        

        
    }
}
