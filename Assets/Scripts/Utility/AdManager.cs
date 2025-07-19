using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoogleMobileAds.Api;
using GoogleMobileAds;
using System;

//"ca-app-pub-3940256099942544/1712485313" test id  reward 
//"ca-app-pub-3940256099942544/4411468910" test id  interstitial
public class AdManager : MonoBehaviour
{
    private RewardedAd _rewardedAd;
    private InterstitialAd _interstitialAd;

    // 안드로이드용 광고 ID
    private string _adUnitIdAndroid_Reward = "ca-app-pub-4449379001767537/5084819999";
    private string _adUnitIdAndroid_Interstitial = "ca-app-pub-4449379001767537/6415710398";
    
    // iOS용 광고 ID
    private string _adUnitIdIOS_Reward = "ca-app-pub-4449379001767537/2697435586";
    private string _adUnitIdIOS_Interstitial = "ca-app-pub-4449379001767537/7332467556";

    // 현재 플랫폼에 따른 광고 ID
    private string _rewardedAdUnitId;
    private string _interstitialAdUnitId;

    private bool IsInterAdLoaded = false;
    private bool IsRewardAdLoaded = false;
    private bool isInitialized = false;
    private bool isLoadingRewardedAd = false;
    private bool isLoadingInterstitialAd = false;

    // 연속 로드 실패 시 재시도 간격을 점진적으로 늘리기 위한 변수
    private int rewardedAdRetryCount = 0;
    private int interstitialAdRetryCount = 0;
    private const int MAX_RETRY_COUNT = 5;
    private const float INITIAL_RETRY_DELAY = 1f;

    // ATT 권한 상태 저장
    private bool attAuthorized = true; // 기본값: 허용 (안드로이드 및 iOS 14 미만)

