using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialEntityWaitTime : TutorialEntity
{
	[SerializeField]
	float waitTime;

	public override void StartEntity()
	{
		base.StartEntity();
		StartCoroutine(WaitTime());
	}

	IEnumerator WaitTime()
	{
		yield return new WaitForSeconds(waitTime);
		Done();
	}
}
