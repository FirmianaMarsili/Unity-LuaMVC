using System;
/********************************************************************************
** desc      ： 用来线程更新下载
** Update: 2018.8.2
*********************************************************************************/
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using UnityEngine;

/// <summary>
/// 当前线程管理器，同时只能做一个任务
/// </summary>
public class ThreadManager : MonoBehaviour
{
    public static ThreadManager Instance;
    private Thread thread;
    private Action<NotiData> func;
    private Stopwatch sw = new Stopwatch();
    private string currDownFile = string.Empty;

    static readonly object m_lockObject = new object();
    static Queue<ThreadEvent> events = new Queue<ThreadEvent>();

    delegate void ThreadSyncEvent(NotiData data);
    private ThreadSyncEvent m_SyncEvent;

    void Awake()
    {
        Instance = this;
        m_SyncEvent = OnSyncEvent;
        thread = new Thread(OnUpdate);
    }

    // Use this for initialization
    void Start()
    {
        thread.Start();
    }

    /// <summary>
    /// 添加到事件队列
    /// </summary>
    public void AddEvent(ThreadEvent ev, Action<NotiData> func)
    {
        lock (m_lockObject)
        {
            this.func = func;
            events.Enqueue(ev);
        }
    }

    /// <summary>
    /// 通知事件
    /// </summary>
    /// <param name="state"></param>
    private void OnSyncEvent(NotiData data)
    {
        if (this.func != null) func(data);  //回调逻辑层
    }

    // Update is called once per frame
    void OnUpdate()
    {
        while (true)
        {
            lock (m_lockObject)
            {
                if (events.Count > 0)
                {
                    ThreadEvent e = events.Dequeue();
                    try
                    {
                        switch (e.Key)
                        {
                            case NotiConst.UPDATE_EXTRACT:
                                {     //解压文件
                                    OnExtractFile(e.evParams);
                                }
                                break;
                            case NotiConst.UPDATE_DOWNLOAD:
                                {    //下载文件
                                    OnDownloadFile(e.evParams);
                                }
                                break;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        UnityEngine.Debug.LogError(ex.Message);
                    }
                }
            }
            Thread.Sleep(1);
        }
    }

    /// <summary>
    /// 下载文件
    /// </summary>
    void OnDownloadFile(List<object> evParams)
    {
        string url = evParams[0].ToString();
        currDownFile = evParams[1].ToString();

        using (System.Net.WebClient client = new System.Net.WebClient())
        {
            sw.Start();
            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
            try
            {
                client.DownloadFileAsync(new System.Uri(url), currDownFile);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError(ex.ToString());           
            }
           
        }
    }

    private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        //UnityEngine.Debug.Log(e.ProgressPercentage);
        /*
        UnityEngine.Debug.Log(string.Format("{0} MB's / {1} MB's",
            (e.BytesReceived / 1024d / 1024d).ToString("0.00"),
            (e.TotalBytesToReceive / 1024d / 1024d).ToString("0.00")));
        */
        //float value = (float)e.ProgressPercentage / 100f;

        var speed = (e.BytesReceived / 1024d / sw.Elapsed.TotalSeconds);
        string value = null;
        if (speed < 1000)
        {
            value = string.Format("{0} kb/s", speed.ToString("0.00"));
        }
        else
        {
            speed /= 1024d;
            value = string.Format("{0} m/s", speed.ToString("0.00"));
        }

         
        NotiData data = new NotiData(NotiConst.UPDATE_Speed, value);
        if (m_SyncEvent != null) m_SyncEvent(data);
        data = new NotiData(NotiConst.UPDATE_Progress, e.ProgressPercentage);
        if (m_SyncEvent != null) m_SyncEvent(data);
        if (e.ProgressPercentage == 100 && e.BytesReceived == e.TotalBytesToReceive)
        {
            sw.Reset();

            data = new NotiData(NotiConst.UPDATE_DOWNLOAD, currDownFile);
            if (m_SyncEvent != null) m_SyncEvent(data);
        }
    }

    /// <summary>
    /// 调用方法
    /// </summary>
    void OnExtractFile(List<object> evParams)
    {
        UnityEngine.Debug.LogWarning("Thread evParams: >>" + evParams.Count);

        ///------------------通知更新面板解压完成--------------------
        NotiData data = new NotiData(NotiConst.UPDATE_DOWNLOAD, null);
        if (m_SyncEvent != null) m_SyncEvent(data);
    }

    /// <summary>
    /// 应用程序退出
    /// </summary>
    void OnDestroy()
    {
        thread.Abort();
    }
}