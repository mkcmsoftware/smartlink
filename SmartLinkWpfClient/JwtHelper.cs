// ****************************************************************************
///*!	\file JwtHelper.cs
// *	\brief JWT helper logic 
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
using Jose;

namespace SmartLinkWfpClient
{
	public class JwtHelper
	{
		public JwtHelper(string clientSecret = null)
		{
			this._clientSecret = clientSecret;
		}

		public bool GetIsJwtValid(string jwt)
		{
			bool result = false;
			try
			{
				byte[] key = this.Base64UrlDecode(this._clientSecret);
				JWT.Decode<IDictionary<string, object>>(jwt, key);
				result = !this.GetIsJwtExpired(jwt);
			}
			catch (Exception)
			{
				Console.WriteLine("JWT has an invalid signature");
				result = false;
			}
			return result;
		}

		private byte[] Base64UrlDecode(string arg)
		{
			string text = arg.Replace('-', '+');
			text = text.Replace('_', '/');
			switch (text.Length % 4)
			{
			case 0:
				goto IL_60;
			case 2:
				text += "==";
				goto IL_60;
			case 3:
				text += "=";
				goto IL_60;
			}
			throw new Exception("Illegal base64url string!");
			IL_60:
			return Convert.FromBase64String(text);
		}

		private long ToUnixTime(DateTime dateTime)
		{
			return (long)((int)dateTime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
		}

		private DateTime UnixTimeStampToDateTime(double unixTimeStamp)
		{
			DateTime result = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			result = result.AddSeconds(unixTimeStamp).ToLocalTime();
			return result;
		}

		public string GetGivenNameFromJwt(string jwt)
		{
			return this.GetPayloadItemFromJwt(jwt, "given_name");
		}

		public string GetFamilyNameFromJwt(string jwt)
		{
			return this.GetPayloadItemFromJwt(jwt, "family_name");
		}

		public string GetPictureFromJwt(string jwt)
		{
			return this.GetPayloadItemFromJwt(jwt, "picture");
		}

		public string GetEmailFromJwt(string jwt)
		{
			return this.GetPayloadItemFromJwt(jwt, "email");
		}

		public string GetAuth0UserIdFromJwt(string jwt)
		{
			return this.GetPayloadItemFromJwt(jwt, "sub");
		}

		public double GetExpirationSecondsLeft(string jwt)
		{
			double num = (double)this.ToUnixTime(DateTime.Now);
			if (this.GetExpirationUnixTimestamp(jwt) <= 0.0)
			{
				return 0.0;
			}
			double num2 = num - this.GetExpirationUnixTimestamp(jwt);
			if (num2 >= 0.0)
			{
				return num2;
			}
			return 0.0;
		}

		public bool GetIsJwtExpired(string jwt)
		{
			double expirationUnixTimestamp = this.GetExpirationUnixTimestamp(jwt);
			DateTime t = this.UnixTimeStampToDateTime(expirationUnixTimestamp);
			bool result;
			if (DateTime.Compare(DateTime.Now, t) < 0)
			{
				Console.WriteLine("JWT has not expired.");
				result = false;
			}
			else
			{
				Console.WriteLine("JWT has expired.");
				result = true;
			}
			return result;
		}

		public double GetExpirationUnixTimestamp(string jwt)
		{
			double result = 0.0;
			if (!double.TryParse(this.GetPayloadItemFromJwt(jwt, "exp"), out result))
			{
				return 0.0;
			}
			return result;
		}

		private string GetPayloadItemFromJwt(string jwt, string itemName)
		{
			try
			{
				return JWT.Payload<IDictionary<string, object>>(jwt)[itemName].ToString();
			}
			catch (Exception)
			{
			}
			return "";
		}

		private string _clientSecret;
	}
}
