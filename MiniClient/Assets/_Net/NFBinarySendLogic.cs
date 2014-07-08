﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using ProtoBuf;
using UnityEngine;
using NFTCPClient;
using NFMsg;

class StructureTransform
{
    static bool bBig = false;//defalut little
    public static void Reverse(byte[] msg)
    {
        if (!bBig)
        {
            Array.Reverse(msg);
        }
    }

    public static void Reverse(byte[] msg, int nOffest, int nSize)
    {
        if (!bBig)
        {
            Array.Reverse(msg, nOffest, nSize);
        }
    }


    public static bool SetEndian(bool bIsBig)
    {
        bBig = bIsBig;
        return bBig;
    }

    public static void ByteArrayToStructureEndian(byte[] bytearray, ref object obj, int startoffset)
    {
        int len = Marshal.SizeOf(obj);
        IntPtr i = Marshal.AllocHGlobal(len);
        byte[] temparray = (byte[])bytearray.Clone();
        // ŽÓœá¹¹ÌåÖžÕë¹¹Ôìœá¹¹Ìå
        obj = Marshal.PtrToStructure(i, obj.GetType());
        // ×öŽó¶Ë×ª»»
        object thisBoxed = obj;
        Type test = thisBoxed.GetType();
        int reversestartoffset = startoffset;
        // ÁÐŸÙœá¹¹ÌåµÄÃ¿žö³ÉÔ±£¬²¢Reverse
        foreach (var field in test.GetFields())
        {
            object fieldValue = field.GetValue(thisBoxed); // Get value

            TypeCode typeCode = Type.GetTypeCode(fieldValue.GetType());  //Get Type
            if (typeCode != TypeCode.Object)  //Èç¹ûÎªÖµÀàÐÍ
            {
                Reverse(temparray, reversestartoffset, Marshal.SizeOf(fieldValue));
                reversestartoffset += Marshal.SizeOf(fieldValue);
            }
            else  //Èç¹ûÎªÒýÓÃÀàÐÍ
            {
                reversestartoffset += ((byte[])fieldValue).Length;
            }
        }
        try
        {
            //œ«×ÖœÚÊý×éžŽÖÆµœœá¹¹ÌåÖžÕë
            Marshal.Copy(temparray, startoffset, i, len);
        }
        catch (Exception ex) { Console.WriteLine("ByteArrayToStructure FAIL: error " + ex.ToString()); }
        obj = Marshal.PtrToStructure(i, obj.GetType());
        Marshal.FreeHGlobal(i);  //ÊÍ·ÅÄÚŽæ
    }

