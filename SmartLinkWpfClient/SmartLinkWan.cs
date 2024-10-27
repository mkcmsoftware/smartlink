// ****************************************************************************
///*!	\file SmartLinkWan.cs
// *	\brief SmartLinkWan is root object that provides SmartLink and Local Radio List
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
using Flex.Smoothlake.FlexLib;
using Flex.UiWpfFramework.Mvvm;
using Flex.Util;

namespace SmartLinkWfpClient
{
    public class SmartLinkWan : ObservableObject
    {
        public SmartLinkWan()
        {
            RadioList = new FlexRadioList();
            wanServer = new WanServer();
        }

        private WanServer _wanServer;
        public WanServer wanServer
        {
            get { return _wanServer; }
            protected set { _wanServer = value; }
        }

        private RadioViewData _RadioVD;
        public RadioViewData RadioVD
        {
            get { return _RadioVD; }
            set
            {
                if (_RadioVD != value)
                {
                    _RadioVD = value;
                }
                base.RaisePropertyChanged(nameof(RadioVD));
            }
        }



        private FlexRadioList _radioList;
        public FlexRadioList RadioList
        {
            get
            {
                return this._radioList;
            }
            set
            {
                this._radioList = value;
                base.RaisePropertyChanged("RadioList");
            }
        }
        public void InitWan()
        {
            this._wanServer.WanRadioConnectReady += this._wanServer_WanRadioConnectReady;
            this._slSettings = new SmartLinkSettings(_wanServer, _radioList);
        }
        public void Shutdown()
        {
            if (RadioChooser != null)
            {
                var rc = RadioChooser;
                RadioChooser = null;
                rc.Close();
            }

            _slSettings?.Shutdown();

            if (_wanServer != null)
            {
                if (_wanServer.IsConnected)
                    _wanServer.Disconnect();
                wanServer = null;
            }
        }

        private void _wanServer_WanRadioConnectReady(string wan_connectionhandle, string serial)
        {
            if (RadioVD == null)
                return;
            if (RadioVD.Radio.Serial != serial)
                return;

            RadioVD.Radio.WANConnectionHandle = wan_connectionhandle;
            RadioVD.ShouldConnect = true;
            RaisePropertyChanged(nameof(RadioVD));
        }

        private SmartLinkSettings _slSettings;
        public SmartLinkSettings SLConfigSettings
        {
            get
            {
                return this._slSettings;
            }
        }

        private RadioChooserData _radioChooserData;
        public SmartLinkRadioChooser RadioChooser { get; set; }

        public void InitRadioChooser(string title)
        {
            if (this._radioChooserData == null)
            {
                this._radioChooserData = new RadioChooserData(RadioList, SLConfigSettings, title);
                this._radioChooserData.PropertyChanged += this._radioChooserViewModel_PropertyChanged;
            }
            if (this.RadioChooser == null)
            {
                RadioChooser = new SmartLinkRadioChooser(this._radioChooserData, this._wanServer);
            }
        }
        private void _radioChooserViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RadioChooserData rdata = sender as RadioChooserData;
            if (rdata != null && e.PropertyName == "ConnectedRadio")
            {
                this.RadioVD = rdata.ConnectedRadio;
            }
        }

    }
}
