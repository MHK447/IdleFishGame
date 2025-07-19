using UnityEngine;
using System.Collections.Generic;

namespace BanpoFri
{
    [System.Serializable]
    public class FoodMergeGroupInfoData
    {
        [SerializeField]
		private int _stageidx;
		public int stageidx
		{
			get { return _stageidx;}
			set { _stageidx = value;}
		}
		[SerializeField]
		private int _mergeidx;
		public int mergeidx
		{
			get { return _mergeidx;}
			set { _mergeidx = value;}
		}
		[SerializeField]
		private int _goal_count;
		public int goal_count
		{
			get { return _goal_count;}
			set { _goal_count = value;}
		}
		[SerializeField]
		private string _stage_prefab;
		public string stage_prefab
		{
			get { return _stage_prefab;}
			set { _stage_prefab = value;}
		}

    }

    [System.Serializable]
    public class FoodMergeGroupInfo : Table<FoodMergeGroupInfoData, KeyValuePair<int,int>>
    {
    }
}