    public static byte[] StructureToByteArrayEndian(object obj)
    {
        object thisBoxed = obj;   //copy £¬œ« struct ×°Ïä
        Type test = thisBoxed.GetType();

        int offset = 0;
        byte[] data = new byte[Marshal.SizeOf(thisBoxed)];

        object fieldValue;
        TypeCode typeCode;
        byte[] temp;
        // ÁÐŸÙœá¹¹ÌåµÄÃ¿žö³ÉÔ±£¬²¢Reverse
        foreach (var field in test.GetFields())
        {
            fieldValue = field.GetValue(thisBoxed); // Get value

            typeCode = Type.GetTypeCode(fieldValue.GetType());  // get type

            switch (typeCode)
            {
                case TypeCode.Single: // float
                    {
                        temp = BitConverter.GetBytes((Single)fieldValue);
                        StructureTransform.Reverse(temp);
                        Array.Copy(temp, 0, data, offset, sizeof(Single));
                        break;
                    }
                case TypeCode.Int32:
                    {
                        temp = BitConverter.GetBytes((Int32)fieldValue);
                        StructureTransform.Reverse(temp);
                        Array.Copy(temp, 0, data, offset, sizeof(Int32));
                        break;
                    }
                case TypeCode.UInt32:
                    {
                        temp = BitConverter.GetBytes((UInt32)fieldValue);
                        StructureTransform.Reverse(temp);
                        Array.Copy(temp, 0, data, offset, sizeof(UInt32));
                        break;
                    }
                case TypeCode.Int16:
                    {
                        temp = BitConverter.GetBytes((Int16)fieldValue);
                        StructureTransform.Reverse(temp);
                        Array.Copy(temp, 0, data, offset, sizeof(Int16));
                        break;
                    }
                case TypeCode.UInt16:
                    {
                        temp = BitConverter.GetBytes((UInt16)fieldValue);
                        StructureTransform.Reverse(temp);
                        Array.Copy(temp, 0, data, offset, sizeof(UInt16));
                        break;
                    }
                case TypeCode.Int64:
                    {
                        temp = BitConverter.GetBytes((Int64)fieldValue);
                        StructureTransform.Reverse(temp);
                        Array.Copy(temp, 0, data, offset, sizeof(Int64));
                        break;
                    }
                case TypeCode.UInt64:
                    {
                        temp = BitConverter.GetBytes((UInt64)fieldValue);
                        StructureTransform.Reverse(temp);
                        Array.Copy(temp, 0, data, offset, sizeof(UInt64));
                        break;
                    }
                case TypeCode.Double:
                    {
                        temp = BitConverter.GetBytes((Double)fieldValue);
                        StructureTransform.Reverse(temp);
                        Array.Copy(temp, 0, data, offset, sizeof(Double));
                        break;
                    }
                case TypeCode.Byte:
                    {
                        data[offset] = (Byte)fieldValue;
                        break;
                    }
                default:
                    {
                        //System.Diagnostics.Debug.Fail("No conversion provided for this type : " + typeCode.ToString());
                        break;
                    }
            }; // switch
            if (typeCode == TypeCode.Object)
            {
                int length = ((byte[])fieldValue).Length;
                Array.Copy(((byte[])fieldValue), 0, data, offset, length);
                offset += length;
            }
            else
            {
                offset += Marshal.SizeOf(fieldValue);
            }
        } // foreach

        return data;
    } // Swap
};

public class NFBinarySendLogic
{
    NFNet net;
    Hashtable mhtID;

    public NFBinarySendLogic(NFNet clientnet)
    {
        net = clientnet;
    }

    public void SendMsg(Int64 charID, NFMsg.EGameMsgID unMsgID, MemoryStream stream)
    {     
        NFMsg.MsgBase xData = new NFMsg.MsgBase();
        xData.player_id = charID;
        xData.msg_data = stream.ToArray();

        MemoryStream body = new MemoryStream();
        Serializer.Serialize<NFMsg.MsgBase>(body, xData);

        MsgHead head = new MsgHead();
        head.unMsgID = (UInt16)unMsgID;
        head.unDataLen = (UInt32)body.Length + (UInt32)ConstDefine.NF_PACKET_HEAD_SIZE;
        
        byte[] bodyByte = body.ToArray();
        byte[] headByte = StructureTransform.StructureToByteArrayEndian(head);


        byte[] sendBytes = new byte[head.unDataLen];
        headByte.CopyTo(sendBytes, 0);
        bodyByte.CopyTo(sendBytes, headByte.Length);

        net.client.SendBytes(sendBytes);

        string strTime = DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second;
        string strData = "S***:" + strTime + " MsgID:" + head.unMsgID + " Len:" + head.unDataLen;
        net.listener.aMsgList.Add(strData);
    }


