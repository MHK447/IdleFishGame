using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialMap : MonoBehaviour
{
    //[SerializeField]
    //private Button skipBtn;
    Queue<TutorialEntity> scenario;
    [SerializeField]
    private int tutoNum = 0;
    private int scenarioSize = 0;
    private int scenarioCurSize = 0;

    private TutorialEntity curEntity = null;

    private void Awake()
    {
//        skipBtn.onClick.AddListener(OnClickSkip);
//#if !TREEPLLA_LOG
//		skipBtn.gameObject.SetActive(false);
//#else
//        skipBtn.gameObject.SetActive(true);
//#endif
    }

    public void StartMap()
    {
        scenario = new Queue<TutorialEntity>(GetComponentsInChildren<TutorialEntity>(true));
        scenarioCurSize = scenarioSize = scenario.Count;
        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        curEntity = scenario.Dequeue();
        curEntity.gameObject.SetActive(true);
        curEntity.StartEntity();
        while (true)
        {
            if (curEntity.Complete)
            {
                --scenarioCurSize;
                //logs
            
                if (scenario.Count > 0)
                {
                    Destroy(curEntity.gameObject);
                    curEntity = scenario.Dequeue();
                    curEntity.gameObject.SetActive(true);
                    curEntity.StartEntity();
                }
                else
                {
                    break;
                }
            }
            else
            {
                yield return null;
            }
        }

        GameRoot.Instance.TutorialSystem.EndTuto();
        //if (tutoNum == 4)
        //{
        //    GameRoot.Instance.PluginSystem.AnalyticsProp.TargetEvent(TpAnalyticsProp.Analytics.Firebase, IngameEventType.None,
        //        "m_tutorial_finish");
        //}

        Destroy(this.gameObject);
    }


    public Action skipAction;
    private void OnClickSkip()
    {
        skipAction?.Invoke();

        curEntity.Complete = true;
        scenario.Clear();

        //if (GameRoot.Instance.CurGameType != GameType.OutGame)
        //{
        //    GameRoot.Instance.InGameSystem.GetInGame<InGameTycoon>().CalculateGameSpeed();
        //}


        //if (regi != null)
        //{
        //    var bc = regi.gameObject.GetComponent<BoxCollider2D>();
        //    if (bc != null)
        //        UnityEngine.Object.Destroy(bc);
        //}

        GameRoot.Instance.UISystem.SetFloatingUIActiveAll(true);
    }
}
