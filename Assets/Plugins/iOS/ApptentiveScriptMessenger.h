//
//  ApptentiveScriptMessenger.h
//  Unity-iPhone
//
//  Created by Alex Lementuev on 1/13/17.
//
//

#import <Foundation/Foundation.h>

@interface ApptentiveScriptMessenger : NSObject

- (instancetype)initWithTargetName:(NSString *)targetName methodName:(NSString *)methodName;

- (void)sendMessageName:(NSString *)name;
- (void)sendMessageName:(NSString *)name params:(NSDictionary *)params;

@end
