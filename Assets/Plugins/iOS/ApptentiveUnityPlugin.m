//
//  UnityApptentivePlugin.m
//  Unity-iPhone
//
//  Created by Alex Lementuev on 1/13/17.
//
//

#import "ApptentiveUnityPlugin.h"
#import "ApptentiveScriptMessenger.h"

@interface ApptentiveUnityPlugin ()
{
    ApptentiveScriptMessenger * _scriptMessenger;
}

@end

@implementation ApptentiveUnityPlugin

- (instancetype)initWithTargetName:(NSString *)targetName
                        methodName:(NSString *)methodName
                           version:(NSString *)version
                            APIKey:(NSString *)apiKey
{
    self = [super init];
    if (self)
    {
        _scriptMessenger = [[ApptentiveScriptMessenger alloc] initWithTargetName:targetName methodName:methodName];
    }
    return self;
}

@end
