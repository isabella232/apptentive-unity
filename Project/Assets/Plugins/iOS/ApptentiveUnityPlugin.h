//
//  UnityApptentivePlugin.h
//  Unity-iPhone
//
//  Created by Alex Lementuev on 1/13/17.
//
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@class ApptentiveConfiguration;

@interface ApptentiveUnityPlugin : NSObject

@property (readonly, nonatomic) BOOL canShowMessageCenter;
@property (readonly, nonatomic) NSUInteger unreadMessageCount;

- (instancetype)initWithTargetName:(NSString *)targetName
                        methodName:(NSString *)methodName
                           version:(NSString *)version
					 configuration:(ApptentiveConfiguration *)configuration;

- (NSUInteger)presentMessageCenterWithCustomData:(NSDictionary *)customData;
- (NSUInteger)canShowInteractionForEvent:(NSString *)event;
- (NSUInteger)engage:(NSString *)event withCustomData:(NSDictionary *)customData withExtendedData:(NSArray<NSDictionary *> *)extendedData;

@end

NS_ASSUME_NONNULL_END
