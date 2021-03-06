﻿using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
namespace Netx.Service
{
    public abstract class ServiceInstall : ServiceBase
    {
        /// <summary>
        /// 命令方法表
        /// </summary>
        protected ConcurrentDictionary<int, MethodRegister> AsyncServicesRegisterDict { get; }


        public ServiceInstall(IServiceProvider container) : base(container)
        {
            AsyncServicesRegisterDict = container.GetRequiredService<ConcurrentDictionary<int, MethodRegister>>();
        }
      

    }
}
