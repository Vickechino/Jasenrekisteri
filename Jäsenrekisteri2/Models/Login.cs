//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Jäsenrekisteri2.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Login
    {
        public int member_id { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public Nullable<System.DateTime> lastseen { get; set; }
        public Nullable<int> admin { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public Nullable<System.DateTime> joinDate { get; set; }
        public string fullname { get; set; }
        public Nullable<bool> emailVerified { get; set; }
        public Nullable<int> verificationCode { get; set; }
        public Nullable<System.DateTime> verificationEmailSent { get; set; }
        public string LoginMessage { get; set; }
    }
}
