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

    }

    [System.Serializable]
    public class SeaInfo : Table<SeaInfoData, int>
    {
    }
}

