//
//  apptentive_unity_native_interface.c
//  Unity-iPhone
//
//  Created by Alex Lementuev on 1/13/17.
//
//

#import "apptentive_unity_native_interface.h"

#import "Apptentive.h"
#import "ApptentiveUnityPlugin.h"

static ApptentiveUnityPlugin * _instance;

static BOOL _parseBoolean(NSString *value)
{
	return value != nil && [value isEqualToString:@"true"];
}

static ApptentiveLogLevel _parseApptentiveLogLevel(NSString *value)
{
	if (value)
	{
		if ([value isEqualToString:@"Verbose"])
			return ApptentiveLogLevelVerbose;
		
		if ([value isEqualToString:@"Debug"])
			return ApptentiveLogLevelDebug;
		
		if ([value isEqualToString:@"Warn"])
			return ApptentiveLogLevelWarn;
		
		if ([value isEqualToString:@"Error"])
			return ApptentiveLogLevelError;
	}
	
	return ApptentiveLogLevelInfo;
}

static ApptentiveConfiguration * _Nullable _parseApptentiveConfiguration(NSString *jsonString)
{
	NSData *data = [jsonString dataUsingEncoding:NSUTF8StringEncoding];
	if (data == nil)
	{
		NSLog(@"Unable to create data from UTF-string: '%@'", jsonString);
		return nil;
	}
	
	NSError *error;
	id object = [NSJSONSerialization JSONObjectWithData:data options:0 error:&error];
	if (error)
	{
		NSLog(@"Unable to parse JSON data: '%@'\n%@", jsonString, error);
		return nil;
	}
	
	if (![object isKindOfClass:[NSDictionary class]])
	{
		NSLog(@"Unexpected JSON object class: %@", [object class]);
		return nil;
	}
	
	NSDictionary *dict = object;
	
	NSString *apptentiveKey = dict[@"apptentiveKey"];
	NSString *apptentiveSignature = dict[@"apptentiveSignature"];
	ApptentiveLogLevel logLevel = _parseApptentiveLogLevel(dict[@"logLevel"]);
	BOOL shouldSanitizeLogMessages = _parseBoolean(dict[@"shouldSanitizeLogMessages"]);
	
	ApptentiveConfiguration *configuration = [ApptentiveConfiguration configurationWithApptentiveKey:apptentiveKey apptentiveSignature:apptentiveSignature];
	configuration.logLevel = logLevel;
	configuration.shouldSanitizeLogMessages = shouldSanitizeLogMessages;
	
	return configuration;
}

void __apptentive_initialize(const char *targetNameStr, const char *methodNameStr, const char *versionStr, const char *configurationJsonStr)
{
	NSString *configurationJson = [[NSString alloc] initWithUTF8String:configurationJsonStr];
	ApptentiveConfiguration *configuration = _parseApptentiveConfiguration(configurationJson);
	if (configuration == nil)
	{
		return;
	}
	
    // FIXME: thread safety
    NSString *targetName = [[NSString alloc] initWithUTF8String:targetNameStr];
    NSString *methodName = [[NSString alloc] initWithUTF8String:methodNameStr];
    NSString *version = [[NSString alloc] initWithUTF8String:versionStr];
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

BOOL __apptentive_can_show_message_center()
{
    return _instance.canShowMessageCenter;
}
