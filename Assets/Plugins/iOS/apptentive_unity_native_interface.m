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
    // FIXME: thread safety
    NSString *targetName = [[NSString alloc] initWithUTF8String:targetNameStr];
    NSString *methodName = [[NSString alloc] initWithUTF8String:methodNameStr];
    NSString *version = [[NSString alloc] initWithUTF8String:versionStr];
    NSString *APIKey = [[NSString alloc] initWithUTF8String:APIKeyStr];
    _instance = [[ApptentiveUnityPlugin alloc] initWithTargetName:targetName
                                                       methodName:methodName
                                                          version:version
                                                           APIKey:APIKey];
}

void __apptentive_destroy()
{
    // FIXME: thread safety
    _instance = nil;
}

BOOL __apptentive_engage(const char *eventNameStr, const char *customDataStr)
{
    // FIXME: thread safety and custom data
    NSString *eventName = [[NSString alloc] initWithUTF8String:eventNameStr];
    return [_instance engage:eventName withCustomData:nil withExtendedData:nil];
}

BOOL __apptentive_present_message_center(const char *customData)
{
    // FIXME: thread safety and custom data
    return [_instance presentMessageCenterWithCustomData:nil];
}

BOOL __apptentive_can_show_interaction(const char *eventNameStr)
{
    // FIXME: thread safety
    NSString *eventName = [[NSString alloc] initWithUTF8String:eventNameStr];
    return [_instance canShowInteractionForEvent:eventName];
}
