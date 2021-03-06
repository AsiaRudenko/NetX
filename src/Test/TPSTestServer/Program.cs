﻿using Microsoft.Extensions.DependencyInjection;
using Netx.Actor;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
namespace TestServer
{
    class Program
    {
        static X509Certificate certificate = new X509Certificate2(Environment.CurrentDirectory + "/server.pfx", "testPassword");

        static void Main()
        {
            var service = new Netx.Service.Builder.NetxServBuilder()
                .RegisterService(Assembly.GetExecutingAssembly())              
                //.ConfigSSL(p =>
                // {
                //     p.IsUse = true;
                //     p.Certificate = certificate;
                // })              
                .ConfigNetWork(p =>
                {                   
                    p.MaxConnectCout = 100;
                    p.Port = 1005;
                    p.MaxPackerSize = 256 * 1024;
                })
                .ConfigBase(p=>
                {                    
                    p.VerifyKey = "123123";
                    p.ClearSessionTime = 5000;
                })       
               // .ConfigureActorScheduler(p=>ActorScheduler.TaskFactory)                  
                .Build();

            service.Start();

            Console.ReadLine();
        }
    }
}
