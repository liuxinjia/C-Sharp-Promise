﻿using System;
using System.Collections.Generic;
using System.Text;

namespace RSG
{

    // similar as IValueTaskSource
    public interface IPromiseTaskSource
    {
        PromiseTaskStatus GetStatus(short token);
        void GetResult(short token);
#if DEBUG
        PromiseTaskStatus UnsafeGetStatus(); // only for debug use.
#endif
        void OnCompleted(Action continuation, short token);

    }


    public interface IPromiseTaskSource<out T> : IPromiseTaskSource
    {
        new T GetResult(short token);
    }

}