    public void LoginPB(string strAccount, string strPassword, string strSessionKey)
    {
        NFMsg.ReqAccountLogin xData = new NFMsg.ReqAccountLogin();
        xData.account = System.Text.Encoding.Default.GetBytes(strAccount);
        xData.password = System.Text.Encoding.Default.GetBytes(strPassword);
        xData.security_code = System.Text.Encoding.Default.GetBytes(strSessionKey);
        xData.signBuff = System.Text.Encoding.Default.GetBytes("");
        xData.clientVersion = 1;
        xData.loginMode = 0;
        xData.clientIP = 0;
        xData.clientMAC = 0;
        xData.device_info = System.Text.Encoding.Default.GetBytes("");
        xData.extra_info = System.Text.Encoding.Default.GetBytes("");

        MemoryStream stream = new MemoryStream();
        Serializer.Serialize<NFMsg.ReqAccountLogin>(stream, xData);

        SendMsg(net.nSelfID, NFMsg.EGameMsgID.EGMI_REQ_LOGIN, stream);

        net.mPlayerState = NFNet.PLAYER_STATE.E_PLAYER_LOGINING;
    }

    public void RequireWorldList()
    {
        NFMsg.ReqServerList xData = new NFMsg.ReqServerList();
        xData.type = NFMsg.ReqServerListType.RSLT_WORLD_SERVER;

        MemoryStream stream = new MemoryStream();
        Serializer.Serialize<NFMsg.ReqServerList>(stream, xData);

        SendMsg(net.nSelfID, NFMsg.EGameMsgID.EGMI_REQ_WORLD_LIST, stream);
    }

    public void RequireConnectWorld(int nWorldID)
    {
        NFMsg.ReqConnectWorld xData = new NFMsg.ReqConnectWorld();
        xData.world_id = nWorldID;

        MemoryStream stream = new MemoryStream();
        Serializer.Serialize<NFMsg.ReqConnectWorld>(stream, xData);

        SendMsg(net.nSelfID, NFMsg.EGameMsgID.EGMI_REQ_CONNECT_WORLD, stream);
    }

    public void RequireVerifyWorldKey(string strAccount, string strKey)
    {
        NFMsg.ReqAccountLogin xData = new NFMsg.ReqAccountLogin();
        xData.account = System.Text.Encoding.Default.GetBytes(strAccount);
        xData.password = System.Text.Encoding.Default.GetBytes("");
        xData.security_code = System.Text.Encoding.Default.GetBytes(strKey);
        xData.signBuff = System.Text.Encoding.Default.GetBytes("");
        xData.clientVersion = 1;
        xData.loginMode = 0;
        xData.clientIP = 0;
        xData.clientMAC = 0;
        xData.device_info = System.Text.Encoding.Default.GetBytes("");
        xData.extra_info = System.Text.Encoding.Default.GetBytes("");

        MemoryStream stream = new MemoryStream();
        Serializer.Serialize<NFMsg.ReqAccountLogin>(stream, xData);

        SendMsg(net.nSelfID, NFMsg.EGameMsgID.EGMI_REQ_CONNECT_KEY, stream);
    }

    public void RequireServerList()
    {
        NFMsg.ReqServerList xData = new NFMsg.ReqServerList();
        xData.type = NFMsg.ReqServerListType.RSLT_GAMES_ERVER;

        MemoryStream stream = new MemoryStream();
        Serializer.Serialize<NFMsg.ReqServerList>(stream, xData);

        SendMsg(net.nSelfID, NFMsg.EGameMsgID.EGMI_REQ_WORLD_LIST, stream);
    }

    public void RequireSelectServer(int nServerID)
    {
        NFMsg.ReqSelectServer xData = new NFMsg.ReqSelectServer();
        xData.world_id = nServerID;

        MemoryStream stream = new MemoryStream();
        Serializer.Serialize<NFMsg.ReqSelectServer>(stream, xData);

        SendMsg(net.nSelfID, NFMsg.EGameMsgID.EGMI_REQ_SELECT_SERVER, stream);
    }

    public void RequireRoleList(string strAccount, int nGameID)
    {
        NFMsg.ReqRoleList xData = new NFMsg.ReqRoleList();
        xData.game_id = nGameID;
        xData.account = UnicodeEncoding.Default.GetBytes(strAccount);

        MemoryStream stream = new MemoryStream();
        Serializer.Serialize<NFMsg.ReqRoleList>(stream, xData);

        SendMsg(net.nSelfID, NFMsg.EGameMsgID.EGMI_REQ_ROLE_LIST, stream);
    }

