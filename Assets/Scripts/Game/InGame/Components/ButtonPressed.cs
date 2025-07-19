using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonPressed : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField]
    private string NormalTrigger = "Normal";
    [SerializeField]
    private string PressedTrigger = "Pressed";
    [SerializeField]
    private string DisabledTrigger = "Disabled";

    private bool interactable = true;
    public bool Interactable
    {
        get
        {
            return interactable;
        }
        set
        {
            if (interactable != value)
            {
                if (value)
                {
                    if (animator != null)
                        animator.Play(NormalTrigger, 0, 0f);
                }
                else
                {
                    if (animator != null)
                        animator.Play(DisabledTrigger, 0, 0f);
                    pressedCnt = 0;

                }
            }
            interactable = value;
        }
    }
    public float click_interval = 0.5f;
    public float click_interval_fast = 0.03f;
    public int fastPressCnt = 10;
    private bool pressed = false;
    private float deltaTime = 0f;
    private int pressedCnt = 0;
    public System.Action OnPressed = null;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        RefreshState();
    }

    void OnEnable()
    {
        RefreshState();
    }

    void RefreshState()
    {
        if (interactable)
        {
            if (animator != null)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                if (!stateInfo.IsName(NormalTrigger))
                {
                    animator.Play(NormalTrigger, 0, 0f);
                }
            }
        }
        else
        {
            pressedCnt = 0;

            if (animator != null)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                if (!stateInfo.IsName(DisabledTrigger))
                {
                    animator.Play(DisabledTrigger, 0, 0f);
                }
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!interactable)
            return;

        pressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!interactable)
            return;

        pressed = false;
        if (pressedCnt < 1)
        {
            if (animator != null)
                animator.Play(PressedTrigger, 0, 0f);
            OnPressed?.Invoke();
            SoundPlayer.Instance.PlaySound("btn");
        }
        pressedCnt = 0;
        deltaTime = 0f;

    }


    private void OnDisable()
    {
        pressed = false;
        pressedCnt = 0;
        deltaTime = 0f;
    }

    public void OnDrag(PointerEventData eventData)
    {
    }

    public void OnCancel()
    {
        pressed = false;
        pressedCnt = 0;
        deltaTime = 0;
    }

    private void Update()
    {
        if (pressed)
        {
            if (!interactable)
            {
                pressed = false;
                return;
            }

            if ((pressedCnt < fastPressCnt ? click_interval : click_interval_fast) < deltaTime)
            {
                if (animator != null)
                    animator.Play(NormalTrigger, 0, 0f);
                SoundPlayer.Instance.PlaySound("btn");
                OnPressed?.Invoke();
                ++pressedCnt;
                deltaTime = 0f;
            }
            deltaTime += Time.deltaTime;
        }
    }
}

/*
 * 가속도 방식
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonPressed : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private bool interactable = true;
    public bool Interactable
    {
        get
        {
            return interactable;
        }
        set
        {
            if (interactable != value)
            {
                if (!value)
                {
                    if (animator != null)
                        animator.Play("Disabled", 0, 0f);
                    pressedCnt = 0;
                }
                else
                {
                    if (animator != null)
                        animator.Play("Normal", 0, 0f);
                }
            }
            interactable = value;
        }
    } 

    [SerializeField]
    private float min_interval = 1f;                                            //최소 시간
    [SerializeField]
    private float click_interval_max = 0.5f;                                    //클릭 주기 최대값
    [SerializeField]
    private float click_interval_min = 0.01f;                                   //클릭 주기 최소값

    [SerializeField]
    private int fastPressCnt = 3;                                               //가속되기 위한 클릭 수

    private float cur_interval;
    private int cur_mulValue;

    private bool pressed = false;
    private bool minTime = false;

    private int pressedCnt = 0;
    private float deltaTime = 0f;

    public System.Action<int> OnPressed = null;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        cur_interval = click_interval_max;
        cur_mulValue = 1;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!interactable)
            return;

        pressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!interactable)
            return;

        pressed = false;
        minTime = false;
        if (pressedCnt < 1)
        {
            if (animator != null)
                animator.Play("Pressed", 0, 0f);
            OnPressed?.Invoke(cur_mulValue);
            SoundPlayer.Instance.PlaySound("btn");
        }
        pressedCnt = 0;
        deltaTime = 0f;
        cur_interval = click_interval_max;
        cur_mulValue = 1;
    }


    private void OnDisable()
    {
        pressed = false;
        minTime = false;
        pressedCnt = 0;
        deltaTime = 0f;
        cur_interval = click_interval_max;
        cur_mulValue = 1;
    }

    public void OnDrag(PointerEventData eventData)
    {
    }

    public void OnCancel()
    {
        pressed = false;
        pressedCnt = 0;
        deltaTime = 0;
        cur_interval = click_interval_max;
        cur_mulValue = 1;
    }

    private void Update()
    {
        if (pressed)
        {
            if (!interactable)
            {
                pressed = false;
                return;
            }

            if (!minTime)
            {
                deltaTime += Time.deltaTime;

                if (deltaTime > min_interval)
                {
                    deltaTime = 0;
                    minTime = true;
                }
            }

            if (minTime)
            {
                if (deltaTime > cur_interval)
                {
                    if (animator != null)
                        animator.Play("Pressed", 0, 0f);
                    SoundPlayer.Instance.PlaySound("btn");

                    OnPressed?.Invoke(cur_mulValue);
                    ++pressedCnt;
                    deltaTime = 0f;

                    if (pressedCnt > fastPressCnt)
                    {
                        if (cur_interval > click_interval_min)
                        {
                            cur_interval = cur_interval * 0.5f;
                        }

                        if (cur_interval < click_interval_min)
                        {
                            cur_interval = click_interval_min;
                            cur_mulValue++;
                        }

                        pressedCnt = 0;
                    }
                }
            }

            deltaTime += Time.deltaTime;
        }
    }
}
*/