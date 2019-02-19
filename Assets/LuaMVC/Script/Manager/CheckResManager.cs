using System;
/********************************************************************************
** desc      ： 释放包内资源到内存卡并检测时候有资源要更新
** Update: 2018.8.2
*********************************************************************************/
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using LuaMVC;
using UnityEngine.UI;
/// <summary>
/// CheckResManager：
/// </summary>

public class CheckResManager : MonoBehaviour
{

    public static CheckResManager Instance;
    public Transform loading_Tra;
    public Text text;
    private int needUpdateCount;
    private int allreadyUpdate = 1;
    private float rotateSpeed = 300;

    private List<string> downloadFiles = new List<string>();
    private void Awake()
    {        
        Instance = this;
        LuaMVC.Loom.Instance.Init();
    }
    private void Start()
    {
        StartCoroutine(CheckExtractResource());
    }


    private void Update()
    {
        loading_Tra.Rotate(-Vector3.forward * rotateSpeed * Time.deltaTime);
    }
    /// <summary>
    /// 检测是否要释放资源
    /// </summary>
    public IEnumerator CheckExtractResource()
    {
        // 是否释放过资源      
        //PlayerPrefs.SetInt("releaseLua" + Application.productName, 0);
        ////bool isExists = Directory.Exists(Utils_LuaMVC.DataPath) && File.Exists(Utils_LuaMVC.DataPath + "files.txt");
        //if (PlayerPrefs.GetInt("releaseLua" + Application.productName) == 1)
        //{
        yield return OnUpdateResource();
        yield break;
        //}
        //yield return (OnExtractResource());    //启动释放协成 
    }

    public void UpdateResource()
    {
        StartCoroutine(OnUpdateResource());
    }

