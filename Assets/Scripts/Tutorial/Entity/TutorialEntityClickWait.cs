using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using BanpoFri;

public class TutorialEntityClickWait : TutorialEntity
{
    [SerializeField]
    bool mask = false;
    [SerializeField]
    protected RectTransform maskTrans;
    [SerializeField]
    TutorialIdent id;
    [SerializeField]
    GameObject clickobj;
    [SerializeField]
    private int targetIdx = -1;
    [SerializeField]
    private int targetSubIdx = -1;
    [SerializeField]
    private float paddingValue = 1.5f;
    [SerializeField]
    private float paddingAddYValue = 0f;
    [SerializeField]
    private Vector2 ClickableOffsetMin;
    [SerializeField]
    private Vector2 ClickableOffsetMax;


    [SerializeField]
    private Text textContext = null;
    [SerializeField]
    private RectTransform rectText = null;
    [SerializeField]
    private string textKey = string.Empty;
    [SerializeField]
    private float textYAddPos = 0f;

    //RectTransform ClickAble;
    Rect ClickAble;
    protected GameObject target;
    bool checkUI = false;
    public override void StartEntity()
    {
        base.StartEntity();
        maskTrans.gameObject.SetActive(false);
        if (id != TutorialIdent.None)
        {
            if (GameRoot.Instance.TutorialSystem.IsDynamaicTarget(id))
            {
                target = GameRoot.Instance.TutorialSystem.GetTarget(id, targetIdx, targetSubIdx);
            }
            else
            {
                var register = GameRoot.Instance.TutorialSystem.GetRegister(id);
                if (register == null)
                    return;

                target = register.Target;
            }
        }

        if (target != null)
        {
            var viewportPointMin = Vector2.zero;
            var viewportPointMax = Vector2.zero;
            target.gameObject.SetActive(true);
            var rect = target.GetComponent<RectTransform>();
            if (rect != null)
            {
                if (clickobj != null)
                {
                    var receiveCanavs = gameObject.GetComponentInParent<Canvas>();
                    var canvas = target.gameObject.GetComponentInParent<Canvas>();
                    if (canvas != null)
                    {
                        switch (canvas.renderMode)
                        {
                            case RenderMode.ScreenSpaceOverlay:
                                {
                                    Vector2 convert = rect.position;
                                    var pivot = new Vector2(0.5f - rect.pivot.x, 0.5f - rect.pivot.y);
                                    var size = new Vector2(rect.rect.width, rect.rect.height);
                                    convert = convert + (size * rect.lossyScale * pivot);
                                    viewportPointMin = new Vector2((convert.x + ClickableOffsetMin.x - (rect.rect.width * rect.lossyScale.x * paddingValue / 2f)) / Screen.width, (convert.y + ClickableOffsetMin.y - (rect.rect.height * rect.lossyScale.y * (paddingValue + paddingAddYValue) / 2f)) / Screen.height);
                                    viewportPointMax = new Vector2((convert.x + ClickableOffsetMax.x + (rect.rect.width * rect.lossyScale.x * paddingValue / 2f)) / Screen.width, (convert.y + ClickableOffsetMax.y + (rect.rect.height * rect.lossyScale.y * (paddingValue + paddingAddYValue) / 2f)) / Screen.height);

                                    ClickAble = new Rect(new Vector2(convert.x - (rect.rect.width * rect.lossyScale.x * paddingValue / 2f), convert.y - (rect.rect.height * rect.lossyScale.y * (paddingValue + paddingAddYValue) / 2f)), size * rect.lossyScale * (paddingValue + paddingAddYValue));
                                }
                                break;
                            case RenderMode.WorldSpace:
                                {
                                    Camera cam = GameRoot.Instance.InGameSystem.CurInGame.MainCamera;

                                    if (cam == null)
                                    {
                                        return;
                                    }

                                    var worldMin = rect.TransformPoint(new Vector2(rect.rect.xMin + ClickableOffsetMin.x, rect.rect.yMin + ClickableOffsetMin.y) * paddingValue);
                                    viewportPointMin = cam.WorldToViewportPoint(worldMin);
                                    var worldMax = rect.TransformPoint(new Vector2(rect.rect.xMax + ClickableOffsetMax.x, rect.rect.yMax + ClickableOffsetMax.y) * paddingValue);
                                    viewportPointMax = cam.WorldToViewportPoint(worldMax);

                                    var ScreenMin = cam.WorldToScreenPoint(worldMin);
                                    var ScreenMax = cam.WorldToScreenPoint(worldMax);
                                    ScreenMin = new Vector2(ScreenMin.x, ScreenMin.y);
                                    ScreenMax = new Vector2(ScreenMax.x, ScreenMax.y);
                                    ClickAble = new Rect(ScreenMin, ScreenMax - ScreenMin);

                                    //checkUI = true;
                                }
                                break;
                        }
                    }
                    clickobj.GetComponent<RectTransform>().anchorMin = viewportPointMin;
                    clickobj.GetComponent<RectTransform>().anchorMax = viewportPointMax;
                    clickobj.SetActive(true);

                    if (mask)
                    {
                        maskTrans.gameObject.SetActive(true);
                        maskTrans.anchorMin = viewportPointMin;
                        maskTrans.anchorMax = viewportPointMax;
                        maskTrans.offsetMin = Vector2.zero;
                        maskTrans.offsetMax = Vector2.zero;
                    }

                }
            }
            else
            {
                Camera cam = GameRoot.Instance.InGameSystem.CurInGame.MainCamera;


                if (cam == null)
                {
                    return;
                }

                var collider = target.GetComponent<BoxCollider2D>();
                if (collider != null)
                {
                    float halfX = collider.size.x / 2f * paddingValue;
                    float halfY = collider.size.y / 2f * (paddingValue + paddingAddYValue);
                    var worldPosMin = new Vector3(target.transform.position.x + ((collider.offset.x - halfX) * Mathf.Abs(target.transform.lossyScale.x)),
                            target.transform.position.y + ((collider.offset.y - halfY) * target.transform.lossyScale.y), target.transform.position.z * target.transform.lossyScale.z);
                    viewportPointMin = cam.WorldToViewportPoint(worldPosMin);
                    var worldPosMax = new Vector3(target.transform.position.x + ((collider.offset.x + halfX) * Mathf.Abs(target.transform.lossyScale.x)),
                            target.transform.position.y + ((collider.offset.y + halfY) * target.transform.lossyScale.y), target.transform.position.z * target.transform.lossyScale.z);
                    viewportPointMax = cam.WorldToViewportPoint(worldPosMax);

                    var clickRect = clickobj.GetComponent<RectTransform>();
                    clickRect.anchorMin = viewportPointMin;
                    clickRect.anchorMax = viewportPointMax;

                    ClickAble = RectTransformToScreenSpace(clickRect);
                    if (mask)
                    {
                        maskTrans.gameObject.SetActive(true);
                        maskTrans.anchorMin = viewportPointMin;
                        maskTrans.anchorMax = viewportPointMax;
                        maskTrans.offsetMin = Vector2.zero;
                        maskTrans.offsetMax = Vector2.zero;
                    }
                }
            }
            if (textContext != null)
            {
                textContext.text = Tables.Instance.GetTable<Localize>().GetString(textKey);
                var clickRect = clickobj.GetComponent<RectTransform>();
                rectText.position = clickRect.position;
                rectText.anchoredPosition = new Vector2(0f, rectText.anchoredPosition.y + textYAddPos);
            }
        }
        else
        {
            ClickAble = new Rect(0, 0, Screen.width, Screen.height);
            maskTrans.gameObject.SetActive(true);
        }

        //StartCoroutine(Co_EffectShow());
    }

