using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PEProtocol;
using System.Text;
using System.Linq;

public class FriendWnd : WindowRoot
{
    #region ui组件
    public InputField iptFriend;//好友申请请求
    public Text friendtxt;
    public Text frientxtacc;
    public GameObject frdacc;
    public Transform scrollTrans;//动态ui父亲节点 好友信息UI
    public Transform scrollTrans1;//好友详细信息ui
    #endregion
    //private List<string> friendLst = new List<string>();//好友信息列表  最大限制设置好友为12个
    // private List<string> frdnameLst = new List<string>();//存储好友姓名列表

    private Dictionary<string, string> frdDic = new Dictionary<string, string>();

    //private List<TaskRewardData> trdLst = new List<TaskRewardData>();

    private PlayerData pd;

    //设置一个全局标志变量 默认为true用于第一次刷新ui时调取数据库中数据显示(即只使用一次 退出重新进入当前客户端时又设置回ture)
    //private bool isInitOne = true;

    protected override void InitWnd()
    {
        base.InitWnd();
        SetActive(frdacc, false);
        pd = GameRoot.Instance.PlayerData;

        RefreshUI();
    }
    //重构逻辑 好友的显示用一个单独的预制体动态加载（请求通过时则加载一条然后设置对应预制体信息）显示其信息 以及删除时只需要发送网络消息然后删除对应数据库数据  
    public void RefreshUI()
    {
        pd = GameRoot.Instance.PlayerData;
        //防止动态ui无限生成
        for (int i = 0; i < scrollTrans.childCount; i++)
        {
            Destroy(scrollTrans.GetChild(i).gameObject);
        }

        //调用客户端返回的pd数据 设置显示

        //正确的思路应为更新实时更新客户端pd数据  尝试删除或者更新时在gameroot设置一个setpd的函数   服务器回应时将返回的msg数据传入设置pdt
        //已解决  删除时发送了网络消息到服务器 服务器更新数据库并更新到缓存 缓存过后获取当前sesion的玩家数据 并返回一个rsppd的网络消息回应当前玩家数据pd  netsvc接收Rsppd的网络消息到主城系统中的rsppd函数处理  调取到游戏入口gameroot中的setplayerdate函数设置当前pd（即刷新了pd显示）
        for (int i = 0; i < pd.friendArr.Length; i++)
        {
            //isInitOne = false;
            //string friend = pd.friendArr[i];
            //if (!frdDic.ContainsKey(pd.friendArr[i]) && pd.friendArr[i] != null)
            //{
            //    frdDic.Add(pd.friendArr[i], " ");
            //}
            if (pd.friendArr[i] != null && pd.friendArr[i] != "" && pd.friendArr[i] != " ")
            {
                GameObject go = resSvc.LoadPrefab(PathDefine.ItemFriend);//动态加载ui预制体
                go.transform.SetParent(scrollTrans);//设置父亲节点
                go.transform.localPosition = Vector3.zero;
                go.transform.localScale = Vector3.one;
                go.name = pd.friendArr[i];

                SetText(GetTrans(go.transform, "txtName"), pd.friendArr[i]);
                //此处如果需要调用其战力或者等级还需要再维护一个专门的friend表 一对多 一一对应其信息（即每次上线时都更新一次好友最新的数据）
                //SetText(GetTrans(go.transform, "txtInfor"), Item.Value);
                //调用点击头像按钮执行好友详细信息按钮事件监听  使用匿名函数
                Button btnHead = GetTrans(go.transform, "icon").GetComponent<Button>();
                btnHead.onClick.AddListener(() =>
                {
                        //调用点击头像按钮方法  发送网络消息查询数据库
                        audioSvc.PlayUIAudio(Constants.UIOpenPage);
                    ClickHeadBtn(go.name);
                        //ClickHeadBtn();


                    });
                //调用删除按钮执行删除好友按钮事件监听  使用匿名函数
                Button btnDel = GetTrans(go.transform, "btnDel").GetComponent<Button>();
                btnDel.onClick.AddListener(() =>
                {
                        //调用删除按钮方法  发送网络消息 更新数据库
                        audioSvc.PlayUIAudio(Constants.UIExtenBtn);

                    ClickDelBtn(go.name, go);
                });
                SetActive(frdacc, false);
            }
            else
            {
                break;
            }

        }
        // }

        // 这段重构将两个列表换成了字典 更好删除只需删除对应数据即可  此处为调用当前临时存储的即在线申请好友成功返回的数据   下线时当前字典自动销毁
        if (frdDic != null)
        {
            foreach (var Item in frdDic)
            {


                GameObject go = resSvc.LoadPrefab(PathDefine.ItemFriend);//动态加载ui预制体
                go.transform.SetParent(scrollTrans);//设置父亲节点
                go.transform.localPosition = Vector3.zero;
                go.transform.localScale = Vector3.one;
                go.name = Item.Key;
                SetText(GetTrans(go.transform, "txtName"), Item.Key);
                //SetText(GetTrans(go.transform, "txtInfor"), Item.Value);
                //调用点击头像按钮执行好友详细信息按钮事件监听  使用匿名函数
                Button btnHead = GetTrans(go.transform, "icon").GetComponent<Button>();
                btnHead.onClick.AddListener(() =>
                {
                //调用点击按钮方法  发送网络消息查询数据库
                audioSvc.PlayUIAudio(Constants.UIOpenPage);
                    ClickHeadBtn(go.name);

                });
                //调用删除按钮执行删除好友按钮事件监听  使用匿名函数
                Button btnDel = GetTrans(go.transform, "btnDel").GetComponent<Button>();
                btnDel.onClick.AddListener(() =>
                {
                    audioSvc.PlayUIAudio(Constants.UIExtenBtn);
                    ClickDelBtn(go.name, go);
                });
                SetActive(frdacc, false);
            }
        }
    }
    #region lookfriend
    /// <summary>
    /// 点击发送查看好友详细信息网络消息给客户端
    /// 查资料后发现，在写好的方法的上一行打出“///”，系统会自动补全。是一个很好用的技巧呢。
    /// 记住调用主城的所有ui时都需要经过MainCitySys主城业务系统上 因为所有的主城uiwnd脚本都挂载在主城业务系统上！！！
    /// </summary>
    private void ClickHeadBtn(string name)
    {
        //发送查看好友消息发送至客户端  （然后等待服务端回应被查看好友的pd信息再进行函数处理）
        GameMsg lookmsg = new GameMsg
        {
            cmd = (int)CMD.ReqLookFriend,
            reqLookFriend = new ReqLookFriend
            {
                frdname = name,
            }
        };
        netSvc.SendMsg(lookmsg);
        //此处底下写个全新函数接受服务器函数客户端有消息回调回来时直接调用那个方法
        //用动态窗口添加到canvas画布上显示（ab处理）

    }
    public void OpenFriendInfo(PlayerData pd)
    {
        GameObject go = resSvc.LoadResourece("FriendInfoWnd", "fiendinfownd.ab");//使用ab包加载资源 ab包会保存对应的脚本代码
        go.transform.SetParent(scrollTrans1);
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one;
        //由于原来默认激活状态为fasle 所以需要激活状态
        SetActive(go);
        //transform.Find用于查找子节点，它并不会递归的查找物体，也就是说它只会查找它的子节点，并不会查找子节点的子节点。
        //如果想要寻找二级或者更下级子物体，需要将路径全标注
        //GameObject只能查找到active的物体。如果name指定路径，则按路径查找；否则递归查找，直到查找到第一个符合条件的GameObject或者返回null
        //由于没有面板赋值且由于层级过多transform.Find用于查找子节点，它并不会递归的查找物体，也就是说它只会查找它的子节点，并不会查找子节点的子节点。通过设置对应的路径找到当前物体为设置其ui的属性

        Button gbbtn = GetTrans(go.transform, "MainContent/btnClose").GetComponent<Button>();
        gbbtn.onClick.AddListener(() =>
        {
            SetActive(go, false);
        });
        SetText(GetTrans(go.transform, "charbg/infobg/txtInfofrd"), pd.name + " LV." + pd.lv);
        SetText(GetTrans(go.transform, "MainContent/valitem1/barbg/txtExpprg"), pd.exp + "/" + PECommon.GetExpUpValByLv(pd.lv));
        Image imgExpPrg = GetTrans(go.transform, "MainContent/valitem1/barbg/imgExpprg").GetComponent<Image>();
        imgExpPrg.fillAmount = pd.exp * 1.0F / PECommon.GetExpUpValByLv(pd.lv);


        SetText(GetTrans(go.transform, "MainContent/valitem2/barbg/txtPowerprg"), pd.power + "/" + PECommon.GetPowerLimit(pd.lv));
        Image imgPowerPrg = GetTrans(go.transform, "MainContent/valitem2/barbg/imgPowerprg").GetComponent<Image>();
        imgPowerPrg.fillAmount = pd.power * 1.0F / PECommon.GetPowerLimit(pd.lv);

        SetText(GetTrans(go.transform, "MainContent/valitem3/chardes"), " 职业   暗夜刺客");
        SetText(GetTrans(go.transform, "MainContent/valitem4/txtFight"), " 战力   " + PECommon.GetFightByProps(pd));
        SetText(GetTrans(go.transform, "MainContent/valitem5/txthp"), " 血量   " + pd.hp);
        SetText(GetTrans(go.transform, "MainContent/valitem6/txthurt"), " 伤害   " + (pd.ad + pd.ap));
        SetText(GetTrans(go.transform, "MainContent/valitem7/txtdef"), " 防御   " + (pd.addef + pd.apdef));


        Button xxbtn = GetTrans(go.transform, "MainContent/btnDetail").GetComponent<Button>();
        xxbtn.onClick.AddListener(() =>
        {
            Transform transDetail = GetTrans(go.transform, "transDetail");
            SetActive(transDetail);
        });
        Button btnClose = GetTrans(go.transform, "transDetail/btnClose").GetComponent<Button>();
        btnClose.onClick.AddListener(() =>
        {
            Transform transDetail = GetTrans(go.transform, "transDetail");
            SetActive(transDetail, false);
        });

        ////detail TODO
        SetText(GetTrans(go.transform, "transDetail/Group/propItem/val"), pd.hp);
        SetText(GetTrans(go.transform, "transDetail/Group/propItem1/val"), pd.ad);
        SetText(GetTrans(go.transform, "transDetail/Group/propItem2/val"), pd.ap);
        SetText(GetTrans(go.transform, "transDetail/Group/propItem3/val"), pd.addef);
        SetText(GetTrans(go.transform, "transDetail/Group/propItem4/val"), pd.apdef);
        SetText(GetTrans(go.transform, "transDetail/Group/propItem5/val"), pd.dodge + "%");
        SetText(GetTrans(go.transform, "transDetail/Group/propItem6/val"), pd.pierce + "%");
        SetText(GetTrans(go.transform, "transDetail/Group/propItem7/val"), pd.critical + "%");

    }
    #endregion
    #region delFriend
    //点击发送删除好友网络消息给客户端
    private void ClickDelBtn(string name, GameObject go)
    {
        //一个消息发送到服务器去删除数据库数据无需回应（如果删除错误直接回应一个错误码消息回来）
        GameMsg rmvmsg = new GameMsg
        {
            cmd = (int)CMD.SndRmvFriend,
            sndRmvFriend = new SndRmvFriend
            {
                frdname = name
            }
        };
        netSvc.SendMsg(rmvmsg);//发送删除好友网络消息

        Destroy(go);//销毁对应的游戏物体
        //删除对应的key值   同时刷新ui显示则可以删除对应的元素？待会需要测试三个客户端 todo
        //frdDic.Remove(name);
        if (frdDic.ContainsKey(name)&&frdDic!=null)
        {
            frdDic.Remove(name);
        }

        RefreshUI();
        GameRoot.AddTips(Constants.Color("您已成功删除好友!", TxtColor.Red));
    }
    #endregion

