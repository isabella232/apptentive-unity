//
//  UnityApptentivePlugin.h
//  Unity-iPhone
//
//  Created by Alex Lementuev on 1/13/17.
//
//

#import <Foundation/Foundation.h>

@interface ApptentiveUnityPlugin : NSObject

- (instancetype)initWithTargetName:(NSString *)targetName
                        methodName:(NSString *)methodName
                           version:(NSString *)version
                            APIKey:(NSString *)apiKey;

@end
