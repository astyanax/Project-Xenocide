using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Text;

namespace Xenocide.Resources
{
    static class XenocideResourceManager
    {
        private static ResourceManager mResourceManager;

        static XenocideResourceManager()
        {
            mResourceManager = new ResourceManager("Xenocide.Resources.Strings", Assembly.GetExecutingAssembly());
        }

        public static string Get(string resourceID)
        {
            return mResourceManager.GetString(resourceID, CultureInfo.InvariantCulture);
        }
    }
}
