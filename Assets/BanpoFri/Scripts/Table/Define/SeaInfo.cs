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
		private int _depth;
		public int depth
		{
			get { return _depth;}
			set { _depth = value;}
		}

    }

    [System.Serializable]
    public class SeaInfo : Table<SeaInfoData, int>
    {
    }
}