    /// <summary>
    /// 释放资源到设备的资源目录
    /// </summary>
    /// <returns></returns>
    IEnumerator OnExtractResource()
    {
        text.text = "正在检查本地文件...";
        string dataPath = Utils_LuaMVC.DataPath + "MVCSCRIPT/"; /*Utils_LuaMVC.DataPath + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + "/"; */ //数据目录  + 当前游戏的名字 为了让他们更好的分开
        string resPath = Utils_LuaMVC.AppContentPath(); //游戏包资源目录

        if (Directory.Exists(dataPath)) Directory.Delete(dataPath, true);
        Directory.CreateDirectory(dataPath);

        string infile = resPath + "files.txt";
        string outfile = dataPath + "files.txt";
        if (File.Exists(outfile)) File.Delete(outfile);

        string message = "正在解包文件:>files.txt";
        Debug.Log(infile);
        Debug.Log(outfile);
        if (File.Exists(infile) && !File.Exists(outfile))
        {
            //if (Application.platform == RuntimePlatform.Android)
            //{
            WWW www = new WWW(infile);
            yield return www;
            //print(www.text);

            if (www.error != null)
            {
                Debug.Log("释放文件错误：" + www.error);
            }
            else
            {
                //File.WriteAllBytes(outfile, www.bytes);
                FileInfo t = new FileInfo(outfile);
                byte[] info = www.bytes;
                FileStream sw;
                int length = info.Length;

                sw = t.Create();
                sw.Write(info, 0, length);
                sw.Close();
                sw.Dispose();
            }
            //if (www.isDone)
            //{

            //}
            //yield return 0;
            //}
            //else File.Copy(infile, outfile, true);
            yield return new WaitForEndOfFrame();

            //释放所有文件到数据目录
            string[] files = File.ReadAllLines(outfile)[0].Split('|');
            int curIndex = 0;
            int totalNum = files.Length;
            foreach (var file in files)
            {
                //string[] fs = file.Split('|');
                infile = resPath + file;  //0
                outfile = dataPath + file;
                curIndex++;
                //message = "正在解包文件:>" + fs[0];
                text.text = string.Format("正在解压本地... ({0}/{1})", curIndex.ToString(), totalNum.ToString());
                //Debug.Log("正在解包文件(" + curIndex + "/" + totalNum + "):>" + infile);
                string dir = Path.GetDirectoryName(outfile);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                //if (Application.platform == RuntimePlatform.Android)
                //{
                WWW www_All = new WWW(infile);
                yield return www_All;

                if (www_All.isDone)
                {
                    try
                    {
                        File.WriteAllBytes(outfile, www_All.bytes);
                    }
                    catch (Exception ex)
                    {

                        throw new Exception("release error : " + "infile :" + infile + "          outfile:" + outfile);
                    }

                }
                yield return 0;
                //}
                //else
                //{
                //    if (File.Exists(outfile))
                //    {
                //        File.Delete(outfile);
                //    }
                //    try
                //    {
                //        File.Copy(infile, outfile, true);
                //    }
                //    catch (Exception ex)
                //    {

                //        throw new Exception("release error : " + "infile :" + infile + "          outfile:" + outfile);
                //    }

                //}
                yield return new WaitForEndOfFrame();
            }
            message = "解包完成!!!";
            yield return new WaitForSeconds(0.1f);
        }

        message = string.Empty;
        //释放完成，开始启动更新资源        
        PlayerPrefs.SetInt("releaseLua" + Application.productName, 1);
        text.text = "正在检查更新...";

        yield return OnUpdateResource();
    }
    private string GetUpdateUrl()
    {
        switch (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
        {
            case "Game_Brag_Loading":
                return AppConst.WebUrl_HotUpdate + "Brag/";
            default:
                throw new Exception("未对当前游戏配置更新url");
        }

    }
    IEnumerator OnUpdateResource()
    {
        text.text = "正在检查更新...";
        if (!AppConst.UpdateMode)
        {
            OnResourceInited();
            yield break;
        }
        string dataPath = Utils_LuaMVC.DataPath + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + "/";  //数据目录
        string url = GetUpdateUrl();
        string listUrl = url + "files.txt";/*"files.txt?v=" + random;*/
        string localFiles = dataPath + "files.txt";
        WWW www = new WWW(listUrl);
        yield return www;
        if (www.error != null)
        {
            OnUpdateFailed(string.Empty);
            yield break;
        }

        string filesText = www.text;
        string[] files = filesText.Split('\n');
        Dictionary<string, string> needUpdateInfo = new Dictionary<string, string>();
        string tmp_path = Path.GetDirectoryName(localFiles);
        if (!Directory.Exists(tmp_path))  //如果不存在file
        {
            Directory.CreateDirectory(tmp_path);
            for (int i = 1; i < files.Length; i++)
            {
                if (string.IsNullOrEmpty(files[i]))
                {
                    continue;
                }
                string[] keyValue = files[i].Split('|');
                string f = keyValue[0];
                string localfile = (dataPath + f).Trim();
                string path = Path.GetDirectoryName(localfile);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                string[] tmpF = f.Split('/');
                tmpF[tmpF.Length - 1] = keyValue[1].Trim();
                string str = null;
                foreach (var item in tmpF)
                {
                    if (str == null)
                    {
                        str += item;
                    }
                    else
                    {
                        str += "/";
                        str += item;
                    }
                }
                string fileUrl = files[0].Trim() + str; /*url + f + "?v=" + random;*/   //更改下载路径在这                          
                needUpdateInfo.Add(fileUrl, localfile);
                Debug.Log("准备更新文件：" + fileUrl);
                //message = "downloading>>" + fileUrl;
            }
        }
        else
        {
            Dictionary<string, string> serverFile = new Dictionary<string, string>();
            for (int i = 1; i < files.Length; i++)
            {
                if (string.IsNullOrEmpty(files[i]))
                {
                    continue;
                }
                string[] keyValue = files[i].Split('|');
                serverFile.Add(keyValue[0], keyValue[1]);
            }
            string local_str = File.ReadAllText(localFiles);
            string[] local_Files = local_str.Split('\n');

            for (int i = 1; i < local_Files.Length; i++)
            {
                if (string.IsNullOrEmpty(local_Files[i]))
                {
                    continue;
                }
                string[] keyValue = local_Files[i].Split('|');

                if (!serverFile.ContainsKey(keyValue[0]))
                {
                    string str = dataPath + keyValue[0];
                    if (File.Exists(str))
                    {
                        File.Delete(str);
                    }
                }
                else
                {
                    if (serverFile[keyValue[0]] == keyValue[1])
                    {
                        serverFile.Remove(keyValue[0]);
                    }
                }
            }
            foreach (var item in serverFile)
            {
                string localfile = (dataPath + item.Key).Trim();
                string path = Path.GetDirectoryName(localfile);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                string[] tmpF = item.Key.Split('/');
                tmpF[tmpF.Length - 1] = item.Value.Trim();
                string str = null;
                for (int i = 0; i < tmpF.Length; i++)
                {
                    if (str == null)
                    {
                        str += tmpF[i];
                    }
                    else
                    {
                        str += "/";
                        str += tmpF[i];
                    }
                }
                string fileUrl = files[0].Trim() + str; /*url + f + "?v=" + random;*/   //更改下载路径在这
                needUpdateInfo.Add(fileUrl, localfile);
                Debug.Log("准备更新文件：" + fileUrl);
            }
        }

        needUpdateCount = needUpdateInfo.Count;
        foreach (var item in needUpdateInfo.Keys)
        {
            BeginDownload(item, needUpdateInfo[item]);
            while (!(IsDownOK(needUpdateInfo[item])))
            {
                yield return new WaitForEndOfFrame();
            }
        }
        yield return new WaitForEndOfFrame();
        File.WriteAllBytes(localFiles, www.bytes);
        text.text = "正在初始化游戏...";
        OnResourceInited();
    }

    void OnUpdateFailed(string file)
    {
        //string message = "更新失败!>" + file;
        text.text = "更新失败,正在返回大厅";
        StartCoroutine(UpdateFile());
    }
    IEnumerator UpdateFile()
    {
        yield return new WaitForSeconds(1);
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("UserCenter");
    }

    /// <summary>
    /// 是否下载完成
    /// </summary>
    bool IsDownOK(string file)
    {
        return downloadFiles.Contains(file);
    }

    /// <summary>
    /// 线程下载
    /// </summary>
    void BeginDownload(string url, string file)
    {     //线程下载
        object[] param = new object[2] { url, file };

        ThreadEvent ev = new ThreadEvent();
        ev.Key = NotiConst.UPDATE_DOWNLOAD;
        ev.evParams.AddRange(param);
        ThreadManager.Instance.AddEvent(ev, OnThreadCompleted);   //线程下载
    }

    /// <summary>
    /// 线程完成
    /// </summary>
    /// <param name="data"></param>
    void OnThreadCompleted(NotiData data)
    {
        switch (data.evName)
        {
            case NotiConst.UPDATE_EXTRACT:  //解压一个完成

                break;
            case NotiConst.UPDATE_DOWNLOAD: //下载一个完成                
                downloadFiles.Add(data.evParam.ToString());
                allreadyUpdate++;
                Debug.Log("资源更新完成：" + data.evParam.ToString());
                break;
            case NotiConst.UPDATE_Speed: //下载进度

                Loom.InvokeSync(() =>
                {
                    text.text = string.Format("正在下载最新资源... ({0}/{1}))       {2}", allreadyUpdate.ToString(), needUpdateCount.ToString(), data.evParam.ToString());

                });
                break;
            case NotiConst.UPDATE_Progress:
                //Loom.QueueOnMainThread(() =>
                //{
                //    //slider.value = (float.Parse(data.evParam.ToString())/100);
                //    //Debug.Log(float.Parse(data.evParam.ToString()) / 100);
                //});
                break;





        }
    }
    /// <summary>
    /// 资源初始化结束
    /// </summary>
    public void OnResourceInited()
    {
        if (!AppConst.UpdateMode)
        {
            OnInitialize();

        }
        else
        {
            ResourceManager.Instance.Initialize(AppConst.AssetDir, delegate ()
            {
                this.OnInitialize();
            });
        }

    }
    public void OnInitialize()
    {
        Debug.Log("资源初始化结束");
        transform.root.gameObject.AddComponent<LuaMVC.Program>();
    }
}
public class ThreadEvent
{
    public string Key;
    public List<object> evParams = new List<object>();
}

public class NotiData
{
    public string evName;
    public object evParam;

    public NotiData(string name, object param)
    {
        this.evName = name;
        this.evParam = param;
    }
}
public class NotiConst
{
    public const string UPDATE_MESSAGE = "UpdateMessage";           //更新消息
    public const string UPDATE_EXTRACT = "UpdateExtract";           //更新解包
    public const string UPDATE_DOWNLOAD = "UpdateDownload";         //更新下载
    public const string UPDATE_Speed = "UpdateSpeed";         //更新速度
    public const string UPDATE_Progress = "UpdateProgress";  //更新进度


}
