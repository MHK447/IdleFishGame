using UnityEngine;
using System.Collections.Generic;

namespace BanpoFri
{
    [System.Serializable]
    public class SeaInfoData
    {
        [SerializeField]
		private int _idx;
		public int idx
		{
			get { return _idx;}
			set { _idx = value;}
		}
		[SerializeField]
		private List<int> _inhabit_fish;
		public List<int> inhabit_fish
		{
			get { return _inhabit_fish;}
			set { _inhabit_fish = value;}
		}
		[SerializeField]
		private int _prefab_sea_idx;
		public int prefab_sea_idx
		{
			get { return _prefab_sea_idx;}
			set { _prefab_sea_idx = value;}
		}
		[SerializeField]
		private int _gradation_on;
		public int gradation_on
		{
			get { return _gradation_on;}
			set { _gradation_on = value;}
		}

    }

    [System.Serializable]
    public class SeaInfo : Table<SeaInfoData, int>
    {
    }
}

