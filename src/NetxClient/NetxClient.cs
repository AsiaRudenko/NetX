﻿using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using ZYSocket;
using ZYSocket.Client;

namespace Netx.Client
{
    /// <summary>
    /// NetX 客户端
    /// </summary>
    public class NetxSClient : NetxSslSetter,IDisposable
    {
        public SocketClient SocketClient { get; private set; }

        public event DisconnectHandler Disconnect;

        internal NetxSClient(IServiceProvider container)
            : base(container)
        {
            Init();
        }

        private void Init()
        {
            SocketClient = Container.GetRequiredService<SocketClient>();
            SocketClient.BinaryInput += SocketClient_BinaryInput;
            SocketClient.Disconnect += SocketClient_Disconnect;
        }

        public void Open(int timeout = 6000)
        {
            var result = SocketClient.Connect(ConnectOption.Host, ConnectOption.Port, timeout);
            if (!result.IsSuccess)
            {
                throw new NetxException(result.Msg, ErrorType.ConnectErr);
            }            
        }

        public async Task<ConnectResult> OpenAsync(int timeout = 6000)
        {
            return await SocketClient.ConnectAsync(ConnectOption.Host, ConnectOption.Port, timeout);
        }

        public void Close()
        {
            IsConnect = false;
            SocketClient?.ShutdownBoth();
            SocketClient?.Dispose();
        }

        protected override bool ConnectIt()
        {
            Init();

            try
            {
                Open();
                return true;
            }
            catch (NetxException er)
            {
                Log.Error(this, er);
                return false;
            }
        }


        private void SocketClient_Disconnect(ISocketClient client, ISockAsyncEventAsClient socketAsync, string msg)
        {
            Log.Info($"{ConnectOption.Host}:{ConnectOption.Port}->{msg}");
            Close();
            Disconnect?.Invoke(client, socketAsync, msg);
        }



        private async void SocketClient_BinaryInput(ISocketClient client, ISockAsyncEventAsClient socketAsync)
        {
            var fiberRw = await GetFiberRw(socketAsync);

            if (fiberRw == null)
            {
                client.SetConnected(false, "ssl error");
                return;
            }

            IWrite = fiberRw;

            if (!isConnect)
            {

                await SendVerify(); //发送KEY和sessionid验证
                await fiberRw.ReadInt32();//丢弃长度,因为这个Socket框架不需要,留给C++ go java等语言和其他SOCKET框架用
                var cmd = await fiberRw.ReadInt32();

                if (cmd.HasValue)
                {
                    switch (cmd)
                    {
                        case 1000: //key check
                            {
                                var iserror = await fiberRw.ReadBoolean();

                                if (iserror.HasValue)
                                {
                                    if (!iserror.Value)
                                    {
                                        Log.Trace(await fiberRw.ReadString());
                                        isConnect = true;
                                        client.SetConnected();
                                        await ReadIng(fiberRw);
                                    }
                                    else
                                    {
                                        var msg = await fiberRw.ReadString();
                                        Log.Info(msg);
                                        client.SetConnected(false, msg);
                                    }
                                }
                                else
                                    client.SetConnected(false, "data error");
                            }
                            break;

                    }

                }
                else
                    client.SetConnected(false, "key error");

            }
            
            client.ShutdownBoth(true);
        }

        public void Dispose()
        {
            Close();          
        }
    }
}