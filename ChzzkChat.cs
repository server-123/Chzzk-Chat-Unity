using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using WebSocketSharp;

[System.Serializable]
public class live_status
{
    public int code;
    public string message;
    public status_content content;

    [System.Serializable]
    public class status_content
    {
        public string liveTitle;
        public string status;
        public int concurrentUserCount;
        public int accumulateCount;
        public bool paidPromotion;
        public bool adult;
        public string chatChannelId;
        public string categoryType;
        public string liveCategory;
        public string liveCategoryValue;
        public string livePollingStatusJson;
        public string faultStatus;
        public string userAdultStatus;
        public bool chatActive;
        public string chatAvailableGroup;
        public string chatAvailableCondition;
        public int minFollowerMinute;
    }
}

[System.Serializable]
public class chat
{
    public int code;
    public string message;
    public chat_content content;

    [System.Serializable]
    public class chat_content
    {
        public string accessToken;
        [Serializable]
        public class TemporaryRestrict
        {
            public bool temporaryRestrict;
            public int times;
            public int duration;
            public int createdTime;
        }
        public bool realNameAuth;
        public string extraToken;
    }
}

[System.Serializable]
public class receivedData
{
    public string svcid;
    public string ver;
    public Body[] bdy;
    public int cmd;
    public string tid;
    public string cid;

    [System.Serializable]
    public class Body
    {
        public string svcid;
        public string cid;
        public int mbrCnt;
        public string uid;
        public string profile;
        public string msg;
        public int msgTypeCode;
        public string msgStatusType;
        public string extras;
        public int ctime;
        public int utime;
        public string msgTid;
        public int msgTime;
    }
}

public class Profile
{
    public string userIdHash;
    public string nickname;
    public string profileImageUrl;
    public string userRoleCode;
    public string badge;
    public string title;
    public bool verifiedMark;
    public Badge[] activityBadges;
    public StreamingProperty streamingProperty;

    [System.Serializable]
    public class Badge
    {
        public int badgeNo;
        public string badgeId;
        public string imageUrl;
        public string title;
        public string description;
        public bool activated;
    }

    [System.Serializable]
    public class StreamingProperty
    {
        public Subscription subscription;

        [System.Serializable]
        public class Subscription
        {
            public int accumulativeMonth;
            public int tier;
            public Badge badge;
        }
    }
}

[System.Serializable]
public class Donation
{
    public string emojis;
    public bool isAnonymous;
    public string payType;
    public int payAmount;
    public string streamingChannelId;
    public string nickname;
    public string osType;
    public string donationType;
    public donationRank[] weeklyRankList;
    public donationRank donationUserWeeklyRank;
    public string chatType;

    [System.Serializable]
    public class donationRank
    {
        public string userIdHash;
        public string nickName;
        public bool verifiedMark;
        public int donationAmount;
        public int ranking;
    }
}

public class ChzzkChat : MonoBehaviour
{
    enum SslProtocolsHack
    {
        Tls = 192,
        Tls11 = 768,
        Tls12 = 3072
    }
    
    public bool stopConnect = false;

    public live_status status;
    public chat chat;

    public string channelId;
    public string chatChannelId;
    public string accessToken;

    WebSocket ws;
    public List<receivedData> Data;

    string heartbeatRequest = "{\"ver\":\"2\",\"cmd\":0}";
    string heartbeatResponse = "{\"ver\":\"2\",\"cmd\":10000}";

    void OnEnable()
    {
        stopConnect = false;
        Data = new List<receivedData>();
        if (!stopConnect) StartCoroutine(GetChat());
    }

    void OnDisable()
    {
        stopConnect = true;
        Disconncect();
    }

    IEnumerator HeartBeat()
    {
        while (!stopConnect)
        {
            yield return new WaitForSecondsRealtime(15);

            if (ws != null && ws.IsAlive)
            {
                ws.Send(heartbeatRequest);
            }
        }
    }

    IEnumerator GetChat()
    {
        string apiUrl = $"https://api.chzzk.naver.com/polling/v2/channels/{channelId}/live-status";

        using (UnityWebRequest www = UnityWebRequest.Get(apiUrl))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                status = JsonUtility.FromJson<live_status>(www.downloadHandler.text);
                chatChannelId = status.content.chatChannelId;
                yield return GetAccessToken();
            }
            else Debug.Log(www.error);
        }
    }

    IEnumerator GetAccessToken()
    {
        string apiUrl = $"https://comm-api.game.naver.com/nng_main/v1/chats/access-token?channelId={chatChannelId}&chatType=STREAMING";

        using (UnityWebRequest www = UnityWebRequest.Get(apiUrl))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                chat = JsonUtility.FromJson<chat>(www.downloadHandler.text);
                accessToken = chat.content.accessToken;

                if (!stopConnect) Connect();
            }
            else Debug.Log(www.error);
        }
    }

    public void Connect()
    {
        string msg = "{\"ver\":\"2\",\"cmd\":100,\"svcid\":\"game\",\"cid\":\"" + chatChannelId + "\",\"bdy\":{\"uid\":null,\"devType\":2001,\"accTkn\":\"" + accessToken + "\",\"auth\":\"READ\"},\"tid\":1}";

        ws = new WebSocket("wss://kr-ss1.chat.naver.com/chat");
        var sslProtocolHack = (System.Security.Authentication.SslProtocols)(SslProtocolsHack.Tls12 | SslProtocolsHack.Tls11 | SslProtocolsHack.Tls);
        ws.SslConfiguration.EnabledSslProtocols = sslProtocolHack;

        ws.OnMessage += ws_OnMessage;
        ws.OnOpen += ws_OnOpen;
        ws.OnClose += ws_OnClose;
        ws.Connect();
        ws.Send(msg);

        StartCoroutine(HeartBeat());
    }

    public void Disconncect()
    {
        try
        {
            if (ws == null) return;
            if (ws.IsAlive)
            {
                ws.Close();
                stopConnect = false;
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    void ws_OnMessage(object sender, MessageEventArgs e)
    {
        receivedData d = JsonUtility.FromJson<receivedData>(e.Data);

        if (d.cmd == 0)
        {
            ws.Send(heartbeatResponse);
        }

        if (d.ver == "1") Data.Add(d);
    }

    void ws_OnOpen(object sender, System.EventArgs e)
    {
        Debug.Log("open");
    }

    void ws_OnClose(object sender, CloseEventArgs e)
    {
        if (!stopConnect) Connect();
    }
}
