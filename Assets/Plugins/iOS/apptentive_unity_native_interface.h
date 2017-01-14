//
//  apptentive_unity_native_interface.h
//  Unity-iPhone
//
//  Created by Alex Lementuev on 1/13/17.
//
//

#ifndef apptentive_unity_native_interface_h
#define apptentive_unity_native_interface_h

// life cycle
OBJC_EXTERN void __apptentive_initialize(const char *targetName, const char *methodName, const char *version, const char *APIKey);

OBJC_EXTERN void __apptentive_destroy(void);

// engagements
OBJC_EXTERN BOOL __apptentive_engage(const char *eventName, const char *customData);

OBJC_EXTERN BOOL __apptentive_present_message_center(const char *customData);

OBJC_EXTERN BOOL __apptentive_can_show_interaction(const char *eventName);

OBJC_EXTERN BOOL __apptentive_can_show_message_center(void);

#endif /* apptentive_unity_native_interface_h */
