using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ToggleController : MonoBehaviour
{
	public enum dir
    {
		leftOn,
		rightOn
    }

	public bool isOn;

	public Color onColorBg;
	public Color offColorBg;

	public Image toggleBgImage;
	public RectTransform toggle;
	public Button toggleBtn;

	public GameObject handle;
	private RectTransform handleTransform;

	private float handleSize;
	private float onPosX;
	private float offPosX;

	public float handleOffset;

	public GameObject[] onIcon;
	public GameObject[] offIcon;

	public float speed;
	static float t = 0.0f;

	public dir direction = dir.rightOn;

	private bool switching = false;

	private System.Action<bool> clickAction;


	void Awake()
	{
		handleTransform = handle.GetComponent<RectTransform>();
		RectTransform handleRect = handle.GetComponent<RectTransform>();
		handleSize = handleRect.sizeDelta.x;
		float toggleSizeX = toggle.sizeDelta.x;

		if(direction == dir.rightOn)
		{
			onPosX = (toggleSizeX / 2) - (handleSize / 2) - handleOffset;
			offPosX = onPosX * -1;
        }
        else
        {
			offPosX = (toggleSizeX / 2) - (handleSize / 2) - handleOffset;
			onPosX = offPosX * -1;
		}

		toggleBtn.onClick.AddListener(Switching);
	}

	public void Interactable(bool value)
	{
		toggleBtn.interactable = value;
	}

	public void Init(bool status)
	{
		isOn = status;
	}

	void Start()
	{
		if (isOn)
		{
			toggleBgImage.color = onColorBg;
			handleTransform.localPosition = new Vector3(onPosX, 0f, 0f);
			foreach (var icon in onIcon) icon.gameObject.SetActive(true);
			foreach (var icon in offIcon) icon.gameObject.SetActive(false);
		}
		else
		{
			toggleBgImage.color = offColorBg;
			handleTransform.localPosition = new Vector3(offPosX, 0f, 0f);
			foreach (var icon in onIcon) icon.gameObject.SetActive(false);
			foreach (var icon in offIcon) icon.gameObject.SetActive(true);
		}
	}

	void Update()
	{

		if (switching)
		{
			Toggle(isOn);
		}
	}

	public void setToggleListener(System.Action<bool> toggleAction)
	{
		clickAction = null;
		clickAction = toggleAction;
	}

	public void DoYourStaff()
	{
		if (clickAction != null) clickAction(isOn);
	}

	public void Switching()
	{
		switching = true;
	}


	public void Toggle(bool toggleStatus)
	{
		//if (!onIcon.activeSelf || !offIcon.activeSelf)
		//{
		//	onIcon.SetActive(true);
		//	offIcon.SetActive(true);
		//}
		foreach (var icon in onIcon) icon.gameObject.SetActive(true);
		foreach (var icon in offIcon) icon.gameObject.SetActive(true);

		if (toggleStatus)
		{
			toggleBgImage.color = SmoothColor(onColorBg, offColorBg);
			foreach (var icon in onIcon) Transparency(icon, 1f, 0f);
			foreach (var icon in offIcon) Transparency(icon, 0f, 1f);
			handleTransform.localPosition = SmoothMove(handle, onPosX, offPosX);
		}
		else
		{
			toggleBgImage.color = SmoothColor(offColorBg, onColorBg);
			foreach (var icon in onIcon) Transparency(icon, 0f, 1f);
			foreach (var icon in offIcon) Transparency(icon, 1f, 0f);
			handleTransform.localPosition = SmoothMove(handle, offPosX, onPosX);
		}

	}


	Vector3 SmoothMove(GameObject toggleHandle, float startPosX, float endPosX)
	{

		Vector3 position = new Vector3(Mathf.Lerp(startPosX, endPosX, t += speed * Time.deltaTime), 0f, 0f);
		StopSwitching();
		return position;
	}

	Color SmoothColor(Color startCol, Color endCol)
	{
		Color resultCol;
		resultCol = Color.Lerp(startCol, endCol, t += speed * Time.deltaTime);
		return resultCol;
	}

	CanvasGroup Transparency(GameObject alphaObj, float startAlpha, float endAlpha)
	{
		CanvasGroup alphaVal;
		alphaVal = alphaObj.gameObject.GetComponent<CanvasGroup>();
		if (alphaVal) alphaVal.alpha = Mathf.Lerp(startAlpha, endAlpha, t += speed * Time.deltaTime);
		else
		{
			if (endAlpha == 0f) alphaObj.SetActive(false);
			else if (startAlpha == 0f) alphaObj.SetActive(true);
		}
		return alphaVal;
	}

	void StopSwitching()
	{
		if (t > 1.0f)
		{
			switching = false;

			t = 0.0f;
			switch (isOn)
			{
				case true:
					isOn = false;
					DoYourStaff();
					break;

				case false:
					isOn = true;
					DoYourStaff();
					break;
			}

		}
	}

}
