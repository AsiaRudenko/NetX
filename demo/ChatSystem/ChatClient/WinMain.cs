﻿using Netx;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChatTag;

namespace ChatClient
{
    public partial class WinMain : Form,IMethodController
    {
        public INetxSClient Current { get => Dependency.Client; set { } }

        public T Get<T>()
        {
            return Current.Get<T>();
        }

        public WinMain()
        {
            InitializeComponent();
        }

        private async void WinMain_Load(object sender, EventArgs e)
        {           
            await LoginServer();
            await LoadingUserList();
            await GetLGetLeaving();
            Dependency.Client.LoadInstance(this);
        }


        private async Task GetLGetLeaving()
        {
            foreach (var item in await Get<IServer>().GetLeavingMessage())
            {
                SayMessage(item.FromUserId, item.NickName, item.MsgType, item.MessageContext,item.Time);
            } 
        }

        private async Task LoadingUserList()
        {
            try
            {
                var userlist = await Get<IServer>().GetUsers();

                this.listView1.Items.Clear();

                var select = this.comboBox1.SelectedIndex;
                this.comboBox1.Items.Clear();
                this.comboBox1.Items.Add("ALL");

                foreach (var user in userlist)
                {
                    ListViewItem item = new ListViewItem(user.NickName)
                    {
                        Tag = user
                    };
                    item.SubItems.Add(new ListViewItem.ListViewSubItem(item, user.OnLineStatus == 0 ? "Offline" : user.OnLineStatus == 2 ? "Leave" : "Online"));
                    this.listView1.Items.Add(item);
                    this.comboBox1.Items.Add(user);
                }

                if (select < this.comboBox1.Items.Count && select != -1)
                {
                    this.comboBox1.SelectedIndex = select;
                }
                else
                {
                    this.comboBox1.SelectedIndex = 0;
                }
            }
            catch (NetxException er)
            {
                MessageBox.Show(er.Message);
            }
        }

        private async Task LoginServer()
        {
            while (true)
            {
                try
                {
                    if (!await Get<IServer>().CheckLogIn())
                    {
                        LogOn logOn = new LogOn();
                        logOn.ShowDialog();

                        if(!await Get<IServer>().CheckLogIn())
                        {
                            this.Close();
                        }

                    }
                    else
                        break;
                }
                catch (Netx.NetxException er)
                {
                    MessageBox.Show(er.Message);
                    this.Close();
                    break;
                }

            }
        }
        [TAG(ClientTag.UserAdd)]
        public void UserAdd(Users newuser)
        {
            this.BeginInvoke(new EventHandler((a, b) =>
            {
                foreach (ListViewItem item in this.listView1.Items)
                {
                    if (item.Tag is Users user)
                    {
                        if (user.UserId == newuser.UserId)
                            return;
                    }
                }

                ListViewItem newitem = new ListViewItem(newuser.NickName)
                {
                    Tag = newuser
                };
                newitem.SubItems.Add(new ListViewItem.ListViewSubItem(newitem, newuser.OnLineStatus == 0 ? "Offline" : newuser.OnLineStatus == 2 ? "Leave" : "Online"));
                this.listView1.Items.Add(newitem);
                this.comboBox1.Items.Add(newuser);

            }));
        }

        [TAG(ClientTag.UpdateStatus)]
        public void SetUserStats(long userid, byte status)
        {
            this.BeginInvoke(new EventHandler((a, b) =>
            {

                foreach (ListViewItem item in this.listView1.Items)
                {
                    if (item.Tag is Users user)
                    {
                        if (user.UserId == userid)
                        {
                            user.OnLineStatus = status;
                            item.SubItems[1].Text = user.OnLineStatus == 0 ? "Offline" : user.OnLineStatus == 2 ? "Leave" : "Online";
                        }
                    }
                }
            }));
        }

        [TAG(ClientTag.Message)]
        public void SayMessage(long fromuserId,string fromusername, byte msgType, string msg,long time=0)
        {
            this.BeginInvoke(new EventHandler((a, b) =>
            {

                switch (msgType)
                {
                    case 0:
                        {
                            this.richTextBox1.AppendText($"[A]{fromusername} {DateTime.Now.ToString("T")} \r\n{msg}\r\n");
                        }
                        break;
                    case 1:
                        {
                            this.richTextBox1.AppendText($"       [P]{fromusername} {DateTime.Now.ToString("T")} \r\n           {msg}\r\n");
                        }
                        break;
                    case 2:
                        {
                            if(time==0)
                                this.richTextBox1.AppendText($" [L]{fromusername} {DateTime.Now.ToString("T")} \r\n   {msg}\r\n");
                            else
                                this.richTextBox1.AppendText($" [L]{fromusername} {TimeHelper.GetTime(time).ToString("T")} \r\n   {msg}\r\n");
                        }
                        break;
                }
            }));
        }

        [TAG(ClientTag.NeedLogOn)]
        public void NeedLogOn()
        {
            this.BeginInvoke(new EventHandler(async (a, b) =>
            {
                await LoginServer();
                await LoadingUserList();
                await GetLGetLeaving();
            }));
        }



        private async void Button1_Click(object sender, EventArgs e)
        {
            var select = this.comboBox1.SelectedItem;

            try
            {
                string msg = this.textBox1.Text;

                long userid = -1;

                if (select is Users user)
                {
                    userid = user.UserId;
                    this.richTextBox1.AppendText($"  ->{user.NickName} {DateTime.Now.ToString("T")}\r\n   {msg}\r\n");
                }
              

                await Get<IServer>().Say(userid, msg);
              

            }
            catch (NetxException er)
            {
                MessageBox.Show(er.Message);
            }
            
        }
    }
}
