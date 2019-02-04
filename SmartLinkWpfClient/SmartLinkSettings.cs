// ****************************************************************************
///*!	\file SmartLinkSettings.cs
// *	\brief SmartLink Settings provide WanServer client logic
// *
// *	\copyright	Copyright 2019 MKCM Software, portions FlexRadio Systems.  All Rights Reserved.
// *				MIT License
// *
// *	\date 2019-01-20
// *	\author Mark Hanson, W3II, based upon APIs from FlexRadio Systems
// */
// ****************************************************************************
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Auth0.Windows;
using Flex.Smoothlake.FlexLib;
using Flex.UiWpfFramework.Mvvm;

namespace SmartLinkWfpClient
{
    public partial class SmartLinkSettings : ObservableObject
	{
        public ManualResetEvent closeNow = new ManualResetEvent(false);

        private WanServer _wanServer;
        private FlexRadioList _radioList;
        private Auth0Model _auth0LVM;
        private AutoResetEvent _wanConnectARE = new AutoResetEvent(false);

        private IDialogService _dialogService = new DialogService();

        public SmartLinkSettings(WanServer wanServer, FlexRadioList radioListVm)
		{
            string domain = "frtest.auth0.com";
            string clientId = "4Y9fEIIsVYyQo5u6jr7yBWc4lV5ugC2m";
            Auth0LVM = new Auth0Model(new Auth0Client(domain, clientId, 900, 630, false));
            this._auth0LVM.PropertyChanged += this._auth0LVM_PropertyChanged;

			this._wanServer = wanServer;
			this._wanServer.PropertyChanged += this._wanServer_PropertyChanged;
			this._wanServer.WanApplicationRegistrationInvalid += this._wanServer_WanApplicationRegistrationInvalid;
			
			this.RadioList = radioListVm;
            Task.Factory.StartNew(delegate
			{
				this._auth0LVM.LoginWithRefreshTokenNoUI();
			}, TaskCreationOptions.LongRunning);

            Task.Factory.StartNew(delegate
			{
				this.WanServerConnectThread();
			}, TaskCreationOptions.LongRunning);
		}
        public void Shutdown()
        {
            closeNow.Set();
            _wanConnectARE.Set();
        }
		private void WanServerConnectThread()
		{
			int num = 0;
			List<int> list = new List<int>
			{
				5000,
				10000,
				30000
			};
			for (; closeNow.WaitOne(1) == false;)
			{
				num = 0;
				while (!this._wanServer.IsConnected && closeNow.WaitOne(0) == false)
				{
					if (this._auth0LVM.IsUserLoggedIn)
					{
						try
						{
							this._wanServer.Connect();
						}
						catch
						{
						}
					}
					int millisecondsTimeout = list[Math.Min(num, list.Count - 1)];
					this._wanConnectARE.WaitOne(millisecondsTimeout);
					num++;
				}
				this._wanConnectARE.WaitOne();
			}
		}

		private void _wanServer_WanApplicationRegistrationInvalid()
		{
			this._auth0LVM.Logout();
			this._dialogService.ShowOkBox("Your session has expired. Please log in.");
		}
        private void _wanServer_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			WanServer wanServer = sender as WanServer;
			if (wanServer == null)
			{
				return;
			}
			string propertyName = e.PropertyName;
            if (propertyName == "UserSettings")
            {
                WanUserSettings wanUserSettings = (WanUserSettings)wanServer.UserSettings.Clone();
                bool flag = false;
                if (string.IsNullOrEmpty(wanUserSettings.FirstName))
                {
                    wanUserSettings.FirstName = this._auth0LVM.GivenName;
                    if (!string.IsNullOrEmpty(wanUserSettings.FirstName))
                    {
                        flag = true;
                    }
                }
                if (string.IsNullOrEmpty(wanUserSettings.LastName))
                {
                    wanUserSettings.LastName = this._auth0LVM.FamilyName;
                    if (!string.IsNullOrEmpty(wanUserSettings.LastName))
                    {
                        flag = true;
                    }
                }
                this.UserSettingsEditor = (WanUserSettings)wanUserSettings.Clone();
                this.UserSettingsServer = (WanUserSettings)wanUserSettings.Clone();
                if (flag)
                {
                    this._wanServer.SendSetUserInfoToServer(this._auth0LVM.GetFreshJwt(), this._userSettingsEditor);
                }
                this.SaveUserTitle = "Save";
            }
            else if (propertyName == "SslClientPublicIp")
            {
                this.ApplicationPublicIp = wanServer.SslClientPublicIp;
            }
            else if (propertyName == "IsConnected")
            {
                this.IsWanServerConnected = wanServer.IsConnected;
                if (!wanServer.IsConnected)
                {
                    RadioList.RemoveAllWanRadios();
                    _wanConnectARE.Set();
                    return;
                }
                this._wanServer.SendRegisterApplicationMessageToServer(API.ProgramName, "Windows", this._auth0LVM.GetFreshJwt());
            }
		}

