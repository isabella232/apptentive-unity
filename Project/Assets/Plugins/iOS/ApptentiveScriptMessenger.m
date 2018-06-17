//
//  ApptentiveScriptMessenger.m
//  Unity-iPhone
//
//  Created by Alex Lementuev on 1/13/17.
//
//

#import "ApptentiveScriptMessenger.h"

extern void UnitySendMessage(const char *objectName, const char *methodName, const char *message);

static NSString *serializeToString(NSDictionary *data)
{
    if (data.count == 0) return @"";
    
    NSMutableString *result = [NSMutableString string];
    NSInteger index = 0;
    for (id key in data)
    {
        id value = [[data objectForKey:key] description];
        value = [value stringByReplacingOccurrencesOfString:@"\n" withString:@"\\n"]; // we use new lines as separators
        [result appendFormat:@"%@:%@", key, value];
        if (++index < data.count)
        {
            [result appendString:@"\n"];
        }
    }
    
    return result;
}

@interface ApptentiveScriptMessenger ()
{
    NSString * _targetName;
    NSString * _methodName;
}

@end

@implementation ApptentiveScriptMessenger

- (instancetype)initWithTargetName:(NSString *)targetName methodName:(NSString *)methodName
{
    self = [super init];
    if (self)
    {
        if (targetName.length == 0)
        {
            NSLog(@"Can't create script messenger: target name is nil or empty");
            self = nil;
            return nil;
        }
        
        if (methodName.length == 0)
        {
            NSLog(@"Can't create script messenger: method name is nil or empty");
            self = nil;
            return nil;
        }
        
        _targetName = [targetName copy];
        _methodName = [methodName copy];
    }
    return self;
}


- (void)sendMessageName:(NSString *)name
{
    [self sendMessageName:name params:nil];
}

- (void)sendMessageName:(NSString *)name params:(NSDictionary *)params
{
    NSMutableDictionary *dict = [NSMutableDictionary dictionaryWithObjectsAndKeys:name, @"name", nil];
    if (params.count > 0)
    {
        [dict addEntriesFromDictionary:params];
    }
    
    NSString *paramString = serializeToString(dict);
    UnitySendMessage(_targetName.UTF8String, _methodName.UTF8String, paramString.UTF8String);
}

@end
