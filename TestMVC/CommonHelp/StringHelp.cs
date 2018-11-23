﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TestMVC.CommonHelp
{
    public class StringHelp
    {
        public const string defaultPicture =
            "data:image/png;base64,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";
    }
}