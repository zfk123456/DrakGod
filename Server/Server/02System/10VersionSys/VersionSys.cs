using System.Collections.Generic;
using System.Runtime.InteropServices;
using PEProtocol;

public class VersionSys
{
    private static VersionSys instance = null;
    public static VersionSys Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new VersionSys();
            }
            return instance;
        }
    }
    public void Init()
    {

        PECommon.Log("VersionSys Init Done.");
    }

    public void ReqABDownlod(MsgPack pack)
    {
        //接收客户端传来的版本号信息
        ReqABDownlod data = pack.msg.reqABDownlod;
        //设定当前服务器发送的网络消息cmd
        GameMsg msg = new GameMsg
        {
            cmd = (int)CMD.RspABDownlod,
        };
        //比较服务器当前版本号与客户端当前版本号是否相同
        //由于当前服务器逻辑较为简单 也没有部署socket热部署程序集  所以当前简单逻辑为服务器的版本号只要与客户端的不相同就代表有更新
        if (data.VersionNumber !=PECommon.clientVersion)
        {
            RspABDownlod rspABDownlod = new RspABDownlod
            {
                VersionNumber = PECommon.clientVersion,//发送当前服务器版本号给客户端
                //同时发送下载的url  最好封装一个下载类   将当前服务器版本号传入然后返回一个对应的url返回给客户端 客户端根据对应url下载
                IsNeedABDownlod = true,
            };
            msg.rspABDownlod = rspABDownlod;
        }
        else if(data.VersionNumber== PECommon.clientVersion)
        {
            RspABDownlod rspABDownlod = new RspABDownlod
            {
            
                IsNeedABDownlod = false,
            };
            msg.rspABDownlod = rspABDownlod;
        }
        else
        {
            //抛出异常给客户端
        }
        pack.session.SendMsg(msg);//发送网络消息给客户端

    }
}