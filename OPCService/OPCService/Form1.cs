﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OPCAutomation;
using System.Net;

namespace OPCService
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        #region 私有变量
        /// <summary>
        /// OPCServer Object
        /// </summary>
        OPCServer KepServer;

        /// <summary>
        /// OPCGroups Object
        /// </summary>
        OPCGroups KepGroups;

        /// <summary>
        /// OPCGroup Object
        /// </summary>
        OPCGroup KepGroup;

        /// <summary>
        /// OPCItems Object
        /// </summary>
        OPCItems KepItems;

        /// <summary>
        /// OPCItem Object
        /// </summary>
        OPCItem KepItem;

        /// <summary>
        /// 主机IP
        /// </summary>
        string strHostIP = "";

        /// <summary>
        /// 主机名称
        /// </summary>
        string strHostName = "";

        /// <summary>
        /// 连接状态
        /// </summary>
        bool opc_connected = false;

        /// <summary>
        /// 客户端句柄
        /// </summary>
        int itmHandleClient = 0;

        /// <summary>
        /// 服务端句柄
        /// </summary>
        int itmHandleServer = 0;

        #endregion

        #region 方法
        /// <summary>
        /// 枚举本地OPC服务器
        /// </summary>
        private void GetLocalServer()
        {
            //获取本地计算机IP,计算机名称
            //IPHostEntry IPHost = Dns.GetHostEntry(Environment.MachineName);
            //if (IPHost.AddressList.Length > 0)
            //{
            //    strHostIP = IPHost.AddressList[0].ToString();
            //}
            //else
            //{
            //    return;
            //}

            strHostName = Dns.GetHostName();
            //MessageBox.Show(strHostName);

            //通过IP来获取计算机名称，可用在局域网内
            //IPHostEntry ipHostEntry = Dns.GetHostEntry(strHostIP);
            //strHostName = ipHostEntry.HostName.ToString();
            //MessageBox.Show(strHostName);

            //获取本地计算机上的OPCServerName
            try
            {
                KepServer = new OPCServer();
                object serverList = KepServer.GetOPCServers(strHostName);

                foreach (string turn in (Array)serverList)
                {
                    cmbServerName.Items.Add(turn);
                }

                cmbServerName.SelectedIndex = 0;
                btnConnServer.Enabled = true;
            }
            catch (Exception err)
            {
                MessageBox.Show("枚举本地OPC服务器出错：" + err.Message, "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

        }
        /// <summary>
        /// 创建组
        /// </summary>
        private bool CreateGroup()
        {
            try
            {
                KepGroups = KepServer.OPCGroups;            // 获取服务器的 OPC groups 集合
                KepGroup = KepGroups.Add("OPCDOTNETGROUP"); // 添加一个新的 OPC group
                SetGroupProperty();
                KepGroup.DataChange += new DIOPCGroupEvent_DataChangeEventHandler(KepGroup_DataChange);
                KepGroup.AsyncWriteComplete += new DIOPCGroupEvent_AsyncWriteCompleteEventHandler(KepGroup_AsyncWriteComplete);
                KepGroup.AsyncReadComplete += new DIOPCGroupEvent_AsyncReadCompleteEventHandler(KepGroup_AsyncReadComplete);
                KepItems = KepGroup.OPCItems;               // 获取该 group 的 Items 集合
            }
            catch (Exception err)
            {
                MessageBox.Show("创建组出现错误：" + err.Message, "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }
        /// <summary>
        /// 设置组属性
        /// </summary>
        private void SetGroupProperty()
        {
            KepServer.OPCGroups.DefaultGroupIsActive = Convert.ToBoolean(txtGroupIsActive.Text);
            KepServer.OPCGroups.DefaultGroupDeadband = Convert.ToInt32(txtGroupDeadband.Text);
            KepGroup.UpdateRate = Convert.ToInt32(txtUpdateRate.Text);
            KepGroup.IsActive = Convert.ToBoolean(txtIsActive.Text);
            KepGroup.IsSubscribed = Convert.ToBoolean(txtIsSubscribed.Text);
        }

        /// <summary>
        /// 列出OPC服务器中所有节点
        /// </summary>
        /// <param name="oPCBrowser"></param>
        private void RecurBrowse(OPCBrowser oPCBrowser)
        {
            //展开分支
            oPCBrowser.ShowBranches();
            //展开叶子
            oPCBrowser.ShowLeafs(true);
            int num = 0;
            foreach (object turn in oPCBrowser)
            {
                listBox1.Items.Add(turn.ToString());
                num++;
            }
            lblTagNum.Text = "" + num;
        }

        /// <summary>
        /// 获取服务器信息，并显示在窗体状态栏上
        /// </summary>
        private void GetServerInfo()
        {
            //tsslServerStartTime.Text = "开始时间:" + KepServer.StartTime.ToString() + "    ";
            //tsslversion.Text = "版本:" + KepServer.MajorVersion.ToString() + "." + KepServer.MinorVersion.ToString() + "." + KepServer.BuildNumber.ToString();
            Console.WriteLine("开始时间:" + KepServer.StartTime.ToString());
            Console.WriteLine("版本:" + KepServer.MajorVersion.ToString() + "." + KepServer.MinorVersion.ToString() + "." + KepServer.BuildNumber.ToString());
        }

        /// <summary>
        /// 连接OPC服务器
        /// </summary>
        /// <param name="remoteServerIP">OPCServerIP</param>
        /// <param name="remoteServerName">OPCServer名称</param>
        private bool ConnectRemoteServer(string remoteServerIP, string remoteServerName)
        {
            try
            {
                KepServer.Connect(remoteServerName, remoteServerIP);

                if (KepServer.ServerState == (int)OPCServerState.OPCRunning)
                {
                    //tsslServerState.Text = "已连接到-" + KepServer.ServerName + "   ";
                    Console.WriteLine("已连接到-" + KepServer.ServerName);
                }
                else
                {
                    //这里你可以根据返回的状态来自定义显示信息，请查看自动化接口API文档
                    //tsslServerState.Text = "状态：" + KepServer.ServerState.ToString() + "   ";
                    Console.WriteLine("状态：" + KepServer.ServerState.ToString());
                }
            }
            catch (Exception err)
            {
                Console.WriteLine("连接远程服务器出现错误：" + err.Message, "提示信息");
                return false;
            }
            return true;
        }
        #endregion

        #region 事件
        /// <summary>
        /// 写入TAG值时执行的事件
        /// </summary>
        /// <param name="TransactionID"></param>
        /// <param name="NumItems"></param>
        /// <param name="ClientHandles"></param>
        /// <param name="Errors"></param>
        void KepGroup_AsyncWriteComplete(int TransactionID, int NumItems, ref Array ClientHandles, ref Array Errors)
        {
            //lblState.Text = "";
            for (int i = 1; i <= NumItems; i++)
            {
                //lblState.Text += "Tran:" + TransactionID.ToString() + "   CH:" + ClientHandles.GetValue(i).ToString() + "   Error:" + Errors.GetValue(i).ToString();
                Console.WriteLine("Tran:" + TransactionID.ToString() + "   CH:" + ClientHandles.GetValue(i).ToString() + "   Error:" + Errors.GetValue(i).ToString());
            }
        }

        void KepGroup_AsyncReadComplete(int transactionID, int numItems, ref Array clientHandles, ref Array itemValues, ref Array qualities, ref Array timeStamps, ref Array errors)
        {
            // 处理异步读取完成的结果
            Console.WriteLine("Async Read completed:");
            for (int i = 1; i <= numItems; i++)
            {
                Console.WriteLine($"Item: {clientHandles.GetValue(i)} Value: {itemValues.GetValue(i)} Quality: {qualities.GetValue(i)} Timestamp: {timeStamps.GetValue(i)}");
            }
        }

        /// <summary>
        /// 每当项数据有变化时执行的事件
        /// </summary>
        /// <param name="TransactionID">处理ID</param>
        /// <param name="NumItems">项个数</param>
        /// <param name="ClientHandles">项客户端句柄</param>
        /// <param name="ItemValues">TAG值</param>
        /// <param name="Qualities">品质</param>
        /// <param name="TimeStamps">时间戳</param>
        void KepGroup_DataChange(int TransactionID, int NumItems, ref Array ClientHandles, ref Array ItemValues, ref Array Qualities, ref Array TimeStamps)
        {
            //为了测试，所以加了控制台的输出，来查看事物ID号
            //Console.WriteLine("********"+TransactionID.ToString()+"*********");
            for (int i = 1; i <= NumItems; i++)
            {
                this.txtTagValue.Text = ItemValues.GetValue(i).ToString();
                this.txtQualities.Text = Qualities.GetValue(i).ToString();
                this.txtTimeStamps.Text = TimeStamps.GetValue(i).ToString();
            }
        }

        /// <summary>
        /// 选择OPC服务器列表项时处理的事情
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (itmHandleClient != 0)
                {
                    this.txtTagValue.Text = "";
                    this.txtQualities.Text = "";
                    this.txtTimeStamps.Text = "";

                    Array Errors;
                    OPCItem bItem = KepItems.GetOPCItem(itmHandleServer);
                    //注：OPC中以1为数组的基数
                    int[] temp = new int[2] { 0, bItem.ServerHandle };
                    Array serverHandle = (Array)temp;
                    //移除上一次选择的项
                    KepItems.Remove(KepItems.Count, ref serverHandle, out Errors);
                }
                itmHandleClient = 1;
                KepItem = KepItems.AddItem(listBox1.SelectedItem.ToString(), itmHandleClient);  // 添加要读取的 OPC Item
                itmHandleServer = KepItem.ServerHandle;
            }
            catch (Exception err)
            {
                //没有任何权限的项，都是OPC服务器保留的系统项，此处可不做处理。
                itmHandleClient = 0;
                txtTagValue.Text = "Error ox";
                txtQualities.Text = "Error ox";
                txtTimeStamps.Text = "Error ox";
                Console.WriteLine("此项为系统保留项:" + err.Message, "提示信息");
            }
        }

        /// <summary>
        /// 载入窗体时处理的事情
        /// </summary>
        private void Form1_Load(object sender, EventArgs e)
        {
            GetLocalServer();
            opcClientClass();
        }
        /// <summary>
        /// 关闭窗体时处理的事情
        /// </summary>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!opc_connected)
            {
                return;
            }

            if (KepGroup != null)
            {
                KepGroup.DataChange -= new DIOPCGroupEvent_DataChangeEventHandler(KepGroup_DataChange);
            }

            if (KepServer != null)
            {
                KepServer.Disconnect();
                KepServer = null;
            }

            opc_connected = false;
        }

        /// <summary>
        /// 【按钮】设置
        /// </summary>
        private void btnSetGroupPro_Click(object sender, EventArgs e)
        {
            SetGroupProperty();
        }

        /// <summary>
        /// 【按钮】连接ＯＰＣ服务器
        /// </summary>
        private void btnConnServer_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ConnectRemoteServer("", cmbServerName.Text))
                {
                    return;
                }
                //ConnectRemoteServer("", "Matrikon.OPC.Simulation.1");


                btnSetGroupPro.Enabled = true;
                opc_connected = true;
                GetServerInfo();
                RecurBrowse(KepServer.CreateBrowser());

                if (!CreateGroup())
                {
                    return;
                }
            }
            catch (Exception err)
            {
                Console.WriteLine("初始化出错：" + err.Message, "提示信息");
            }
        }

        /// <summary>
        /// 【按钮】写入
        /// </summary>
        private void btnWrite_Click(object sender, EventArgs e)
        {
            OPCItem bItem = KepItems.GetOPCItem(itmHandleServer);
            int[] temp = new int[2] { 0, bItem.ServerHandle };
            Array serverHandles = (Array)temp;
            object[] valueTemp = new object[2] { "", txtWriteTagValue.Text };
            Array values = (Array)valueTemp;
            Array Errors;
            int cancelID;
            KepGroup.AsyncWrite(1, ref serverHandles, ref values, out Errors, 2009, out cancelID);
            //KepItem.Write(txtWriteTagValue.Text);//这句也可以写入，但并不触发写入事件
            GC.Collect();
        }
        #endregion

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void opcClientClass()
        {
            using (var opcClient = new OpcClient("Matrikon.OPC.Simulation.1"))
            {
                if (opcClient.Connect())
                {
                    if (opcClient.AddGroup("Group1"))
                    {
                        if (opcClient.AddItem("Group1", "Bucket Brigade.Real8"))
                        {
                            Console.WriteLine("Item added successfully. Reading data...");
                            object value = opcClient.ReadItemValue("Group1", "Bucket Brigade.Real8");
                            if (value != null)
                            {
                                Console.WriteLine($"Value of Item 'Bucket Brigade.Real8': {value}");
                            }
                            else
                            {
                                Console.WriteLine("Failed to read item value.");
                            }

                            int newVal = Convert.ToInt32(value) + 1;
                            if (opcClient.WriteItemValue("Group1", "Bucket Brigade.Real8", newVal))
                            {
                                Console.WriteLine("Item Bucket Brigade.Real8 write successfully");
                            }
                            else
                            {
                                Console.WriteLine("Failed to write item value.");
                            }

                            //异步写入批量值 
                            //opcClient.AddItem("Group1", "Bucket Brigade.Real4");
                            Dictionary<string, object> itemValues = new Dictionary<string, object>();
                            itemValues.Add("Bucket Brigade.Real8", 58);
                            //itemValues.Add("Bucket Brigade.Real4", 25);
                            opcClient.WriteItemValues("Group1", itemValues);
                        }
                        else
                        {
                            Console.WriteLine("Failed to add item.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Failed to add group.");
                    }
                }
                else
                {
                    Console.WriteLine("Failed to connect to OPC server.");
                }
            }
        }

    }
}
