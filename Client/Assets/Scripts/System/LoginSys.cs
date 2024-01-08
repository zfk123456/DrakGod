/****************************************************
    文件：LoginSys.cs
	功能：登录注册业务系统
*****************************************************/

using PEProtocol;
using UnityEngine;

public class LoginSys : SystemRoot {
    public static LoginSys Instance = null;

    public LoginWnd loginWnd;
    public CreateWnd createWnd;
    private AbSvc AbSvc;
    //public bool IsLoadab;//用于判断ab的下载情况 用于异步加载登陆场景时的一个条件
    public override void InitSys() {
        base.InitSys();

        Instance = this;
        PECommon.Log("Init LoginSys...");
    }

    /// <summary>
    /// 进入登录场景
    /// </summary>
    public void EnterLogin() {
        //异步的加载登录场景
        //并显示加载的进度
        //加载登陆场景 并将进度条窗口委托  先显示加载进度条的界面
        resSvc.AsyncLoadSceneAB(Constants.SceneLogin, () => {
            //加载完成以后再打开注册登录界面
            loginWnd.SetWndState();//此处由于增加了逻辑   加载进度条ui时会发送网络消息到服务器校验当前版本号 所以需要思考怎么设立条件让当前进度条ui在不满足当前版本号条件下不进入异步加载好的登陆场景
            //已解决 使用了一个absvc服务中的回调函数将当前进度回调给资源服务中设置的变量IsLoadab  接收到服务器回应的需要下载的网络消息后只有当IsLoadab为1时才可以进入加载场景
            audioSvc.PlayBGMusic(Constants.BGLogin);
        });
    }

    public void RspLogin(GameMsg msg) {
        GameRoot.AddTips("登录成功");
        GameRoot.Instance.SetPlayerData(msg.rspLogin);

        if (msg.rspLogin.playerData.name == "") {
            createWnd.SetWndState();
        }
        else {
            MainCitySys.Instance.EnterMainCity();
        }
        //关闭登录界面
        loginWnd.SetWndState(false);
    }

    public void RspRename(GameMsg msg) {
        GameRoot.Instance.SetPlayerName(msg.rspRename.name);

        //跳转场景进入主城
        MainCitySys.Instance.EnterMainCity();
        //关闭创建界面
        createWnd.SetWndState(false);
    }



    public void RspABDownlod(GameMsg msg)
    {
        if (msg.rspABDownlod.IsNeedABDownlod == true)
        {
            //同时需要更新客户端用GameSettings存储的版本号也由服务器发送

            string updateUrl = msg.rspABDownlod.VersionNumber;//这里的url由服务器msg发送过来
            
            AbSvc = new AbSvc();
            StartCoroutine(AbSvc.DownloadAssetBundle(updateUrl,
                 (bundle) => {
                     //此处为下载完成ab包后的处理  安装ab包等等
                     
                     //下载完成后将resSvc.IsLoadab至空
                     resSvc.IsLoadab = 1;
                     Debug.Log("Download success");
                 },
                 (progress) => {
                     //此处用于调用当前下载进度并赋值给resSvc设置的变量用于判断当前是否应该加载场景 
                     resSvc.IsLoadab = progress;
                     Debug.Log($"Download progress: {progress * 100}%");
                     }
                 ));//OnUpdateDownloaded为下载完成后执行的回调函数
        }
        else if(msg.rspABDownlod.IsNeedABDownlod == false)
        {
            //如果为不需要更新的网络消息则直接将resSvc.IsLoadab至为1直接在场景异步加载完成后关闭掉当前进度ui的显示
            resSvc.IsLoadab = 1;
        }

    }
}