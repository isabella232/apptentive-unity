//
//  UnityApptentivePlugin.h
//  Unity-iPhone
//
//  Created by Alex Lementuev on 1/13/17.
//
//

#import <Foundation/Foundation.h>

@interface ApptentiveUnityPlugin : NSObject

@property (readonly, nonatomic) BOOL canShowMessageCenter;
@property (readonly, nonatomic) NSUInteger unreadMessageCount;

- (instancetype)initWithTargetName:(NSString *)targetName
                        methodName:(NSString *)methodName
                           version:(NSString *)version
                            APIKey:(NSString *)APIKey;

- (BOOL)presentMessageCenterWithCustomData:(NSDictionary *)customData;
- (BOOL)canShowInteractionForEvent:(NSString *)event;
- (BOOL)engage:(NSString *)event withCustomData:(NSDictionary *)customData withExtendedData:(NSArray<NSDictionary *> *)extendedData;

@end
