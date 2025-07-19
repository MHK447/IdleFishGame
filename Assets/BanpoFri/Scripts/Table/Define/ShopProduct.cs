using UnityEngine;
using System.Collections.Generic;

namespace BanpoFri
{
    [System.Serializable]
    public class ShopProductData
    {
        [SerializeField]
		private int _idx;
		public int idx
		{
			get { return _idx;}
			set { _idx = value;}
		}
		[SerializeField]
		private int _type;
		public int type
		{
			get { return _type;}
			set { _type = value;}
		}
		[SerializeField]
		private List<int> _reward_type;
		public List<int> reward_type
		{
			get { return _reward_type;}
			set { _reward_type = value;}
		}
		[SerializeField]
		private List<int> _reward_idx;
		public List<int> reward_idx
		{
			get { return _reward_idx;}
			set { _reward_idx = value;}
		}
		[SerializeField]
		private List<int> _value;
		public List<int> value
		{
			get { return _value;}
			set { _value = value;}
		}
		[SerializeField]
		private string _product_id;
		public string product_id
		{
			get { return _product_id;}
			set { _product_id = value;}
		}
		[SerializeField]
		private int _consumable_check;
		public int consumable_check
		{
			get { return _consumable_check;}
			set { _consumable_check = value;}
		}

    }

    [System.Serializable]
    public class ShopProduct : Table<ShopProductData, int>
    {
    }
}