    public void RequireCreateRole(string strAccount, string strRoleName, int byCareer, int bySex, int nGameID)
    {
        if (strRoleName.Length >= 20 || strRoleName.Length < 4)
        {
            return;
        }

        NFMsg.ReqCreateRole xData = new NFMsg.ReqCreateRole();
        xData.career = byCareer;
        xData.sex = bySex;
        xData.noob_name = UnicodeEncoding.Default.GetBytes(strRoleName);
        xData.account = UnicodeEncoding.Default.GetBytes(strAccount);
        xData.race = 0;
        xData.game_id = nGameID;

        MemoryStream stream = new MemoryStream();
        Serializer.Serialize<NFMsg.ReqCreateRole>(stream, xData);

        SendMsg(net.nSelfID, NFMsg.EGameMsgID.EGMI_REQ_CREATE_ROLE, stream);
    }

    public void RequireDelRole(string strAccount, string strRoleName, int nGameID)
    {
        NFMsg.ReqDeleteRole xData = new NFMsg.ReqDeleteRole();
        xData.name = UnicodeEncoding.Default.GetBytes(strRoleName);
        xData.account = UnicodeEncoding.Default.GetBytes(strAccount);
        xData.game_id = nGameID;

        MemoryStream stream = new MemoryStream();
        Serializer.Serialize<NFMsg.ReqDeleteRole>(stream, xData);

        SendMsg(net.nSelfID, NFMsg.EGameMsgID.EGMI_REQ_DELETE_ROLE, stream);
    }

    public void RequireEnterGameServer(string strAccount, string strRoleName, int nServerID)
    {
        NFMsg.ReqEnterGameServer xData = new NFMsg.ReqEnterGameServer();
        xData.name = UnicodeEncoding.Default.GetBytes(strRoleName);
        xData.account = UnicodeEncoding.Default.GetBytes(strAccount);
        xData.game_id = nServerID;

        MemoryStream stream = new MemoryStream();
        Serializer.Serialize<NFMsg.ReqEnterGameServer>(stream, xData);

        SendMsg(net.nSelfID, NFMsg.EGameMsgID.EGMI_REQ_ENTER_GAME, stream);
    }

    public void RequireHeartBeat()
    {

    }

    //有可能是他副本的NPC移动,因此增加64对象ID
    public void RequireMove(Int64 charID, Int64 objectID, float fX, float fZ)
    {


    }

    //有可能是他副本的NPC移动,因此增加64对象ID
    public void RequireSkill(Int64 charID, string strKillID, Int64 nTargetID)
    {


    }

    public void RequireItem(Int64 charID, string strItem, uint nCount)
    {


    }

    public void RequireChat(Int64 charID, UInt32 targetID, int nType, string strData)
    {


    }

    public void RequireSwapEquip(Int64 charID, uint nOriginRow, uint nTargetRow)
    {


    }

    public void RequireSwapScene(Int64 charID, Int32 nTransferType, UInt32 nSceneID, int nLineIndex)
    {
    }

    public void RequireProperty(Int64 charID, string strPropertyName, int nValue)
    {

    }

    public void RequireAcceptTask(Int64 charID, Int64 nTargetID, string strTaskID)
    {

    }

    public void RequireCompeleteTask(Int64 charID, Int64 nTargetID, string strTaskID)
    {

    }

    public void RequirePullDownCustom(Int64 charID, int nResult)
    {

    }

    public void RequirePickUpItem(Int64 charID, string strItemID, Int64 nNPCID, int nRow)
    {
    }

    public void ReqProduceItem(Int64 charID, string strItemID)
    {

    }

    public void ReqDestroyItem(Int64 charID, Int64 nID, Int32 nRow)
    {

    }
}