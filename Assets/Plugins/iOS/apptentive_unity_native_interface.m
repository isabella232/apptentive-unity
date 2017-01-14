//
//  apptentive_unity_native_interface.c
//  Unity-iPhone
//
//  Created by Alex Lementuev on 1/13/17.
//
//

#import "apptentive_unity_native_interface.h"

#import "ApptentiveUnityPlugin.h"

static ApptentiveUnityPlugin * _instance;

void __apptentive_initialize(const char *targetNameStr, const char *methodNameStr, const char *versionStr, const char *APIKeyStr)
{
    dispatch_async(dispatch_get_main_queue(), ^{
        NSString *targetName = [[NSString alloc] initWithUTF8String:targetNameStr];
        NSString *methodName = [[NSString alloc] initWithUTF8String:methodNameStr];
        NSString *version = [[NSString alloc] initWithUTF8String:versionStr];
        NSString *APIKey = [[NSString alloc] initWithUTF8String:APIKeyStr];
        _instance = [[ApptentiveUnityPlugin alloc] initWithTargetName:targetName
                                                           methodName:methodName
                                                              version:version
                                                               APIKey:APIKey];
    });
}

BOOL __apptentive_engage(const char *eventName, const char *customData)
{
    return YES;
}

void __apptentive_destroy()
{
    dispatch_async(dispatch_get_main_queue(), ^{
        _instance = nil;
    });
}
