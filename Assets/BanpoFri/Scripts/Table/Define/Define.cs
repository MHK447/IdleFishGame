using UnityEngine;
using System.Collections.Generic;

namespace BanpoFri
{
    [System.Serializable]
    public class DefineData
    {
        [SerializeField]
		private string _index;
		public string index
		{
			get { return _index;}
			set { _index = value;}
		}
		[SerializeField]
		private int _value;
		public int value
		{
			get { return _value;}
			set { _value = value;}
		}

    }

    [System.Serializable]
    public class Define : Table<DefineData, string>
    {
    }
}

