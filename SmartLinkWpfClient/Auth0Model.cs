// ****************************************************************************
///*!	\file Auth0Model.cs
// *	\brief Open Auth0 
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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

using Auth0.Windows;
using Flex.UiWpfFramework.Mvvm;
using Flex.Util;
using Newtonsoft.Json.Linq;

namespace SmartLinkWfpClient
{
	public class Auth0Model : ObservableObject
	{
		public Auth0Model(Auth0Client auth0Client)
		{
			this._auth0Cli = auth0Client;
			this._refreshToken = this.GetSavedRefreshToken();
			this.UpdateLoggedInState();
		}

		public string GetFreshJwt()
		{
			if (this._jwtAuth.GetExpirationSecondsLeft(this._jwt) < 15.0)
			{
				return this._jwt;
			}
			this._jwt = this.GetNewJwtUsingRefreshToken(this._refreshToken);
			if (this._jwt == "fail")
			{
				this._jwt = string.Empty;
			}
			return this._jwt;
		}

		public void TryLogin()
		{
			this.UpdateLoggedInState();
			try
			{
				if (string.IsNullOrEmpty(this._refreshToken))
				{
					Console.WriteLine("Requesting login with new refresh token");
					this.Login(true);
				}
				else
				{
					Console.WriteLine("Attempting to login using Refresh Token");
					this._jwt = this.GetNewJwtUsingRefreshToken(this._refreshToken);
					if (string.IsNullOrEmpty(this._jwt))
					{
						this.DeleteTokens();
						this.Login(true);
					}
				}
			}
			catch (Exception)
			{
			}
			this.UpdateLoggedInState();
			this.OnLoginWindowClosed();
		}

		private void OnLoginWindowClosed()
		{
			if (this.LoginWindowClosed != null)
			{
				this.LoginWindowClosed(this, null);
			}
		}

		private string GetSavedRefreshToken()
		{
			string result;
			try
			{
				result = StringCipher.Decrypt(SmartLinkWpfClient.Properties.Settings.Default.RefreshToken, this._encryptPassphrase);
			}
			catch
			{
				result = "";
			}
			return result;
		}

		private void SaveRefreshToken()
		{
            SmartLinkWpfClient.Properties.Settings.Default.RefreshToken = StringCipher.Encrypt(this._refreshToken, this._encryptPassphrase);
            SmartLinkWpfClient.Properties.Settings.Default.Save();
		}

		public void LoginWithRefreshTokenNoUI()
		{
			try
			{
				this.UpdateLoggedInState();
				if (!string.IsNullOrEmpty(this._refreshToken))
				{
					//Console.WriteLine("Attempting to login using Refresh Token");
					string newJwtWithRefreshToken = this.GetNewJwtUsingRefreshToken(this._refreshToken);
					if (!(newJwtWithRefreshToken == "fail"))
					{
						this._jwt = newJwtWithRefreshToken;
						if (string.IsNullOrEmpty(this._jwt))
						{
							this.DeleteTokens();
						}
						this.UpdateLoggedInState();
					}
				}
			}
			catch
			{
			}
		}

		public bool IsUserLoggedIn
		{
			get
			{
				return this._isUserLoggedIn;
			}
			set
			{
				this._isUserLoggedIn = value;
				base.RaisePropertyChanged("IsUserLoggedIn");
			}
		}

		public string PictureUrl
		{
			get
			{
				return this._pictureUrl;
			}
			set
			{
				this._pictureUrl = value;
				base.RaisePropertyChanged("PictureUrl");
			}
		}

		public string GivenName
		{
			get
			{
				return this._givenName;
			}
			set
			{
				this._givenName = value;
				base.RaisePropertyChanged("GivenName");
			}
		}

		public string FamilyName
		{
			get
			{
				return this._familyName;
			}
			set
			{
				this._familyName = value;
				base.RaisePropertyChanged("FamilyName");
			}
		}

