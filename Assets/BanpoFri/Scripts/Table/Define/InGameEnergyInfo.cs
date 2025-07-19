using UnityEngine;
using System.Collections.Generic;

namespace BanpoFri
{
    [System.Serializable]
    public class InGameEnergyInfoData
    {
        [SerializeField]
		private int _energy_idx;
		public int energy_idx
		{
			get { return _energy_idx;}
			set { _energy_idx = value;}
		}
		[SerializeField]
		private int _value;
		public int value
		{
			get { return _value;}
			set { _value = value;}
		}
		[SerializeField]
		private string _image;
		public string image
		{
			get { return _image;}
			set { _image = value;}
		}
		[SerializeField]
		private string _name;
		public string name
		{
			get { return _name;}
			set { _name = value;}
		}
		[SerializeField]
		private string _desc;
		public string desc
		{
			get { return _desc;}
			set { _desc = value;}
		}

    }

    [System.Serializable]
    public class InGameEnergyInfo : Table<InGameEnergyInfoData, int>
    {
    }
}