    #region 第一次发送申请（发送好友请求）
    private bool canSend = true;//好友申请时间限制标志变量
    //这里逻辑没问题  问题出在第二段发送取得请求 
    public void ClickSendBtn()//发送好友请求按钮 第一次
    {
        if (canSend == false)
        {
            GameRoot.AddTips("好友申请每5秒钟才能发送一条");
            return;
        }
        PlayerData pd = GameRoot.Instance.PlayerData;//引入玩家数据
        if (iptFriend != null && iptFriend.text != "" && iptFriend.text != " ")
        {
            if (iptFriend.text.Length > 4)
            {
                GameRoot.AddTips("输入的用户名称不合法");

            }
            else if (iptFriend.text == pd.name)
            {
                GameRoot.AddTips("您无法添加自己为好友");
            }
            else if (frdDic.ContainsKey(iptFriend.text))
            {
                GameRoot.AddTips("您已经添加过该好友");
            }
            else
            {
                //发送服务器请求
                GameMsg msg = new GameMsg
                {
                    cmd = (int)CMD.ReqFriend,
                    reqFriend = new ReqFriend
                    {
                        myname = pd.name,
                        frdname = iptFriend.text

                    }
                };
                iptFriend.text = "";//将好友名字清空            
                netSvc.SendMsg(msg);//发送网络消息
                //将标志变量设置为false
                canSend = false;

                //使用定时服务设置标志变量
                timerSvc.AddTimeTask((int tid) =>
                {
                    canSend = true;
                }, 5, PETimeUnit.Second);
            }
        }
        else
        {

            GameRoot.AddTips("尚未输入好友名称");
        }
    }

