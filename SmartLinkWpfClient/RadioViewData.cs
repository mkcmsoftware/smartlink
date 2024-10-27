// ****************************************************************************
///*!	\file RadioViewData.cs
// *	\brief RadioViewData provides UI model for SmartLink radio object
// *
// *	\copyright	Copyright 2019 MKCM Software, portions FlexRadio Systems.  All Rights Reserved.
// *				MIT License
// *
// *	\date 2019-01-20
// *	\author Mark Hanson, W3II, based upon APIs from FlexRadio Systems
// */
// ****************************************************************************
using Flex.Smoothlake.FlexLib;
using Flex.UiWpfFramework.Mvvm;
using Flex.Util;
using System.Windows;
using System.Windows.Media;

namespace SmartLinkWfpClient
{
    public class RadioViewData : ObservableObject
    {
        private Radio _radio;

        public RadioViewData()
        {

        }
        public RadioViewData(Radio r)
        {
            Radio = r;
        }

        public Radio Radio
        {
            get
            {
                return this._radio;
            }
            set
            {
                if (_radio != value)
                {
                    if (_radio != null)
                    {
                        _radio.PropertyChanged -= _radio_PropertyChanged;
                    }
                    _radio = value;
                    if (value != null)
                    {
                        HolePunchNotRequired = _radio.RequiresHolePunch ? false : true;
                        _radio.PropertyChanged += _radio_PropertyChanged;
                        UpdateDisplayName();
                        SetImageForModel();
                        UpdateVersion(_radio.Version);
                        UpdateConnectInfo();
                    }
                    base.RaisePropertyChanged("Radio");
                }
            }
        }

        private void _radio_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "InUseHost" || e.PropertyName == "InUseIP")
            {
                RaisePropertyChanged(e.PropertyName);
            }
            if (e.PropertyName == "IP")
            {
                RaisePropertyChanged("IPStr");
            }
            else if (e.PropertyName == "ConnectedState")
            {
                RaisePropertyChanged("ConnectedState");
            }
            else if (e.PropertyName == "Version")
            {
                UpdateVersion(Radio.Version);
            }
            else if (e.PropertyName == "IsPortForwardOn")
            {
                this.IsPortForwardOn = _radio.IsPortForwardOn;
            }
            else if (e.PropertyName == "PublicTlsPort")
            {
                this.PublicTlsPort = this._radio.PublicTlsPort;
            }
            else if (e.PropertyName == "PublicUdpPort")
            {
                this.PublicUdpPort = this._radio.PublicUdpPort;
            }
            else if (e.PropertyName == "Serial")
            {
                this.Serial = this._radio.Serial;
            }
            else if (e.PropertyName == "RequiresHolePunch")
            {
                this.HolePunchNotRequired = this._radio.RequiresHolePunch ? false : true;
            }
            UpdateConnectInfo();
        }

        public string ConnectedState
        {
            get
            {
                if (Radio != null)
                    return Radio.Status;//.ConnectedState;
                return string.Empty;
            }
        }

        private bool _connectEnabled;
        public bool ConnectEnabled
        {
            get
            {
                return this._connectEnabled;
            }
            private set
            {
                if (this._connectEnabled != value)
                {
                    this._connectEnabled = value;
                    base.RaisePropertyChanged("ConnectEnabled");
                }
            }
        }

        private string _connectedStateStr;
        public string ConnectedStateStr
        {
            get
            {
                return this._connectedStateStr;
            }
            private set
            {
                if (this._connectedStateStr != value)
                {
                    this._connectedStateStr = value;
                    base.RaisePropertyChanged("ConnectedStateStr");
                }
            }
        }
        private bool _inUseByOtherClient;
        public bool InUseByOtherClient
        {
            get
            {
                return this._inUseByOtherClient;
            }
            private set
            {
                if (this._inUseByOtherClient != value)
                {
                    this._inUseByOtherClient = value;
                    base.RaisePropertyChanged("InUseByOtherClient");
                }
            }
        }
        private Visibility _connectVisibility;

        public Visibility ConnectVisibility
        {
            get
            {
                return this._connectVisibility;
            }
            set
            {
                if (this._connectVisibility != value)
                {
                    this._connectVisibility = value;
                    base.RaisePropertyChanged("ConnectVisibility");
                }
            }
        }

#if LOWBW
        private Visibility _lowBWConnectVisibility;
        public Visibility LowBWConnectVisibility
        {
            get
            {
                return this._lowBWConnectVisibility;
            }
            set
            {
                if (this._lowBWConnectVisibility != value)
                {
                    this._lowBWConnectVisibility = value;
                    base.RaisePropertyChanged("LowBWConnectVisibility");
                }
            }
        }
