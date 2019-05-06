﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Netx.Interface;

namespace Netx
{
    /// <summary>
    /// 默认Id生成类
    /// </summary>
    public class DefaultMakeIds : IIds
    {
        /// <summary>
        /// 种子
        /// </summary>
        private long Id;

        /// <summary>
        /// 生成一个新的Id
        /// </summary>
        public long MakeId => Interlocked.Increment(ref Id);

        public DefaultMakeIds()
        {
            //Id =  DateTime.Now.Ticks;         
        }       


    }
}