		private void _auth0LVM_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			Auth0Model loginm = sender as Auth0Model;
			if (loginm == null)
			{
				return;
			}
			string propertyName = e.PropertyName;
			if (propertyName == "IsUserLoggedIn")
			{
				if (!loginm.IsUserLoggedIn)
				{
					if (this._wanServer != null)
					{
						this._wanServer.Disconnect();
					}
					RadioList.RemoveAllWanRadios();
					_userSettingsEditor.Clear();
					_userSettingsServer.Clear();
					return;
				}
				this._wanConnectARE.Set();
			}
		}
        public Auth0Model Auth0LVM
		{
			get
			{
				return this._auth0LVM;
			}
			set
			{
				this._auth0LVM = value;
				base.RaisePropertyChanged(nameof(Auth0LVM));
			}
		}

        private WanUserSettings _userSettingsEditor = new WanUserSettings();
        public WanUserSettings UserSettingsEditor
		{
			get
			{
				return this._userSettingsEditor;
			}
			private set
			{
				this._userSettingsEditor = value;
				base.RaisePropertyChanged(nameof(UserSettingsEditor));
			}
		}

        private WanUserSettings _userSettingsServer = new WanUserSettings();
        public WanUserSettings UserSettingsServer
		{
			get
			{
				return this._userSettingsServer;
			}
			private set
			{
				this._userSettingsServer = value;
				base.RaisePropertyChanged(nameof(UserSettingsServer));
			}
		}

        private string _saveUserTitle = "Save";
        public string SaveUserTitle
		{
			get
			{
				return this._saveUserTitle;
			}
			set
			{
				this._saveUserTitle = value;
				base.RaisePropertyChanged(nameof(SaveUserTitle));
			}
		}

		public FlexRadioList RadioList
		{
			get
			{
				return this._radioList;
			}
			set
			{
				this._radioList = value;
			}
		}

        private bool _isWanServerConnected;
        public bool IsWanServerConnected
		{
			get
			{
				return this._isWanServerConnected;
			}
			set
			{
				this._isWanServerConnected = value;
				base.RaisePropertyChanged(nameof(IsWanServerConnected));
			}
		}

        private string _applicationPublicIp = "";
        public string ApplicationPublicIp
		{
			get
			{
				return this._applicationPublicIp;
			}
			set
			{
				this._applicationPublicIp = value;
				base.RaisePropertyChanged(nameof(ApplicationPublicIp));
			}
		}

        public EventHandler LoginButtonPressed;
        private void OnLoginButtonPressed()
		{
			if (this.LoginButtonPressed != null)
			{
				this.LoginButtonPressed(this, null);
			}
		}

        public EventHandler SaveAccountAction;
        private void OnSaveAccountAction()
		{
			if (this.SaveAccountAction != null)
			{
				this.SaveAccountAction(this, null);
			}
		}

        private ICommand _loginCommand;
        public ICommand LoginCommand
		{
			get
			{
				if (this._loginCommand == null)
				{
					this._loginCommand = new RelayCommand<object>(delegate(object param)
					{
						this.OnLoginButtonPressed();
						this._auth0LVM.TryLogin();
					});
				}
				return this._loginCommand;
			}
			set
			{
				this._loginCommand = value;
			}
		}

		public void DisconnectAllGuiClients(string radioSerial)
		{
			this._wanServer.SendDisconnectUsersMessageToServer(radioSerial);
		}

        private ICommand _logoutCommand;
        public ICommand LogoutCommand
		{
			get
			{
				if (this._logoutCommand == null)
				{
					this._logoutCommand = new RelayCommand<object>(delegate(object param)
					{
						this._auth0LVM.Logout();
					});
				}
				return this._logoutCommand;
			}
			set
			{
				this._logoutCommand = value;
			}
		}

        private ICommand _setUserInfoCommand;
        public ICommand SetUserInfoCommand
		{
			get
			{
				if (this._setUserInfoCommand == null)
				{
					this._setUserInfoCommand = new RelayCommand<object>(delegate(object param)
					{
						this.SaveUserTitle = "Saving...";
						this._wanServer.SendSetUserInfoToServer(this._auth0LVM.GetFreshJwt(), this._userSettingsEditor);
						this.OnSaveAccountAction();
					});
				}
				return this._setUserInfoCommand;
			}
			set
			{
				this._setUserInfoCommand = value;
			}
		}
        

		

		

		

		
        
        

	}
}
