using UnityEngine;
using System.Collections.Generic;

namespace BanpoFri
{
    [System.Serializable]
    public class UpgradeInfoData
    {
        [SerializeField]
		private int _upgrade_idx;
		public int upgrade_idx
		{
			get { return _upgrade_idx;}
			set { _upgrade_idx = value;}
		}
		[SerializeField]
		private int _start_value;
		public int start_value
		{
			get { return _start_value;}
			set { _start_value = value;}
		}
		[SerializeField]
		private int _level_up_value;
		public int level_up_value
		{
			get { return _level_up_value;}
			set { _level_up_value = value;}
		}
		[SerializeField]
		private int _base_upgrade_cost;
		public int base_upgrade_cost
		{
			get { return _base_upgrade_cost;}
			set { _base_upgrade_cost = value;}
		}
		[SerializeField]
		private int _inceease_upgrade_cost;
		public int inceease_upgrade_cost
		{
			get { return _inceease_upgrade_cost;}
			set { _inceease_upgrade_cost = value;}
		}
		[SerializeField]
		private string _str_desc;
		public string str_desc
		{
			get { return _str_desc;}
			set { _str_desc = value;}
		}

    }

    [System.Serializable]
    public class UpgradeInfo : Table<UpgradeInfoData, int>
    {
    }
}

