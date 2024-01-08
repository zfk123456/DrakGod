/****************************************************
    文件：LoadingWnd.cs
	功能：加载进度界面
*****************************************************/

using UnityEngine;
using UnityEngine.UI;
using PEProtocol;

public class LoadingWnd : WindowRoot {
    public Text txtTips;
    public Image imgFG;
    public Image imgPoint;
    public Text txtPrg;
    public Text txtVersion;

    //PlayerPrefs数据类对象
    GameSettings gameSettings = new GameSettings();


    
    private float fgWidth;

    protected override void InitWnd() {
        base.InitWnd();
        fgWidth = imgFG.GetComponent<RectTransform>().sizeDelta.x;
        //初始化时使用的为存储在的gameSettings类最初版本号
        gameSettings.SaveSettings();

        //发送当前版本号到服务端中
        GameMsg msg = new GameMsg
        {
            cmd = (int)CMD.ReqABDownlod,
            reqABDownlod = new ReqABDownlod
            {
                //使用当前服务器版本号发送至服务器进行校验
                VersionNumber = gameSettings.LoadSettings(),//最初版本号配置在客户端Constants常量配置中
            }
        };
        
        netSvc.SendMsg(msg);//发送网络消息

        SetText(txtTips, "这是一条游戏Tips");
        SetText(txtPrg, "0%");
        //如果不需要更新则使用最初的版本号 需要时写收到网络消息后的版本号  
        SetText(txtVersion, gameSettings.LoadSettings());
        imgFG.fillAmount = 0;
        imgPoint.transform.localPosition = new Vector3(-545f, 0, 0);
    }
    //在这个加载进度界面写一个发送当前客户端版本号到服务器的网络请求消息在服务端进行判断（是否需要更新）  通过在netsvc网络服务中中转到LoginSys.cs进行判断处理  需要更新则中转到异步到WWW下载类去下载更新的文件（异步完成后也是进入场景） 不需要则直接进入登陆界面
    //解析ab包时到netsvc进行ab解析处理  记住下载的ab包名字要和原来的名字相同这样才能进行覆盖操作才能达到热更新的效果  否则原来用ab包加载的ui或者其他还是和原来一样

    //发送当前客户端版本号到服务端中 
    public void ClickAcceptBtn()
    {

    }


    public void SetProgress(float prg) {
        SetText(txtPrg, (int)(prg * 100) + "%");
        imgFG.fillAmount = prg;

        float posX = prg * fgWidth - 545;
        imgPoint.GetComponent<RectTransform>().anchoredPosition = new Vector2(posX, 0);
    }
}