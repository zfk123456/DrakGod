/****************************************************
	文件：NetSvc.cs 	
	功能：AB包服务
*****************************************************/
using System;
using System.IO;
using PENet;
using PEProtocol;

//public class DownloadAB
//{

//    public void ABDownload(ClientState state, string abName, string abVersion)
//    {
//        // 构造AB包的完整路径 ABFolder为存放ab包的文件夹名
//        //Directory.GetCurrentDirectory获取应用程序当前工作目录
//        // Path.Combine获取包含当前执行的代码的程序集的加载文件的完整路径
//        string abPath = Path.Combine(Directory.GetCurrentDirectory(), "ABFolder", abName + "." + abVersion);

//        // 判断AB包是否存在
//        if (File.Exists(abPath))
//        {
//            // 读取AB包内容，并发送给客户端
//            byte[] abBytes = File.ReadAllBytes(abPath);
//            byte[] sendData = Message.PackData((int)MessageCode.DownloadAB, abBytes);
//            state.socket.Send(sendData);
//        }
//        else
//        {
//            // AB包不存在，返回错误信息给客户端
//            byte[] sendData = Message.PackData((int)MessageCode.Error, "AB package not found.");
//            state.socket.Send(sendData);
//        }
//    }

//    public void ReqABDownlod(MsgPack pack)
//    {

//        ReqABDownlod data = pack.msg.reqABDownlod;//转接当前客户端的ab请求下载信息（获取当前判断是否需要下载的一个依据 如服务器设置一个唯一静态变量版本号 则游戏加载时就发送网络消息过来 判断当前版本是否与服务器中存储的版本号相同）

//    }
//}

