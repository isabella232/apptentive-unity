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

#endif /* apptentive_unity_native_interface_h */
