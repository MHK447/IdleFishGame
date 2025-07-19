#import <Foundation/Foundation.h>

// iOS 14 이상에서만 ATT 프레임워크 import
#if defined(__IPHONE_14_0) && __IPHONE_OS_VERSION_MAX_ALLOWED >= __IPHONE_14_0
#import <AppTrackingTransparency/AppTrackingTransparency.h>
#import <AdSupport/AdSupport.h>
#endif

// 약한 링킹을 위한 정의
#ifndef ATTrackingManagerAuthorizationStatusNotDetermined
    #define ATTrackingManagerAuthorizationStatusNotDetermined 0
    #define ATTrackingManagerAuthorizationStatusRestricted 1
    #define ATTrackingManagerAuthorizationStatusDenied 2
    #define ATTrackingManagerAuthorizationStatusAuthorized 3
#endif

extern "C" {
    
    // ATT 권한 상태 확인
    int ATTrackingManagerGetTrackingAuthorizationStatus() {
#if defined(__IPHONE_14_0) && __IPHONE_OS_VERSION_MAX_ALLOWED >= __IPHONE_14_0
        if (@available(iOS 14, *)) {
            return (int)[ATTrackingManager trackingAuthorizationStatus];
        }
#endif
        return 3; // iOS 14 미만에서는 허용으로 처리
    }
    
    // ATT 권한 요청
    void ATTrackingManagerRequestTrackingAuthorization() {
#if defined(__IPHONE_14_0) && __IPHONE_OS_VERSION_MAX_ALLOWED >= __IPHONE_14_0
        if (@available(iOS 14, *)) {
            [ATTrackingManager requestTrackingAuthorizationWithCompletionHandler:^(ATTrackingManagerAuthorizationStatus status) {
                // 콜백은 Unity에서 별도로 처리
                NSLog(@"ATT 권한 응답: %lu", (unsigned long)status);
            }];
        }
#endif
    }
    
    // IDFA 값 가져오기 (선택사항)
    const char* GetIDFA() {
#if defined(__IPHONE_14_0) && __IPHONE_OS_VERSION_MAX_ALLOWED >= __IPHONE_14_0
        if (@available(iOS 14, *)) {
            if ([ATTrackingManager trackingAuthorizationStatus] == ATTrackingManagerAuthorizationStatusAuthorized) {
                NSUUID* idfa = [[ASIdentifierManager sharedManager] advertisingIdentifier];
                NSString* idfaString = [idfa UUIDString];
                return strdup([idfaString UTF8String]);
            }
        }
#endif
        return strdup("00000000-0000-0000-0000-000000000000");
    }
    
    // iOS 버전 확인
    const char* _GetSystemVersion() {
        NSString* version = [[UIDevice currentDevice] systemVersion];
        return strdup([version UTF8String]);
    }
} 