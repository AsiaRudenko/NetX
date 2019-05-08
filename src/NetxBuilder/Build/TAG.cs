﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Netx
{
    /// <summary>
    /// 用于设置函数的命令
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class TAG : Attribute
    {
        public int CmdTag { get; set; }

        /// <summary>
        /// 数据包格式化类
        /// </summary>
        /// <param name="cmdTag">数据包命令类型</param>
        public TAG(int cmdTag)
        {
            this.CmdTag = cmdTag;
        }
    }
}