using UnityEngine;
using System.Collections.Generic;

namespace BanpoFri
{
    [System.Serializable]
    public class FishInfoData
    {
        [SerializeField]
		private int _idx;
		public int idx
		{
			get { return _idx;}
			set { _idx = value;}
		}
		[SerializeField]
		private int _move_type;
		public int move_type
		{
			get { return _move_type;}
			set { _move_type = value;}
		}
		[SerializeField]
		private int _weight_price;
		public int weight_price
		{
			get { return _weight_price;}
			set { _weight_price = value;}
		}
		[SerializeField]
		private int _fish_weight_min;
		public int fish_weight_min
		{
			get { return _fish_weight_min;}
			set { _fish_weight_min = value;}
		}
		[SerializeField]
		private int _fish_weight_max;
		public int fish_weight_max
		{
			get { return _fish_weight_max;}
			set { _fish_weight_max = value;}
		}
		[SerializeField]
		private int _money_value;
		public int money_value
		{
			get { return _money_value;}
			set { _money_value = value;}
		}
		[SerializeField]
		private string _fish_name;
		public string fish_name
		{
			get { return _fish_name;}
			set { _fish_name = value;}
		}
		[SerializeField]
		private string _fish_desc;
		public string fish_desc
		{
			get { return _fish_desc;}
			set { _fish_desc = value;}
		}

    }

    [System.Serializable]
    public class FishInfo : Table<FishInfoData, int>
    {
    }
}

