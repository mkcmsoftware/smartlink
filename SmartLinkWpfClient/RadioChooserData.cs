// ****************************************************************************
///*!	\file RadioChooserData.cs
// *	\brief RadioChooserData is UI model for Radio Selection UI
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
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using Flex.UiWpfFramework.Mvvm;
using Flex.Util;

namespace SmartLinkWfpClient
{
    public class RadioChooserData : ObservableObject
	{
		public RadioChooserData(FlexRadioList radList, SmartLinkSettings slset, string title)
		{
			this._radiolist = radList;
			this._radiolist.PropertyChanged += this._radioList_PropertyChanged;
			this._slSettings = slset;
			this.InitStartupScreens();
			
            this.WindowTitle = title + " Radio Chooser";
            this.AppIconPath = "resources/radiochoosericon.ico";

            SmartLinkSettings wanset = this._slSettings;
			wanset.SaveAccountAction = (EventHandler)Delegate.Combine(wanset.SaveAccountAction, new EventHandler(delegate(object o, EventArgs e)
			{
				this.ShowScreen(this.RadioChooserScreen);
			}));
			this.FilterFileExists = File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\FlexRadio Systems\\filter.txt");
		}

		private void _radioList_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			FlexRadioList radioListViewModel = sender as FlexRadioList;
			if (radioListViewModel == null)
			{
				return;
			}
			if (radioListViewModel != this._radiolist)
			{
				return;
			}
			string propertyName = e.PropertyName;
			if (propertyName == "SelectedRadio" && this._radiolist.SelectedRadio == null && this.WanSettingsScreen.IsVisible)
			{
				this.ShowScreen(this.RadioChooserScreen);
			}
		}


		public string WindowTitle { get; private set; }

		public string AppIconPath { get; private set; }

        private RadioViewData _radio;
        public RadioViewData SelectedRadio
		{
			get
			{
				return this._radio;
			}
			set
			{
				this._radio = value;
				base.RaisePropertyChanged(nameof(SelectedRadio));
			}
		}

        private RadioViewData _connectedRadio;
        public RadioViewData ConnectedRadio
		{
			get
			{
				return this._connectedRadio;
			}
			set
			{
				this._connectedRadio = value;
				base.RaisePropertyChanged("ConnectedRadio");
			}
		}

        private FlexRadioList _radiolist;
        public FlexRadioList RadioList
		{
			get
			{
				return this._radiolist;
			}
			set
			{
				this._radiolist = value;
				base.RaisePropertyChanged("RadioList");
			}
		}

        private SmartLinkSettings _slSettings;
        public SmartLinkSettings SLConfigSettings
		{
			get
			{
				return this._slSettings;
			}
		}

        private ICommand _radioChooserCommand;
        public ICommand RadioChooserCommand
		{
			get
			{
				if (this._radioChooserCommand == null)
				{
					this._radioChooserCommand = new RelayCommand<object>(delegate(object param)
					{
						this.ShowScreen(this.RadioChooserScreen);
					});
				}
				return this._radioChooserCommand;
			}
			set
			{
				this._radioChooserCommand = value;
			}
		}


        private ICommand _openAppDataFolderCommand;
        public ICommand OpenAppDataFolderCommand
        {
            get
            {
                if (this._openAppDataFolderCommand == null)
                {
                    this._openAppDataFolderCommand = new RelayCommand<object>(delegate (object param)
                    {
                        Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\FlexRadio Systems\\");
                    });
                }
                return this._openAppDataFolderCommand;
            }
            set
            {
                this._openAppDataFolderCommand = value;
            }
        }
        private ICommand _accountSettingsCommand;
        public ICommand AccountSettingsCommand
		{
			get
			{
				if (this._accountSettingsCommand == null)
				{
					this._accountSettingsCommand = new RelayCommand<object>(delegate(object param)
					{
						this.ShowScreen(this.AccountSettingsScreen);
						if (!this._slSettings.Auth0LVM.IsUserLoggedIn)
						{
							this._slSettings.Auth0LVM.TryLogin();
						}
					});
				}
				return this._accountSettingsCommand;
			}
			set
			{
				this._accountSettingsCommand = value;
			}
		}

        private void DisconnectAllGuiClients()
		{
			if (this._radio != null && this._radio != null)
			{
				if (this._radio.Radio.IsWan && this._slSettings != null)
				{
					this._slSettings.DisconnectAllGuiClients(this._radio.Radio.Serial);
					return;
				}
				this._radio.Radio.DisconnectAllGuiClients();
			}
		}


        private bool _filterFileExists;
        public bool FilterFileExists
		{
			get
			{
				return this._filterFileExists;
			}
			set
			{
				this._filterFileExists = value;
				base.RaisePropertyChanged("FilterFileExists");
			}
		}

		private void ShowScreen(StartupScreen screen)
		{
			if (screen == null)
			{
				return;
			}
			foreach (StartupScreen startupScreen in this._startupScreens)
			{
				startupScreen.IsVisible = (startupScreen == screen);
			}
		}

		public StartupScreen RadioChooserScreen { get; set; }

		public StartupScreen WanSettingsScreen { get; set; }

		public StartupScreen AccountSettingsScreen { get; set; }

        private List<StartupScreen> _startupScreens = new List<StartupScreen>();

        public void InitStartupScreens()
		{
			this.RadioChooserScreen = new StartupScreen();
			this.WanSettingsScreen = new StartupScreen();
			this.AccountSettingsScreen = new StartupScreen();
			this._startupScreens.Add(this.RadioChooserScreen);
			this._startupScreens.Add(this.WanSettingsScreen);
			this._startupScreens.Add(this.AccountSettingsScreen);
			this.RadioChooserScreen.IsVisible = true;
		}

		

		public class StartupScreen : ObservableObject
		{
			public bool IsVisible
			{
				get
				{
					return this._isVisible;
				}
				set
				{
					if (this._isVisible != value)
					{
						this._isVisible = value;
						base.RaisePropertyChanged("IsVisible");
					}
				}
			}

			private bool _isVisible;
		}
	}
}