    public Rect RectTransformToScreenSpace(RectTransform _transform)
    {
        Vector2 size = Vector2.Scale(_transform.rect.size, _transform.lossyScale);
        return new Rect((Vector2)_transform.position - (size * _transform.pivot), size);
    }

    bool isClicked = false;
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isClicked = true;
        }

        if (isClicked && Input.GetMouseButtonUp(0))
        {
            if (checkUI)
            {
                Vector2 lp;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, Input.mousePosition, null, out lp);
                if (ClickAble.Contains(lp))
                {
                    Click();
                }
            }
            else
            {
                if (ClickAble.Contains(Input.mousePosition))
                {
                    Click();
                }
            }
        }
    }

    void Click()
    {
        maskTrans.gameObject.SetActive(false);
        Done();
        if (target != null)
        {
            var btn = target.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.Invoke();
            }
            else
            {
                var toggle = target.GetComponent<Toggle>();
                if (toggle != null)
                {
                    var pointerEvent = new PointerEventData(UnityEngine.EventSystems.EventSystem.current);
                    ExecuteEvents.Execute(target, pointerEvent, ExecuteEvents.pointerClickHandler);
                }
                else
                {
                    var cc = target.GetComponent<ClickCallback>();
                    if (cc != null)
                    {
                        cc.TutorialClick();
                    }
                    else
                    {
                        var btnPressed = target.GetComponent<ButtonPressed>();
                        if (btnPressed != null)
                        {
                           var pointerEvent = new PointerEventData(UnityEngine.EventSystems.EventSystem.current);
                           btnPressed.OnPointerUp(pointerEvent);
                        }
                    }
                }
            }
        }

        //string[] TutoIdx = { "One", "Two", "Three", "Four", "Five", "Six", "Seven" };

        //for(int i = 1; i <= TutoIdx.Length; i++)
        //      {
        //	int index = i;
        //	if(this.transform.parent.name.Contains(TutoIdx[index - 1]))
        //          {
        //		//GameRoot.Instance.PluginSystem.AnalyticsProp.AllEvent(IngameEventType.None, $"m_funnel_tutorial_{index}_{tutoStep}");
        //		break;
        //          }
        //      }

        //
    }

    // IEnumerator Co_EffectShow()
    // {
    //     var effect = Instantiate(Effect.gameObject);
    //     effect.gameObject.SetActive(true);
    //     yield return new WaitForSeconds(3);
    //     Destroy(effect.gameObject);
    // }
}
