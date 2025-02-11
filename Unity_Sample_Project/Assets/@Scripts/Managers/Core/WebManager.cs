using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class CertificateWhore : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        return true;
    }
}

public class WebManager
{
    public string BaseUrl { get; set; }

    public string ip = "127.0.0.1";
    public int port = 7777;

    public void Init()
    {
        IPAddress ipv4 = Util.GetIpv4Address(ip);
        if (ipv4 == null)
        {
            Debug.LogError("WebServer IPv4 Failed");
            return;
        }

        BaseUrl = $"https://{ipv4.ToString()}:{port}";
        Debug.Log($"WebServer BaseUrl : {BaseUrl}");
    }

    public void SendPostRequest<T>(string url, object obj, Action<T> res)
    {
        Managers.Instance.StartCoroutine(CoSendWebRequest(url, UnityWebRequest.kHttpVerbPOST, obj, res));
    }

    public void SendGetRequest<T>(string url, object obj, Action<T> res)
    {
        Managers.Instance.StartCoroutine(CoSendWebRequest(url, UnityWebRequest.kHttpVerbGET, obj, res));
    }

    IEnumerator CoSendWebRequest<T>(string url, string method, object obj, Action<T> res)
    {
        if (string.IsNullOrEmpty(BaseUrl))
            Init();

        string sendUrl = $"{BaseUrl}/{url}";

        byte[] jsonBytes = null;
        if (obj != null)
        {
            string jsonStr = JsonUtility.ToJson(obj);
            jsonBytes = Encoding.UTF8.GetBytes(jsonStr);
        }

        // 리소스 자동 해제를 위한 using
        // 정확히는 주로 IDisposable 인터페이스를 구현하는 객체에 대해 사용
        // -> 명시적인 해제 (더 이상 객체가 필요하지 않을때 Dispose 메서드를 호출)
        // -> 정확한 해제 시점 (using은 해당 블록이 끝나면 리소스 해제를 보장하므로, 네트워크 연결, DB 연결 등에 사용)
        using (var uwr = new UnityWebRequest(sendUrl, method))
        {
            // 유니티용 web 통신용
            uwr.uploadHandler = new UploadHandlerRaw(jsonBytes);
            uwr.downloadHandler = new DownloadHandlerBuffer();
            uwr.certificateHandler = new CertificateWhore(); // https 에 필요 (임시)
            uwr.SetRequestHeader("Content-Type", "application/json");

            // 서버에서 답 올때까지 대기
            yield return uwr.SendWebRequest();

            // 서버에서 답이 온 이후의 상황
            if (uwr.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log($"CoSendWebRequest Failed : {uwr.error}");
            }
            else
            {
                // json 파싱하여
                T resObj = JsonUtility.FromJson<T>(uwr.downloadHandler.text);
                // action 에 알려준다
                res.Invoke(resObj);
            }
        }
    }
}
