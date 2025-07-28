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
		private int _min_depth;
		public int min_depth
		{
			get { return _min_depth;}
			set { _min_depth = value;}
		}
		[SerializeField]
		private int _max_depth;
		public int max_depth
		{
			get { return _max_depth;}
			set { _max_depth = value;}
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

