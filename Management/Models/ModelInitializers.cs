/*!
* DisplayMonkey source file
* http://displaymonkey.org
*
* Copyright (c) 2015 Fuel9 LLC and contributors
*
* Released under the MIT license:
* http://opensource.org/licenses/MIT
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DisplayMonkey.Models
{
    public partial class Panel
    {
        public void init(DisplayMonkeyEntities _db)
        {
            Setting fadeLength = Setting.GetSetting(_db, Setting.Keys.DefaultPanelFadeLength);
            if (fadeLength != null)
            {
                this.FadeLength = fadeLength.DecimalValue;
            }
        }
    }

    public partial class FullScreen
    {
        public void init(DisplayMonkeyEntities _db)
        {
            Setting fadeLength = Setting.GetSetting(_db, Setting.Keys.DefaultFullPanelFadeLength);
            if (fadeLength != null)
            {
                this.Panel.FadeLength = fadeLength.DecimalValue;
            }
        }
    }

    public partial class Display
    {
        public void init(DisplayMonkeyEntities _db)
        {
            Setting readyTimeout = Setting.GetSetting(_db, Setting.Keys.DefaultDisplayReadyEventTimeout);
            if (readyTimeout != null)
            {
                this.ReadyTimeout = readyTimeout.IntValuePositive;
            }

            Setting errorLength = Setting.GetSetting(_db, Setting.Keys.DefaultDisplayErrorLength);
            if (errorLength != null)
            {
                this.ErrorLength = errorLength.IntValuePositive;
            }

            Setting pollInterval = Setting.GetSetting(_db, Setting.Keys.DefaultDisplayPollInterval);
            if (pollInterval != null)
            {
                this.PollInterval = pollInterval.IntValuePositive;
            }
        }
    }
}
