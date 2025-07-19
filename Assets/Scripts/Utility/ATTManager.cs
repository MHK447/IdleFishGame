using System;
using UnityEngine;
using System.Runtime.InteropServices;

public class ATTManager : MonoBehaviour
{
    public static ATTManager Instance { get; private set; }
    
    [Header("ATT Settings")]
    [SerializeField] private bool requestOnStart = true;
    [SerializeField] private float delayBeforeRequest = 0.5f; // 시작 후 0.5초 대기 (광고 초기화 최적화)
    
    public event Action<bool> OnATTResponse;
    
#if UNITY_IOS && !UNITY_EDITOR
    // iOS 네이티브 함수 바인딩 (약한 링킹)
    [DllImport("__Internal")]
    private static extern int ATTrackingManagerGetTrackingAuthorizationStatus();
    
    [DllImport("__Internal")]
    private static extern void ATTrackingManagerRequestTrackingAuthorization();
    
    // 시스템 버전 확인
    [DllImport("__Internal")]
    private static extern string _GetSystemVersion();
    
    // iOS 버전 확인을 위한 헬퍼 메서드
    private static bool IsIOSVersionSupported()
    {
        try
        {
            string version = SystemInfo.operatingSystem;
            // "iPhone OS 14.5" 형식에서 버전 추출
            string[] parts = version.Split(' ');
            if (parts.Length >= 3)
            {
                string versionStr = parts[2];
                if (float.TryParse(versionStr, out float versionNum))
                {
                    return versionNum >= 14.0f;
                }
            }
            return false;
        }
        catch
        {
            return false;
        }
    }
#endif
    
    // ATT 상태 열거형
    public enum ATTStatus
    {
        NotDetermined = 0,
        Restricted = 1,
        Denied = 2,
        Authorized = 3
    }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        if (requestOnStart)
        {
            Debug.Log("ATT 팝업 요청 예약됨 - " + delayBeforeRequest + "초 후");
            Invoke(nameof(RequestATT), delayBeforeRequest);
        }
    }
    
    /// <summary>
    /// iOS ATT 팝업을 요청합니다.
    /// </summary>
    public void RequestATT()
    {
        Debug.Log("RequestATT 호출됨");
        
#if UNITY_IOS && !UNITY_EDITOR
        try 
        {
            // iOS 14 이상에서만 ATT 지원
            if (!IsIOSVersionSupported())
            {
                Debug.Log("iOS 14 미만에서는 ATT가 지원되지 않습니다. 허용으로 처리합니다.");
                OnATTResponse?.Invoke(true);
                NotifyAdSDKs(true);
                return;
            }
            
            // iOS 14 이상에서만 ATT 팝업 표시
            ATTStatus currentStatus = GetTrackingAuthorizationStatus();
            Debug.Log($"현재 ATT 상태: {currentStatus}");
            
            if (currentStatus == ATTStatus.NotDetermined)
            {
                Debug.Log("ATT 팝업 요청 중...");
                
                // 네이티브 함수 호출
                ATTrackingManagerRequestTrackingAuthorization();
                
                // 결과 확인 (약간의 지연 후)
                Invoke(nameof(CheckATTResult), 1f);
            }
            else
            {
                // 이미 결정된 상태인 경우
                bool isAuthorized = currentStatus == ATTStatus.Authorized;
                Debug.Log($"ATT 이미 결정됨: {currentStatus} (허용됨: {isAuthorized})");
                
                OnATTResponse?.Invoke(isAuthorized);
                NotifyAdSDKs(isAuthorized);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ATT 요청 중 오류: {e.Message}");
            Debug.LogError($"스택 트레이스: {e.StackTrace}");
            OnATTResponse?.Invoke(true);
            NotifyAdSDKs(true);
        }
#else
        Debug.Log("ATT는 iOS 빌드에서만 지원됩니다. (에디터에서는 시뮬레이션)");
        OnATTResponse?.Invoke(true); // 에디터/다른 플랫폼에서는 허용으로 처리
#endif
    }
    
    private void CheckATTResult()
    {
#if UNITY_IOS && !UNITY_EDITOR
        ATTStatus status = GetTrackingAuthorizationStatus();
        bool isAuthorized = status == ATTStatus.Authorized;
        Debug.Log($"ATT 응답: {status} (허용됨: {isAuthorized})");
        
        OnATTResponse?.Invoke(isAuthorized);
        NotifyAdSDKs(isAuthorized);
#endif
    }
    
    /// <summary>
    /// 현재 ATT 상태를 확인합니다.
    /// </summary>
    public bool IsTrackingAuthorized()
    {
#if UNITY_IOS && !UNITY_EDITOR
        try 
        {
            return GetTrackingAuthorizationStatus() == ATTStatus.Authorized;
        }
        catch
        {
            return true;
        }
#else
        return true; // 다른 플랫폼에서는 허용으로 처리
#endif
    }
    
    /// <summary>
    /// ATT 상태를 가져옵니다.
    /// </summary>
    public ATTStatus GetTrackingAuthorizationStatus()
    {
#if UNITY_IOS && !UNITY_EDITOR
        try 
        {
            int status = ATTrackingManagerGetTrackingAuthorizationStatus();
            return (ATTStatus)status;
        }
        catch
        {
            return ATTStatus.Authorized;
        }
#else
        return ATTStatus.Authorized; // 다른 플랫폼에서는 허용으로 처리
#endif
    }
    
    /// <summary>
    /// ATT 상태를 문자열로 반환합니다.
    /// </summary>
    public string GetTrackingStatusString()
    {
#if UNITY_IOS && !UNITY_EDITOR
        try 
        {
            var status = GetTrackingAuthorizationStatus();
            return status switch
            {
                ATTStatus.NotDetermined => "결정되지 않음",
                ATTStatus.Restricted => "제한됨",
                ATTStatus.Denied => "거부됨",
                ATTStatus.Authorized => "허용됨",
                _ => "알 수 없음"
            };
        }
        catch
        {
            return "오류";
        }
#else
        return "에디터/안드로이드";
#endif
    }
    
    /// <summary>
    /// 광고 SDK들에게 추적 상태를 알립니다.
    /// </summary>
    private void NotifyAdSDKs(bool isAuthorized)
    {
        Debug.Log($"광고 SDK에 ATT 상태 알림: {isAuthorized}");
        
        // Google Mobile Ads에 알림
        if (GameRoot.Instance != null && GameRoot.Instance.GetAdManager != null)
        {
            // AdManager에 ATT 상태 전달 (필요시 AdManager에 메서드 추가)
        }
        
        // Firebase Analytics 등 다른 SDK에도 알림 가능
    }
    
    /// <summary>
    /// 수동으로 ATT 팝업을 다시 요청 (테스트용)
    /// </summary>
    [ContextMenu("Test ATT Request")]
    public void TestATTRequest()
    {
        RequestATT();
    }
} 