    #endregion

    #region 第二次发送消息（回应）
    private string name2;
    //推送给被申请的人
    public void AddFriendTips(string myname)
    {

        SetActive(frdacc, true);
        SetText(frientxtacc, myname + "请求添加你为好友");
        name2 = myname;
    }
    public void ClickAcceptBtn()//接受好友请求按钮
    {
        GameMsg msg = new GameMsg
        {
            cmd = (int)CMD.SndFriend,
            sndFriend = new SndFriend
            {
                isAccept = true,
                frdname = name2
            }
        };
        netSvc.SendMsg(msg);//发送网络消息
        RefreshUI();
    }

    public void ClickRefuseBtn()//拒绝好友请求按钮
    {
        GameMsg msg = new GameMsg
        {
            cmd = (int)CMD.SndFriend,
            sndFriend = new SndFriend
            {
                isAccept = false,
            }
        };
        netSvc.SendMsg(msg);//发送网络消息
        RefreshUI();
    }

    #endregion
    //增加好友成功处理函数(服务器回应消息请求)
    public void AddFriendMsg(string frdname, int lv, int power)
    {
        //friendLst.Add(Constants.Color(frdname, TxtColor.Blue) + "        等级:" + lv + "        战力:" + power);
        //friendLst.Add( "等级:" + lv + "战力:" + power);

        //if (friendLst.Count >= 12)
        //{
        //    GameRoot.AddTips("好友已达上限");

        //}
        //todo好友上限设置 不应该在这处理  想下在哪里处理且不能增加（应该是在添加好友申请处进行判定处理 包起来将整个）
        if (frdDic.Count >= 12)
        {
            GameRoot.AddTips("好友已达上限");

        }
        else if(frdname!=null)
        {
            //此处只解决的临时加好友的dic   bug出现在本身数据库内存数据中就拥有好友     （通过初始化数据   初始化时就将原有好友数据加入字典  这样刷新ui时也只需要一个函数！）todo
            frdDic.Add(frdname, "等级:" + lv + "战力:" + power);
            // frdDic.Add(frdname);

        }
        //处理完成后调用自身窗口刷新ui显示
        if (GetWndState())
        {
            RefreshUI();
        }
    }

    public void ClickCloseBtn()
    {
        audioSvc.PlayUIAudio(Constants.UIClickBtn);
        RefreshUI();//添加成功时刷新ui 刷新好友显示
        SetWndState(false);
    }
}

