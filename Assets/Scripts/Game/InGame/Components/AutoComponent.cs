using UnityEngine;
using BanpoFri;
using UnityEngine.UI;
using UniRx;


public class AutoComponent : MonoBehaviour
{
    [SerializeField]
    private Button AutoBtn;

    [SerializeField]
    private GameObject AutoOnObj;

    [SerializeField]
    private GameObject AutoOffObj;

    private CompositeDisposable disposables = new CompositeDisposable();

    void Awake()
    {
        AutoBtn.onClick.AddListener(OnclickAutoBtn);
    }


    void OnEnable()
    {
        ProjectUtility.SetActiveCheck(AutoOnObj, GameRoot.Instance.UserData.Fishingautoproperty.Value);
        ProjectUtility.SetActiveCheck(AutoOffObj, !GameRoot.Instance.UserData.Fishingautoproperty.Value);

        disposables.Clear();

        GameRoot.Instance.UserData.Fishingautoproperty.Subscribe(x =>
        {
            ProjectUtility.SetActiveCheck(AutoOnObj, x);
            ProjectUtility.SetActiveCheck(AutoOffObj, !x);
        }).AddTo(disposables);
    }

    void OnDisable()
    {
        disposables.Clear();
    }

    void OnDestroy()
    {
        disposables.Clear();
    }


    public void OnclickAutoBtn()
    {
        GameRoot.Instance.UserData.Fishingautoproperty.Value = !GameRoot.Instance.UserData.Fishingautoproperty.Value;
    }

}