		private void Login(bool requestRefreshToken = false)
		{
			Window window = new Window();
			string loginResult = "";
            string scope = requestRefreshToken ? "openid offline_access profile" : "openid profile";
			string device = "myDevice";
			this._auth0Cli.LoginAsync(new WindowWrapper(new WindowInteropHelper(window).Handle), "", scope, device).ContinueWith(delegate(Task<Auth0User> t)
			{
				if (!t.IsFaulted)
				{
					if (requestRefreshToken)
					{
						this._refreshToken = this._auth0Cli.CurrentUser.RefreshToken;
						this.SaveRefreshToken();
					}
					this._jwt = this._auth0Cli.CurrentUser.IdToken;
					return;
				}
				if (t.Exception.InnerException != null)
				{
					loginResult = t.Exception.InnerException.ToString();
					return;
				}
				loginResult = t.Exception.ToString();
			}, TaskScheduler.FromCurrentSynchronizationContext()).Wait();
			this.UpdateLoggedInState();

            if (!string.IsNullOrEmpty(loginResult))
            {
                //var mbt = new MessageBoxTimed();
                MessageBox.Show(loginResult, "SmartLink Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

		public void Logout()
		{
			this.DeleteTokens();
			this._auth0Cli.Logout();
			this.UpdateLoggedInState();
		}

		private void UpdateLoggedInState()
		{
			if (!string.IsNullOrEmpty(this._refreshToken) && !string.IsNullOrEmpty(this.GetFreshJwt()))
			{
				this.IsUserLoggedIn = true;
				this.PictureUrl = this._jwtAuth.GetPictureFromJwt(this._jwt);
				this.GivenName = this._jwtAuth.GetGivenNameFromJwt(this._jwt);
				this.FamilyName = this._jwtAuth.GetFamilyNameFromJwt(this._jwt);
				return;
			}
			this.IsUserLoggedIn = false;
			this.PictureUrl = null;
			this.GivenName = "";
			this.FamilyName = "";
		}

		private void DeleteTokens()
		{
			this._refreshToken = string.Empty;
			this.SaveRefreshToken();
			Console.WriteLine("Refresh Token deleted");
			this.DeleteJWT();
		}

		private void DeleteJWT()
		{
			this._jwt = string.Empty;
			Console.WriteLine("JWT deleted");
			this.UpdateLoggedInState();
		}

		private string GetNewJwtUsingRefreshToken(string refreshToken)
		{
            string targetClientId = "4Y9fEIIsVYyQo5u6jr7yBWc4lV5ugC2m";
			string newJwt = string.Empty;
			Dictionary<string, string> options = new Dictionary<string, string>
			{
				{
					"scope",
                    "openid profile"
                },
				{
					"refresh_token",
					refreshToken
				}
			};
            string gdtError = "";
			this._auth0Cli.GetDelegationToken(targetClientId, options).ContinueWith(delegate(Task<JObject> t)
			{
				if (t.Result == null)
				{
					newJwt = "fail";
					return;
				}
                if (t.Exception != null)
                {
                    if (t.Exception.InnerException != null)
                        gdtError = t.Exception.InnerException.Message;
                    else
                        gdtError = t.Exception.Message;
                }
				newJwt = (string)t.Result["id_token"];
			}).Wait();
			if (newJwt == null)
			{
				newJwt = string.Empty;
			}
            if (!string.IsNullOrEmpty(gdtError))
            {
                //var mbt = new MessageBoxTimed();
                MessageBox.Show(gdtError, "SmartLink GetDelegationToken", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return newJwt;
		}

		private Auth0Client _auth0Cli;

		private JwtHelper _jwtAuth = new JwtHelper(null);

		private string _refreshToken = string.Empty;

		private string _jwt = string.Empty;

		private string _encryptPassphrase = Environment.MachineName + "ZpdO533QI9";

		public EventHandler LoginWindowClosed;

		private bool _isUserLoggedIn;

		private string _pictureUrl;

		private string _givenName;

		private string _familyName;
	}
}
