using System;
using System.ComponentModel;
using System.Windows;
using Flex.Smoothlake.FlexLib;
using SmartLinkWfpClient;


namespace MKCMSoftware.SmartLinkTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Radio fradio { get; set; }
        public SmartLinkWan smartLinkWan { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            API.ProgramName = "SmartLinkTest";
            API.IsGUI = false;

            API.Init();

            smartLinkWan = new SmartLinkWan();
            smartLinkWan.PropertyChanged += SmartLinkWan_PropertyChanged;
            smartLinkWan.InitWan();

        }

        private void BntConnect_Click(object sender, RoutedEventArgs e)
        {
            if (fradio != null)
                return;

            derefRadio();

            if (smartLinkWan != null)
            {
                if (smartLinkWan.RadioChooser.IsVisible)
                {
                    smartLinkWan.RadioChooser.Focus();
                }
                else
                {
                    smartLinkWan.RadioChooser.Show();
                }
            }
        }

        private void BtnDisconnect_Click(object sender, RoutedEventArgs e)
        {
            if (fradio == null)
                return;

            derefRadio();
        }

        private void SmartLinkWan_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            string propertyName = e.PropertyName;
            if (propertyName == "RadioVD")
            {
                InvokeHelper.BeginInvokeIfNeeded(base.Dispatcher, delegate
                {
                    if (smartLinkWan.RadioVD.Radio != null && smartLinkWan.RadioVD.ShouldConnect)
                    {
                        if (fradio == smartLinkWan.RadioVD.Radio)
                            return;
                        Radio rr = smartLinkWan.RadioVD.Radio;
                        this.Activate();
                        if (rr != null)
                            connectToRadio(rr);
                    }
                });
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            smartLinkWan.InitRadioChooser(this.Title);
            smartLinkWan.RadioChooser.Closing += RadioChooser_Closing;
        }

        private void RadioChooser_Closing(object sender, CancelEventArgs e)
        {
            if (smartLinkWan.RadioChooser != null)
            {
                smartLinkWan.RadioChooser.Closing -= this.RadioChooser_Closing;
                smartLinkWan.RadioChooser = null;
            }
            if (!isclosingnow)
                this.Close();
        }

        private void connectToRadio(Radio rr)
        {
            if (rr != null)
            {
                this.Title = $"Connected {rr.Model} - {rr.Nickname}" + (rr.IsWan ? " SMARTLINK" : string.Empty);

                rr.PropertyChanged += fradio_PropertyChanged;
                rr.SliceAdded += fradio_SliceAdded;
                rr.SliceRemoved += fradio_SliceRemoved;

                if (rr.Connected == false)
                {
                    rr.Connect();
                }

                fradio = rr;

                SliceCount.Text = fradio.SliceList.Count.ToString();
            }
        }

        private void fradio_SliceRemoved(Slice slc)
        {
            InvokeHelper.BeginInvokeIfNeeded(base.Dispatcher, delegate
            {

                SliceCount.Text = fradio.SliceList.Count.ToString();
            });
        }

        private void fradio_SliceAdded(Slice slc)
        {
            InvokeHelper.BeginInvokeIfNeeded(base.Dispatcher, delegate
            {

                SliceCount.Text = fradio.SliceList.Count.ToString();
            });
        }

        private void fradio_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            InvokeHelper.BeginInvokeIfNeeded(base.Dispatcher, delegate
            {
                try
                {
                    Radio r = sender as Radio;
                    if (r != null)
                    {
                        // this will happen when radio is shutdown and Connected property change event is sent
                        if (!r.Connected)
                        {
                            derefRadio();
                            return;
                        }


                        System.Diagnostics.Debug.WriteLine("Radio Property " + e.PropertyName + " changed");


                        if (StatusText.Text != r.Status)
                            StatusText.Text = r.Status;
                    }
                }
                catch { }

            });
        }

        void derefRadio()
        {
            try
            {
                if (fradio != null)
                {
                    fradio.SliceRemoved -= fradio_SliceRemoved;
                    fradio.SliceAdded -= fradio_SliceAdded;
                    fradio.PropertyChanged -= fradio_PropertyChanged;

                    fradio.Disconnect();
                    fradio = null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            this.Title = "Disconnected";
            StatusText.Text = "";

        }

        private bool isclosingnow = false;
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            isclosingnow = true;

            derefRadio();

            smartLinkWan?.Shutdown();

            API.CloseSession();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
    }
}