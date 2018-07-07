//
//  UnityApptentivePlugin.m
//  Unity-iPhone
//
//  Created by Alex Lementuev on 1/13/17.
//
//

#import "Apptentive.h"

#import "ApptentiveUnityPlugin.h"
#import "ApptentiveScriptMessenger.h"

#import "UnityAppController.h"

static NSUInteger _nextCallbackId;

@interface ApptentiveBooleanIdCallback : NSObject

@property (nonatomic, readonly) NSUInteger identifier;
@property (nonatomic, readonly) NSString *name;
@property (nonatomic, readonly, copy) void (^callbackBlock)(BOOL);

+ (instancetype)callbackWithName:(NSString *)name;
- (instancetype)initWithName:(NSString *)name;

@end

@interface ApptentiveUnityPlugin ()
{
    ApptentiveScriptMessenger * _scriptMessenger;
}

@end

@implementation ApptentiveUnityPlugin

- (instancetype)initWithTargetName:(NSString *)targetName
						methodName:(NSString *)methodName
						   version:(NSString *)version
					 configuration:(ApptentiveConfiguration *)configuration
{
    self = [super init];
    if (self)
    {
		_scriptMessenger = [[ApptentiveScriptMessenger alloc] initWithTargetName:targetName methodName:methodName];
		
		[Apptentive registerWithConfiguration:configuration];
    }
    return self;
}

#pragma mark -
#pragma mark Public interface

- (NSUInteger)presentMessageCenterWithCustomData:(NSDictionary *)customData
{
	NSUInteger callbackId = [self nextCallbackId];
	
	__weak ApptentiveUnityPlugin *weakSelf = self;
	[Apptentive.shared presentMessageCenterFromViewController:self.rootViewController
											   withCustomData:customData
												   completion:^(BOOL presented) {
		[weakSelf sendNativeCallbackId:callbackId name:@""];
	}];
	
	return callbackId;
}

- (NSUInteger)canShowInteractionForEvent:(NSString *)event
{
    return [[Apptentive sharedConnection] canShowInteractionForEvent:event];
}

- (NSUInteger)engage:(NSString *)event withCustomData:(NSDictionary *)customData withExtendedData:(NSArray<NSDictionary *> *)extendedData
{
	NSUInteger callbackId = [self nextCallbackId];
	
	[Apptentive.shared engage:event withCustomData:customData
			 withExtendedData:extendedData
		   fromViewController:self.rootViewController
				   completion:^(BOOL engaged) {
		   // TODO: send callback id
	}];
	
	return callbackId;
}

#pragma mark -
#pragma mark Native Messages

- (NSUInteger)nextCallbackId
{
	@synchronized (self)
	{
		return ++_nextCallbackId;
	}
}

- (void)sendNativeCallbackId:(NSUInteger)callbackId name:(NSString *)name
{
}

#pragma mark -
#pragma mark Properties

- (BOOL)canShowMessageCenter
{
    return [Apptentive sharedConnection].canShowMessageCenter;
}

- (NSUInteger)unreadMessageCount
{
    return [Apptentive sharedConnection].unreadMessageCount;
}

- (UIViewController *)rootViewController
{
    return GetAppController().rootViewController;
}

@end

static NSUInteger _nextCallbackId;

@implementation ApptentiveBooleanIdCallback

+ (instancetype)callbackWithName:(NSString *)name
{
	return [[[self class] alloc] initWithName:name];
}

- (instancetype)initWithName:(NSString *)name
{
	self = [super init];
	if (self)
	{
		_identifier = ++_nextCallbackId; // TODO:
		_name = name;
	}
	return self;
}

@end
