using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialRegister : MonoBehaviour
{
	public TutorialIdent index;
	public GameObject Target;
	private void Awake()
	{
		if(index == TutorialIdent.None)
			return;
			
		if(Target == null)
			Target = this.gameObject;
		GameRoot.Instance.TutorialSystem.AddRegister(index, this);
	}

	public void AddData(TutorialIdent _index)
	{
		index = _index;
		Target = this.gameObject;
		GameRoot.Instance.TutorialSystem.AddRegister(index, this);
	}

    private void OnDestroy()
    {
		if(GameRoot.GetInstance() != null)
			GameRoot.Instance.TutorialSystem.RemoveRegister(index);
    }
}
