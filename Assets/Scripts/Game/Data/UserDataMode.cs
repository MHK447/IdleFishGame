using System.Collections;
using System;
using System.Numerics;
using System.Collections.Generic;
using UniRx;

public interface IUserDataMode
{
	IReactiveProperty<BigInteger> Money { get; set; }
	DateTime LastLoginTime { get; set; }
	DateTime CurPlayDateTime { get; set; }
	IReactiveProperty<BigInteger> EnergyMoney { get; set; }
	IReactiveProperty<int> GachaCoin { get; set; }
	public List<StageFishUpgradeData> FishUpgradeDatas { get; set; }

	public PlayerData PlayerData { get; set; }

	public IReactiveCollection<NoticeData> NoticeCollections {get; set;}

	public IReactiveProperty<int> BoostTime { get; set; }

	public SelectFoodUpgradeData SelectFoodUpgradeData { get; set; }

}

public class UserDataMain : IUserDataMode
{
	public IReactiveProperty<BigInteger> Money { get; set; } = new ReactiveProperty<BigInteger>(0);
	public DateTime LastLoginTime { get; set; } = default(DateTime);
	public DateTime CurPlayDateTime { get; set; } = new DateTime(1, 1, 1);
	public IReactiveProperty<BigInteger> EnergyMoney { get; set; } = new ReactiveProperty<BigInteger>(0);
	public IReactiveProperty<int> GachaCoin { get; set; } = new ReactiveProperty<int>(0);
	public List<StageFishUpgradeData> FishUpgradeDatas { get; set; } = new List<StageFishUpgradeData>();
	public PlayerData PlayerData { get; set; } = new PlayerData();
	public IReactiveProperty<int> BoostTime { get; set; } = new ReactiveProperty<int>();

	public SelectFoodUpgradeData SelectFoodUpgradeData { get; set; } = new SelectFoodUpgradeData();

	public IReactiveCollection<NoticeData> NoticeCollections { get; set; } = new ReactiveCollection<NoticeData>();
}

public class UserDataEvent : UserDataMain
{
}