    void Start()
    {
        // 플랫폼별 광고 ID 설정
        SetAdUnitIdByPlatform();
        
        // iOS에서는 ATT 권한 완료 후 초기화, 다른 플랫폼은 즉시 초기화
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            Debug.Log("iOS: ATT 권한 완료 후 AdMob 초기화 예정");
            // ATT 권한 완료 후 InitializeAds()가 호출될 것임
        }
        else
        {
            // 안드로이드 및 기타 플랫폼은 즉시 초기화
            InitializeAds();
        }
    }

    // ATT 권한 완료 후 호출될 메서드
    public void InitializeAdsAfterATT(bool attAuthorized)
    {
        Debug.Log($"ATT 권한 완료 후 AdMob 초기화 시작: {attAuthorized}");
        
        // ATT 권한 상태 저장
        this.attAuthorized = attAuthorized;
        
        // ATT 권한 상태를 AdMob에 전달하기 위한 추가 설정
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            // iOS에서는 ATT 권한 상태에 따라 광고 요청 설정
            if (!attAuthorized)
            {
                Debug.Log("ATT 권한 거부됨: 비개인화 광고만 요청");
            }
        }
        
        InitializeAds();
    }

    // 플랫폼별 광고 ID 설정
    private void SetAdUnitIdByPlatform()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            _rewardedAdUnitId = _adUnitIdAndroid_Reward;
            _interstitialAdUnitId = _adUnitIdAndroid_Interstitial;
            Debug.Log("Android 플랫폼 광고 ID 설정");
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            _rewardedAdUnitId = _adUnitIdIOS_Reward;
            _interstitialAdUnitId = _adUnitIdIOS_Interstitial;
            Debug.Log("iOS 플랫폼 광고 ID 설정");
        }
        else
        {
            // 에디터 또는 기타 플랫폼에서는 테스트 ID 사용
            _rewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917"; // 테스트 리워드 광고 ID
            _interstitialAdUnitId = "ca-app-pub-3940256099942544/1033173712"; // 테스트 전면 광고 ID
            Debug.Log("테스트 광고 ID 설정 (에디터 또는 기타 플랫폼)");
        }
    }

    // GameRoot에서 즉시 호출할 수 있는 사전 초기화 메서드
    public void PreInitialize()
    {
        // 플랫폼별 광고 ID 설정
        SetAdUnitIdByPlatform();
        
        // 이미 초기화되었거나 초기화 중인 경우 무시
        if (isInitialized) return;
        
        Debug.Log("광고 SDK 사전 초기화 시작");
        InitializeAds();
    }

    private void InitializeAds()
    {
        try
        {
            // 네트워크 상태 확인
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.LogWarning("네트워크 연결이 없습니다. 광고 초기화 지연됩니다.");
                // 3초 후 다시 시도 (더 짧은 간격으로 변경)
                GameRoot.Instance.WaitTimeAndCallback(3f, InitializeAds);
                return;
            }

            // 이미 초기화된 경우 중복 초기화 방지
            if (isInitialized) return;

            Debug.Log("광고 SDK 초기화 시작...");
            
            // 초기화 시작
            MobileAds.Initialize(initStatus => {
                isInitialized = true;
                Debug.Log("광고 SDK 초기화 완료");
                
                // 초기화 성공 후 즉시 광고 로드 시작
                LoadRewardedAd();
                
                // 전면 광고는 약간 지연시켜 리워드 광고 로드에 우선순위 부여
                GameRoot.Instance.WaitTimeAndCallback(1f, LoadInterstitialAd);
            });
        }
        catch (Exception e)
        {
            Debug.LogError("광고 초기화 중 오류: " + e.Message);
            // 오류 발생 시 더 짧은 시간 후 다시 시도
            GameRoot.Instance.WaitTimeAndCallback(3f, InitializeAds);
        }
    }   

    public void LoadInterstitialAd()
    {
        if (isLoadingInterstitialAd) return; // 중복 로드 방지
        isLoadingInterstitialAd = true;
        
        try
        {
            // 이전 광고 정리
            if (_interstitialAd != null)
            {
                _interstitialAd.Destroy();
                _interstitialAd = null;
            }

            Debug.Log("전면 광고 로딩 시작");

            var adRequest = CreateAdRequest();
            
            // 테스트 장치 설정 (현재 SDK 버전에서는 기본 AdRequest 사용)
            // 필요시 AdMob 콘솔에서 테스트 광고 활성화 또는 개발자 모드 사용

            InterstitialAd.Load(_interstitialAdUnitId, adRequest,
                (InterstitialAd ad, LoadAdError error) =>
                {
                    isLoadingInterstitialAd = false;
                    
                    if (error != null || ad == null)
                    {
                        interstitialAdRetryCount++;
                        float retryDelay = INITIAL_RETRY_DELAY * Mathf.Pow(2, interstitialAdRetryCount);
                        retryDelay = Mathf.Min(retryDelay, 60f); // 최대 60초까지 지연
                        
                        Debug.LogError($"전면 광고 로드 실패 (시도 {interstitialAdRetryCount}/{MAX_RETRY_COUNT}): {error?.ToString()}");
                        
                        if (interstitialAdRetryCount < MAX_RETRY_COUNT)
                        {
                            Debug.Log($"{retryDelay}초 후 전면 광고 다시 로드 시도");
                            GameRoot.Instance.WaitTimeAndCallback(retryDelay, LoadInterstitialAd);
                        }
                        return;
                    }

                    interstitialAdRetryCount = 0; // 성공 시 재시도 카운트 초기화
                    _interstitialAd = ad;
                    IsInterAdLoaded = true;

                    Debug.Log("전면 광고 로드 성공");

                    RegisterInterstitialEventHandlers(ad);
                });
        }
        catch (Exception e)
        {
            isLoadingInterstitialAd = false;
            Debug.LogError("전면 광고 로드 중 예외 발생: " + e.Message);
            
            // 5초 후 다시 시도
            GameRoot.Instance.WaitTimeAndCallback(5f, LoadInterstitialAd);
        }
    }

    private void RegisterInterstitialEventHandlers(InterstitialAd ad)
    {
        // 광고가 닫혔을 때
        ad.OnAdFullScreenContentClosed += () => {
            Debug.Log("전면 광고가 닫혔습니다.");
            IsInterAdLoaded = false;
            
            // 광고 닫힘 핸들러 호출 후 신규 광고 로드
            HandleInterstitialAdClosed();
        };
        
        // 광고 표시 실패 시
        ad.OnAdFullScreenContentFailed += (AdError error) => {
            Debug.LogError("전면 광고 표시 실패: " + error);
            IsInterAdLoaded = false;
            LoadInterstitialAd(); // 실패 시 즉시 새 광고 로드
        };
    }

    // 전면 광고 표시
    public void ShowInterstitialAd(System.Action onAdClosed = null)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("광고 SDK가 초기화되지 않았습니다. 광고 표시를 건너뜁니다.");
            onAdClosed?.Invoke();
            return;
        }
        
        if (IsInterAdLoaded && _interstitialAd != null && _interstitialAd.CanShowAd())
        {
            // 광고 닫힘 이벤트에 콜백 추가
            if (onAdClosed != null)
            {
                Action adClosedAction = null;
                adClosedAction = () => {
                    _interstitialAd.OnAdFullScreenContentClosed -= adClosedAction;
                    onAdClosed.Invoke();
                };
                
                _interstitialAd.OnAdFullScreenContentClosed += adClosedAction;
            }
            
            _interstitialAd.Show();
            Debug.Log("전면 광고 표시");
            IsInterAdLoaded = false;
        }
        else
        {
            Debug.Log("전면 광고가 준비되지 않았습니다.");
            onAdClosed?.Invoke(); // 광고가 없어도 콜백 호출
            
            // 광고가 로드되지 않은 상태라면 다시 로드 시도
            if (!isLoadingInterstitialAd)
            {
                LoadInterstitialAd();
            }
        }
    }

    public void HandleInterstitialAdClosed()
    {
        // 다음 광고를 위해 새 광고 로드
        GameRoot.Instance.WaitTimeAndCallback(0.5f, LoadInterstitialAd);
    }

    public void LoadRewardedAd()
    {
        if (isLoadingRewardedAd) return; // 중복 로드 방지
        isLoadingRewardedAd = true;
        
        try
        {
            // 네트워크 상태 확인
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.LogWarning("리워드 광고 로드 실패: 네트워크에 연결되어 있지 않습니다.");
                isLoadingRewardedAd = false;
                
                // 네트워크 연결 없을 때 재시도 지연 시간 증가
                float retryDelay = 5f + (rewardedAdRetryCount * 2f);
                rewardedAdRetryCount++;
                
                GameRoot.Instance.WaitTimeAndCallback(retryDelay, LoadRewardedAd);
                return;
            }
            
            // 이전 광고 정리
            if (_rewardedAd != null)
            {
                try {
                    _rewardedAd.Destroy();
                }
                catch (Exception) {
                    // 광고 파괴 중 오류 무시
                }
                _rewardedAd = null;
            }

            Debug.Log("리워드 광고 로딩 시작");

            var adRequest = CreateAdRequest();
            
            // 타임아웃 처리를 위한 백업 타이머
            bool requestTimedOut = false;
            float timeoutDelay = 15f; // 15초 타임아웃
            
            System.Action timeoutAction = null;
            timeoutAction = () => {
                if (isLoadingRewardedAd && !IsRewardAdLoaded)
                {
                    requestTimedOut = true;
                    isLoadingRewardedAd = false;
                    Debug.LogWarning("리워드 광고 로드 타임아웃");
                    
                    // 타임아웃 후 재시도
                    float retryDelay = INITIAL_RETRY_DELAY * Mathf.Pow(1.5f, rewardedAdRetryCount);
                    rewardedAdRetryCount++;
                    GameRoot.Instance.WaitTimeAndCallback(retryDelay, LoadRewardedAd);
                }
            };
            
            // 타임아웃 타이머 설정
            GameRoot.Instance.WaitTimeAndCallback(timeoutDelay, timeoutAction);

            RewardedAd.Load(_rewardedAdUnitId, adRequest,
                (RewardedAd ad, LoadAdError error) =>
                {
                    // 이미 타임아웃된 요청인 경우 무시
                    if (requestTimedOut) return;
                    
                    isLoadingRewardedAd = false;
                    
                    if (error != null || ad == null)
                    {
                        rewardedAdRetryCount++;
                        float retryDelay = INITIAL_RETRY_DELAY * Mathf.Pow(2, rewardedAdRetryCount);
                        retryDelay = Mathf.Min(retryDelay, 60f); // 최대 60초까지 지연
                        
                        Debug.LogError($"리워드 광고 로드 실패 (시도 {rewardedAdRetryCount}/{MAX_RETRY_COUNT}): {error?.ToString()}");
                        
                        if (rewardedAdRetryCount < MAX_RETRY_COUNT)
                        {
                            Debug.Log($"{retryDelay}초 후 리워드 광고 다시 로드 시도");
                            GameRoot.Instance.WaitTimeAndCallback(retryDelay, LoadRewardedAd);
                        }
                        else
                        {
                            // 최대 재시도 횟수에 도달한 경우 15분 후 다시 시도
                            Debug.LogWarning("최대 재시도 횟수에 도달했습니다. 15분 후 다시 시도합니다.");
                            rewardedAdRetryCount = 0;
                            GameRoot.Instance.WaitTimeAndCallback(900f, LoadRewardedAd);
                        }
                        return;
                    }

                    rewardedAdRetryCount = 0; // 성공 시 재시도 카운트 초기화
                    _rewardedAd = ad;
                    IsRewardAdLoaded = true;
                    
                    Debug.Log("리워드 광고 로드 성공");

                    RegisterRewardedEventHandlers(ad);
                });
        }
        catch (Exception e)
        {
            isLoadingRewardedAd = false;
            Debug.LogError("리워드 광고 로드 중 예외 발생: " + e.Message);
            
            // 예외 발생 시 재시도 지연
            float retryDelay = 5f + (rewardedAdRetryCount * 3f);
            rewardedAdRetryCount++;
            
            GameRoot.Instance.WaitTimeAndCallback(retryDelay, LoadRewardedAd);
        }
    }

    // 리워드 광고 표시
    public void ShowRewardedAd(System.Action rewardAction, bool skipRewardIfNotReady = true)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("광고 SDK가 초기화되지 않았습니다.");
            if (skipRewardIfNotReady)
            {
                Debug.LogWarning("보상을 즉시 지급합니다.");
                rewardAction?.Invoke();
            }
            return;
        }
        
        if (IsRewardAdReady)
        {
            Debug.Log("리워드 광고 표시");
            
            // 보상 지급 핸들러 설정
            _rewardedAd.Show((Reward reward) => {
                Debug.Log($"사용자에게 보상이 지급되었습니다: {reward.Type}, {reward.Amount}");
                rewardAction?.Invoke();
            });
            
            IsRewardAdLoaded = false;
        }
        else
        {
            Debug.LogWarning("리워드 광고가 준비되지 않았습니다. 광고 로드 중...");
            
            // 광고가 준비되지 않은 경우의 처리
            if (skipRewardIfNotReady)
            {
                Debug.LogWarning("보상을 즉시 지급합니다.");
                rewardAction?.Invoke();
            }
            
            // 광고가 로드되지 않은 상태라면 다시 로드 시도
            if (!isLoadingRewardedAd)
            {
                LoadRewardedAd();
            }
        }
    }

    // 광고가 닫혔을 때
    public void HandleRewardedAdClosed()
    {
        Debug.Log("리워드 광고가 닫혔습니다.");
        
        // 다음 광고를 위해 새 광고 로드 (지연 로딩으로 변경)
        GameRoot.Instance.WaitTimeAndCallback(0.5f, LoadRewardedAd);
    }

    private void RegisterRewardedEventHandlers(RewardedAd ad)
    {
        // 광고가 닫혔을 때
        ad.OnAdFullScreenContentClosed += () => {
            Debug.Log("리워드 광고가 닫혔습니다.");
            IsRewardAdLoaded = false;
            HandleRewardedAdClosed();
        };
        
        // 광고 표시 실패 시
        ad.OnAdFullScreenContentFailed += (AdError error) => {
            Debug.LogError("리워드 광고 표시 실패: " + error);
            IsRewardAdLoaded = false;
            LoadRewardedAd(); // 실패 시 새 광고 로드
        };
        
        // 광고 수익이 발생했을 때
        ad.OnAdPaid += (AdValue adValue) => {
            Debug.Log($"리워드 광고 수익 발생: {adValue.Value} {adValue.CurrencyCode}");
            
            // 여기에 iOS와 안드로이드에 따른 분석 이벤트 전송 로직을 추가할 수 있습니다.
            // 예: Firebase Analytics 이벤트 전송
        };
        
        // 광고 노출이 기록되었을 때
        ad.OnAdImpressionRecorded += () => {
            Debug.Log("리워드 광고 노출이 기록되었습니다.");
        };
        
        // 광고 클릭이 기록되었을 때
        ad.OnAdClicked += () => {
            Debug.Log("리워드 광고가 클릭되었습니다.");
        };
    }
    
    // 앱이 일시 정지되거나 재개될 때 호출
    private void OnApplicationPause(bool pause)
    {
        if (!pause) // 앱이 포그라운드로 돌아올 때
        {
            // iOS에서는 백그라운드에서 포그라운드로 전환될 때 추가 처리가 필요할 수 있습니다.
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                Debug.Log("iOS 앱이 포그라운드로 돌아왔습니다. 광고 상태 확인 중...");
            }
            
            // 광고가 로드되지 않은 상태라면 새로 로드
            if (!IsRewardAdLoaded && !isLoadingRewardedAd)
            {
                GameRoot.Instance.WaitTimeAndCallback(1f, LoadRewardedAd);
            }
            
            if (!IsInterAdLoaded && !isLoadingInterstitialAd)
            {
                GameRoot.Instance.WaitTimeAndCallback(1f, LoadInterstitialAd);
            }
        }
    }
    
    // 스테이지 전환 중 광고 작업 일시 중지/재개
    public void PauseAdOperations(bool pause)
    {
        if (pause)
        {
            Debug.Log("광고 관련 작업 일시 중지 (스테이지 전환 중)");
            
            // 진행 중인 광고 로드 작업 표시 초기화
            isLoadingRewardedAd = false;
            isLoadingInterstitialAd = false;
            
            // 기존 광고 리소스 정리
            if (_rewardedAd != null)
            {
                try
                {
                    _rewardedAd.Destroy();
                    _rewardedAd = null;
                }
                catch (Exception e)
                {
                    Debug.LogWarning("리워드 광고 정리 중 오류: " + e.Message);
                }
            }
            
            if (_interstitialAd != null)
            {
                try
                {
                    _interstitialAd.Destroy();
                    _interstitialAd = null;
                }
                catch (Exception e)
                {
                    Debug.LogWarning("전면 광고 정리 중 오류: " + e.Message);
                }
            }
            
            IsRewardAdLoaded = false;
            IsInterAdLoaded = false;
        }
        else
        {
            Debug.Log("광고 관련 작업 재개");
            
            // 재시도 카운트 초기화
            rewardedAdRetryCount = 0;
            interstitialAdRetryCount = 0;
            
            // 광고 다시 로드
            GameRoot.Instance.WaitTimeAndCallback(1f, () => {
                if (!isLoadingRewardedAd && !IsRewardAdLoaded)
                {
                    LoadRewardedAd();
                }
                
                GameRoot.Instance.WaitTimeAndCallback(1.5f, () => {
                    if (!isLoadingInterstitialAd && !IsInterAdLoaded)
                    {
                        LoadInterstitialAd();
                    }
                });
            });
        }
    }

    // 리워드 광고 준비 상태를 외부에서 확인할 수 있는 프로퍼티
    public bool IsRewardAdReady
    {
        get { return isInitialized && IsRewardAdLoaded && _rewardedAd != null && _rewardedAd.CanShowAd(); }
    }
    
    // 전면 광고 준비 상태를 외부에서 확인할 수 있는 프로퍼티
    public bool IsInterstitialAdReady
    {
        get { return isInitialized && IsInterAdLoaded && _interstitialAd != null && _interstitialAd.CanShowAd(); }
    }

    // 광고 디버깅 정보 출력
    public void LogAdDebugInfo()
    {
        Debug.Log("=== 광고 디버깅 정보 ===");
        Debug.Log($"플랫폼: {Application.platform}");
        Debug.Log($"초기화 상태: {isInitialized}");
        Debug.Log($"VIP 상태: {GameRoot.Instance.ShopSystem.IsVipProperty.Value}");
        Debug.Log($"리워드 광고 ID: {_rewardedAdUnitId}");
        Debug.Log($"전면 광고 ID: {_interstitialAdUnitId}");
        Debug.Log($"리워드 광고 로드됨: {IsRewardAdLoaded}");
        Debug.Log($"전면 광고 로드됨: {IsInterAdLoaded}");
        Debug.Log($"네트워크 상태: {Application.internetReachability}");
        
        // ATT 상태 확인
        if (GameRoot.Instance.GetATTManager != null)
        {
            Debug.Log($"ATT 권한: {GameRoot.Instance.GetATTManager.GetTrackingStatusString()}");
        }
        
        Debug.Log("===================");
    }

    // VIP 상태 강제 해제 (테스트용)
    public void ForceDisableVIP()
    {
        GameRoot.Instance.ShopSystem.IsVipProperty.Value = false;
        Debug.Log("VIP 상태 강제 해제됨");
    }

    // 광고 강제 재로드
    public void ForceReloadAds()
    {
        Debug.Log("광고 강제 재로드 시작");
        
        // 기존 광고 정리
        if (_rewardedAd != null)
        {
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }
        
        if (_interstitialAd != null)
        {
            _interstitialAd.Destroy();
            _interstitialAd = null;
        }
        
        IsRewardAdLoaded = false;
        IsInterAdLoaded = false;
        isLoadingRewardedAd = false;
        isLoadingInterstitialAd = false;
        
        // 재시도 카운트 초기화
        rewardedAdRetryCount = 0;
        interstitialAdRetryCount = 0;
        
        // 광고 다시 로드
        LoadRewardedAd();
        GameRoot.Instance.WaitTimeAndCallback(1f, LoadInterstitialAd);
    }

    // ATT 권한 상태에 따른 AdRequest 생성
    private AdRequest CreateAdRequest()
    {
        var adRequest = new AdRequest();
        
        // iOS에서 ATT 권한이 거부된 경우 비개인화 광고 요청
        if (Application.platform == RuntimePlatform.IPhonePlayer && !attAuthorized)
        {
            Debug.Log("ATT 권한 거부됨: 비개인화 광고 요청");
            // Google Mobile Ads SDK에서는 ATT 권한 거부 시 자동으로 비개인화 광고 요청
            // 추가 설정이 필요한 경우 여기에 구현
        }
        
        return adRequest;
    }
}