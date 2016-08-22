/*!
* DisplayMonkey source file
* http://displaymonkey.org
*
* Copyright (c) 2015 Fuel9 LLC and contributors
*
* Released under the MIT license:
* http://opensource.org/licenses/MIT
*/

namespace DisplayMonkey.Models
{
    using System;

    public enum FrameTypes : int
    {
        //Unknown = -1,
        Clock = 0,
        Html = 1,
        Memo = 2,
        //News = 3, reserved
        Outlook = 4,
        Picture = 5,
        Report = 6,
        Video = 7,
        Weather = 8,
        YouTube = 9,
        Powerbi = 10
    }
}
