﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIRI.Serialization
{
    public interface IResults
    {
        long TotalResults { get; set; }
        int StartIndex { get; set; }
        int ItemsPerPage { get; set; }
        IResultItem[] Items { get; set; }
    }
}
