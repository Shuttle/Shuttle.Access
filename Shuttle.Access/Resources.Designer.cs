﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Shuttle.Access {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Shuttle.Access.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The &apos;Administrator&apos; role could not be found..
        /// </summary>
        public static string AdministratorRoleMissingException {
            get {
                return ResourceManager.GetString("AdministratorRoleMissingException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not find a configuration section with name &quot;accessClient&quot; or &quot;shuttle/accessClient&quot;..
        /// </summary>
        public static string ClientSectionException {
            get {
                return ResourceManager.GetString("ClientSectionException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not find a connection string with name &apos;{0}&apos;..
        /// </summary>
        public static string ConnectionStringMissing {
            get {
                return ResourceManager.GetString("ConnectionStringMissing", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not delete the session..
        /// </summary>
        public static string DeleteSessionException {
            get {
                return ResourceManager.GetString("DeleteSessionException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Permission with Id &apos;{0}&apos; already exists on role &apos;{1}&apos;..
        /// </summary>
        public static string DuplicatePermissionException {
            get {
                return ResourceManager.GetString("DuplicatePermissionException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Identity with name &apos;{0}&apos; is inactive..
        /// </summary>
        public static string IdentityInactiveException {
            get {
                return ResourceManager.GetString("IdentityInactiveException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid credentials..
        /// </summary>
        public static string InvalidCredentialsException {
            get {
                return ResourceManager.GetString("InvalidCredentialsException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The &apos;PasswordHash&apos; may not be empty..
        /// </summary>
        public static string PasswordHashException {
            get {
                return ResourceManager.GetString("PasswordHashException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Permission with Id &apos;{0}&apos; does not exist on role &apos;{1}&apos;..
        /// </summary>
        public static string PermissionNotFoundException {
            get {
                return ResourceManager.GetString("PermissionNotFoundException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Property &apos;{0}&apos; has not changed.  Cannot apply value of &apos;{1}&apos;..
        /// </summary>
        public static string PropertyUnchangedException {
            get {
                return ResourceManager.GetString("PropertyUnchangedException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There is already a password reset token registered..
        /// </summary>
        public static string RegisterPasswordResetTokenException {
            get {
                return ResourceManager.GetString("RegisterPasswordResetTokenException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not register the session..
        /// </summary>
        public static string RegisterSessionException {
            get {
                return ResourceManager.GetString("RegisterSessionException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Role &apos;{0}&apos; does not exist on user &apos;{1}&apos;..
        /// </summary>
        public static string RoleNotFoundException {
            get {
                return ResourceManager.GetString("RoleNotFoundException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The session has expired for identity &apos;{0}&apos;..
        /// </summary>
        public static string SessionRegisterExpired {
            get {
                return ResourceManager.GetString("SessionRegisterExpired", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not authenticate identity &apos;{0}&apos;..
        /// </summary>
        public static string SessionRegisterFailure {
            get {
                return ResourceManager.GetString("SessionRegisterFailure", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Attemping to register with password for identity &apos;{0}&apos;..
        /// </summary>
        public static string SessionRegisterIdentity {
            get {
                return ResourceManager.GetString("SessionRegisterIdentity", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid token specified for identity &apos;{0}&apos;..
        /// </summary>
        public static string SessionRegisterInvalidToken {
            get {
                return ResourceManager.GetString("SessionRegisterInvalidToken", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The session has been renewed for identity &apos;{0}&apos;..
        /// </summary>
        public static string SessionRegisterRenewed {
            get {
                return ResourceManager.GetString("SessionRegisterRenewed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Session with token &apos;{0}&apos; does not have the required permission &apos;{1}&apos;..
        /// </summary>
        public static string SessionRegisterRequestDenied {
            get {
                return ResourceManager.GetString("SessionRegisterRequestDenied", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The session has expired for identity &apos;{0}&apos;..
        /// </summary>
        public static string SessionRegisterRequestExpired {
            get {
                return ResourceManager.GetString("SessionRegisterRequestExpired", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid token &apos;{0}&apos; specified..
        /// </summary>
        public static string SessionRegisterRequestInvalidToken {
            get {
                return ResourceManager.GetString("SessionRegisterRequestInvalidToken", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The session has been renewed for identity &apos;{0}&apos;..
        /// </summary>
        public static string SessionRegisterRequestRenewed {
            get {
                return ResourceManager.GetString("SessionRegisterRequestRenewed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The session has been created for identity &apos;{0}&apos;..
        /// </summary>
        public static string SessionRegisterSuccess {
            get {
                return ResourceManager.GetString("SessionRegisterSuccess", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Attemping to register with token for identity &apos;{0}&apos;..
        /// </summary>
        public static string SessionRegisterToken {
            get {
                return ResourceManager.GetString("SessionRegisterToken", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No token could be retrieved to fulfill the request..
        /// </summary>
        public static string SessionTokenException {
            get {
                return ResourceManager.GetString("SessionTokenException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The session token has expired.  Please log in again..
        /// </summary>
        public static string SessionTokenExpiredException {
            get {
                return ResourceManager.GetString("SessionTokenExpiredException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not find an identity with name &apos;{0}&apos;..
        /// </summary>
        public static string UnknownIdentityException {
            get {
                return ResourceManager.GetString("UnknownIdentityException", resourceCulture);
            }
        }
    }
}
