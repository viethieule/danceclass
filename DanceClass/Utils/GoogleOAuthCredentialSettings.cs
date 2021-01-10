using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace DanceClass.Utils
{
    public class GoogleOAuthCredentialSettings : ConfigurationSection
    {
        public GoogleOAuthCredentialSettings()
        {
        }

        [ConfigurationProperty("clientId")]
        public string ClientId
        {
            get { return ((string)this["clientId"]); }
        }

        [ConfigurationProperty("clientSecret")]
        public string ClientSecret
        {
            get { return ((string)this["clientSecret"]); }
        }
    }
}