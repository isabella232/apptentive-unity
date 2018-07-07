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

@interface ApptentiveBooleanIdCallback : NSObject

@property (nonatomic, readonly) NSUInteger identifier;

@end

@interface ApptentiveUnityPlugin () <ApptentiveDelegate>
{
    ApptentiveScriptMessenger * _scriptMessenger;
}

@end

@implementation ApptentiveUnityPlugin

- (instancetype)initWithTargetName:(NSString *)targetName
                        methodName:(NSString *)methodName
                           version:(NSString *)version
                            APIKey:(NSString *)APIKey
{
    self = [super init];
    if (self)
    {
        [Apptentive sharedConnection].APIKey = APIKey;
        [Apptentive sharedConnection].delegate = self;
        
        _scriptMessenger = [[ApptentiveScriptMessenger alloc] initWithTargetName:targetName methodName:methodName];
    }
    return self;
}

#pragma mark -
#pragma mark Public interface

- (BOOL)presentMessageCenterWithCustomData:(NSDictionary *)customData
{
    return [[Apptentive sharedConnection] presentMessageCenterFromViewController:self.rootViewController withCustomData:customData];
}

- (BOOL)canShowInteractionForEvent:(NSString *)event
{
    return [[Apptentive sharedConnection] canShowInteractionForEvent:event];
}

- (BOOL)engage:(NSString *)event withCustomData:(NSDictionary *)customData withExtendedData:(NSArray<NSDictionary *> *)extendedData
{
    return [[Apptentive sharedConnection] engage:event
                                  withCustomData:customData
                                withExtendedData:extendedData
                              fromViewController:self.rootViewController];
}

#pragma mark -
#pragma mark ApptentiveDelegate

- (UIViewController *)viewControllerForInteractionsWithConnection:(Apptentive *)connection
{
    return self.rootViewController;
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

- (instancetype)init
{
	self = [super init];
	if (self)
	{
		_identifier = ++_nextCallbackId;
	}
	return this;
}

@end