#endif
        public string InUseHost
        {
            get
            {
                if (Radio != null)
                    return Radio.InUseHost;
                return string.Empty;
            }
        }
        public string InUseIP
        {
            get
            {
                if (Radio != null)
                    return Radio.InUseIP;
                return string.Empty;
            }
        }
        public string IPStr
        {
            get
            {
                return this.Radio.IP.ToString();
            }
        }
        
        private ulong _version;
        public ulong Version
        {
            get
            {
                return this._version;
            }
        }
        private string _versionStr;
        public string VersionStr
        {
            get
            {
                return this._versionStr;
            }
        }
        private void UpdateVersion(ulong new_version)
        {
            if (this._version == new_version)
            {
                return;
            }
            this._version = new_version;
            this._versionStr = "v" + FlexVersion.ToString(this._version);
            base.RaisePropertyChanged("Version");
            base.RaisePropertyChanged("VersionStr");
        }

        private bool _isPortForwardOn;
        public bool IsPortForwardOn
        {
            get
            {
                return this._isPortForwardOn;
            }
            set
            {
                if (_isPortForwardOn != value)
                {
                    this._isPortForwardOn = value;
                    base.RaisePropertyChanged("IsPortForwardOn");
                }
            }
        }
        private int _publicTlsPort;
        public int PublicTlsPort
        {
            get { return _publicTlsPort; }
            set
            {
                if (value != _publicTlsPort)
                {
                    _publicTlsPort = value;
                    RaisePropertyChanged("PublicTlsPort");
                }
            }
        }

        private int _publicUdpPort;
        public int PublicUdpPort
        {
            get { return _publicUdpPort; }
            set
            {
                if (value != _publicUdpPort)
                {
                    _publicUdpPort = value;
                    RaisePropertyChanged("PublicUdpPort");
                }
            }
        }
        private void UpdateConnectInfo()
        {
            if (_radio == null)
                return;
            this.IsPortForwardOn = _radio.IsPortForwardOn;
            this.PublicTlsPort = this._radio.PublicTlsPort;
            this.PublicUdpPort = this._radio.PublicUdpPort;
            this.ConnectEnabled = (Radio.Status == "Available" || Radio.Status == "In_Use");
            string connectedState = Radio.Status;
            if (connectedState == "Update")
            {
                this.ConnectedStateStr = "Update";
                this.InUseByOtherClient = false;
                this.ConnectVisibility = Visibility.Collapsed;
#if LOWBW
                this.LowBWConnectVisibility = Visibility.Collapsed;
#endif
                return;
            }
            if (!(connectedState == "In_Use"))
            {
                if (!(connectedState == "Available"))
                {
                    this.InUseByOtherClient = false;
                    this.ConnectVisibility = Visibility.Collapsed;
                    this.ConnectedStateStr = "";
#if LOWBW
                    this.LowBWConnectVisibility = Visibility.Collapsed;
#endif
                    return;
                }
                if (Radio.RequiresAdditionalLicense)
                {
                    this.ConnectedStateStr = "License Required";
                    this.InUseByOtherClient = false;
                    this.ConnectVisibility = Visibility.Collapsed;
#if LOWBW
                    this.LowBWConnectVisibility = Visibility.Collapsed;
#endif
                    return;
                }
                this.InUseByOtherClient = false;
                this.ConnectVisibility = Visibility.Visible;
                this.ConnectedStateStr = "Available";
                if (Radio.IsWan)
                {
#if LOWBW
                    this.LowBWConnectVisibility = Visibility.Visible;
#endif
                    return;
                }
#if LOWBW
                this.LowBWConnectVisibility = Visibility.Collapsed;
#endif
                return;
            }
            else
            {
                if (InUseHost != "")
                {
                    this.ConnectedStateStr = "In Use (" + this.InUseHost + ")";
                }
                else if (InUseIP != "")
                {
                    this.ConnectedStateStr = "In Use (" + this.InUseIP + ")";
                }
                else
                {
                    this.ConnectedStateStr = "In Use";
                }
                this.ConnectVisibility = Visibility.Visible; //Visibility.Collapsed;
#if LOWBW
                this.LowBWConnectVisibility = Visibility.Collapsed;
#endif
                if (!Radio.Connected)
                {
                    this.InUseByOtherClient = true;
                    return;
                }
                this.InUseByOtherClient = false;
                return;
            }
        }

        private bool _shouldConnect;
        public bool ShouldConnect
        {
            get
            {
                return this._shouldConnect;
            }
            set
            {
                this._shouldConnect = value;
                base.RaisePropertyChanged("ShouldConnect");
            }
        }
        private string _displayName;
        public string DisplayName
        {
            get
            {
                return this._displayName;
            }
            set
            {
                if (this._displayName != value)
                {
                    this._displayName = value;
                    base.RaisePropertyChanged("DisplayName");
                }
            }
        }
        private void UpdateDisplayName()
        {
            if (Radio.Nickname == null || Radio.Nickname.Trim() == "")
            {
                this.DisplayName = Radio.Serial;
                return;
            }
            this.DisplayName = Radio.Nickname;
        }
        private string _imageSource;
        public string ImageSource
        {
            get
            {
                return this._imageSource;
            }
            set
            {
                if (this._imageSource != value)
                {
                    this._imageSource = value;
                    base.RaisePropertyChanged("ImageSource");
                }
            }
        }
        private string _serial;
        public string Serial
        {
            get
            {
                return this._serial;
            }
            set
            {
                if (this._serial != value)
                {
                    this._serial = value;
                    base.RaisePropertyChanged("Serial");
                }
            }
        }
        private bool _hpnr;
        public bool HolePunchNotRequired
        {
            get
            {
                return this._hpnr;
            }
            set
            {
                if (this._hpnr != value)
                {
                    this._hpnr = value;
                    base.RaisePropertyChanged("HolePunchNotRequired");
                }
            }
        }

        

        private void SetImageForModel()
        {
            string model = _radio.Model;
            if (model == "FLEX-6400M" || model == "FLEX-6600M")
            {
                this.ImageSource = "resources/6600m_small.png";
            }
            else if (model == "FLEX-6400" || model == "FLEX-6600")
            {
                this.ImageSource = "resources/6600_small.png";
            }
            else
            {
                this.ImageSource = "resources/flex6000.png";
            }
        }

    }
}
