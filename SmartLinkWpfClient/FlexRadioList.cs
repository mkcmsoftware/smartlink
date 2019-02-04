// ****************************************************************************
///*!	\file FlexRadioList.cs
// *	\brief Open Auth0 
// *
// *	\copyright	Copyright 2019 MKCM Software, portions FlexRadio Systems.  All Rights Reserved.
// *				MIT License
// *
// *	\date 2019-01-20
// *	\author Mark Hanson, W3II, based upon APIs from FlexRadio Systems
// */
// ****************************************************************************
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Flex.Smoothlake.FlexLib;
using Flex.UiWpfFramework.Mvvm;
using Util;

namespace SmartLinkWfpClient
{
    public class FlexRadioList : ObservableObject
	{
		public FlexRadioList()
		{
			this._RadiosFound = new SafeObservableCollection<RadioViewData>();
			this._SmartLinkRadios = new List<RadioViewData>();
			this._LocalRadios = new List<RadioViewData>();
			API.RadioAdded += this.API_RadioAdded;
			API.RadioRemoved += this.API_RadioRemoved;
		    API.WanListReceived += this.API_WanListReceived;
	
			foreach (Radio radio in API.RadioList.ToList<Radio>())
			{
				this.API_RadioAdded(radio);
			}
		}

		private void API_WanListReceived(List<Radio> radioListFromServer)
		{
			string lastSelectedWanRadioSerial = "";
			bool selradioiswan = _selectedRadio != null && _selectedRadio.Radio.IsWan;
			lock (_sllock)
			{
				if (selradioiswan)
				{
					lastSelectedWanRadioSerial = _selectedRadio.Radio.Serial;
				}
				_SmartLinkRadios.Clear();
                foreach(Radio r in radioListFromServer)
                {
                    _SmartLinkRadios.Add(new RadioViewData(r));

                }
				base.RaisePropertyChanged("SmartLinkRadios");
				this.RemoveAllFoundWanRadios();
				this.AddWanRadiosToList();
				this.UpdateSelectedSmartLinkRadio();
			}
			if (selradioiswan)
			{
				InvokeHelper.BeginInvokeIfNeeded(Application.Current.Dispatcher, delegate
				{
                    RadioViewData rvd = this._RadiosFound.FirstOrDefault((RadioViewData r) => r.Radio.Serial == lastSelectedWanRadioSerial);
					if (rvd != null)
					{
						this.SelectedRadio = rvd;
					}
				});
			}
		}


        private void RemoveAllFoundWanRadios()
		{
			foreach (RadioViewData fr in _RadiosFound.ToList<RadioViewData>())
			{
				if (fr.Radio.IsWan)
				{
					this._RadiosFound.Remove(fr);
				}
			}
			this.UpdateSelectedSmartLinkRadio();
		}

		private void AddWanRadiosToList()
		{
			using (List<RadioViewData>.Enumerator enumerator = _SmartLinkRadios.ToList<RadioViewData>().GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
                    RadioViewData wanRvm = enumerator.Current;
					bool notfound1 = this._LocalRadios.FirstOrDefault((RadioViewData lanRvm) => lanRvm.Radio.Serial == wanRvm.Radio.Serial) == null;
					bool notfound2 = this._RadiosFound.FirstOrDefault((RadioViewData rvm) => rvm.Radio.Serial == wanRvm.Radio.Serial) == null;
					if (notfound1 && notfound2)
					{
						this._RadiosFound.Add(wanRvm);
					}
				}
			}
		}

        public bool IsRadioBothWanAndLan(string serial)
        {
            bool flag = this._LocalRadios.FirstOrDefault((RadioViewData r) => r.Radio.Serial == serial) != null;
            bool flag2 = this._SmartLinkRadios.FirstOrDefault((RadioViewData r) => r.Radio.Serial == serial) != null;
            return flag && flag2;
        }

        public void RemoveAllWanRadios()
		{
			API_WanListReceived(new List<Radio>());
		}

		private void API_RadioAdded(Radio radio)
		{
			bool notfound = _LocalRadios.FirstOrDefault((RadioViewData r) => r.Radio.Serial == radio.Serial) == null;
			if (notfound)
			{
                RadioViewData newRvm = new RadioViewData(radio);
				_LocalRadios.Add(newRvm);
				_RadiosFound.Add(newRvm);
                RadioViewData rr = _RadiosFound.FirstOrDefault((RadioViewData r) => r.Radio.IsWan && r.Radio.Serial == newRvm.Radio.Serial);
				if (rr != null)
				{
					_RadiosFound.Remove(rr);
				}
			}
		}

		private void API_RadioRemoved(Radio radio)
		{
			RemoveRadioFromLists(radio);
		}

		public void Radio_Disconnected(Radio radio)
		{
			RemoveRadioFromLists(radio);
		}

		private void RemoveRadioFromLists(Radio radio)
		{
			lock (this)
			{
                RadioViewData rr = this._LocalRadios.FirstOrDefault((RadioViewData r) => r.Radio.Serial == radio.Serial);
				if (rr != null)
				{
					this._LocalRadios.Remove(rr);
					this._RadiosFound.Remove(rr);
				}
			}
		}

		public SafeObservableCollection<RadioViewData> RadiosFound
		{
			get
			{
				return this._RadiosFound;
			}
		}

		public RadioViewData SelectedRadio
		{
			get
			{
				return this._selectedRadio;
			}
			set
			{
				if (this._selectedRadio != value)
				{
					this._selectedRadio = value;
					this.UpdateSelectedSmartLinkRadio();
					base.RaisePropertyChanged("SelectedRadio");
				}
			}
		}

		private void UpdateSelectedSmartLinkRadio()
		{
			if (this._selectedRadio == null || this._selectedRadio.Radio == null || this._selectedRadio.Radio.IsWan)
			{
				this.SelectedSmartLinkRadio = null;
				return;
			}
			this.SelectedSmartLinkRadio = _SmartLinkRadios.FirstOrDefault((RadioViewData r) => r.Radio.Serial == _selectedRadio.Radio.Serial);
		}

		public RadioViewData SelectedSmartLinkRadio
		{
			get
			{
				return this._selectedSmartLinkRadio;
			}
			set
			{
				this._selectedSmartLinkRadio = value;
				base.RaisePropertyChanged("SelectedSmartLinkRadio");
			}
		}

		public List<RadioViewData> LocalRadios
		{
			get
			{
				return this._LocalRadios;
			}
		}

		public List<RadioViewData> SmartLinkRadios
		{
			get
			{
				return this._SmartLinkRadios;
			}
		}

		private object _sllock = new object();

		private SafeObservableCollection<RadioViewData> _RadiosFound;

		private RadioViewData _selectedRadio;

		private RadioViewData _selectedSmartLinkRadio;

		private List<RadioViewData> _LocalRadios;

		private List<RadioViewData> _SmartLinkRadios;
	}
}
