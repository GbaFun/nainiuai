using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

public static class HttpUtil
{
    /// <summary>
    /// 发送 GET 请求
    /// </summary>
    /// <param name="url">请求的 URL</param>
    /// <param name="headers">请求头</param>
    /// <returns>响应内容</returns>
    public static string Get(string url, Dictionary<string, string> headers = null)
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = "GET";

        if (headers != null)
        {
            foreach (var header in headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }
        }

        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
        {
            return reader.ReadToEnd();
        }
    }

    /// <summary>
    /// 发送 POST 请求
    /// </summary>
    /// <param name="url">请求的 URL</param>
    /// <param name="data">请求的数据</param>
    /// <param name="headers">请求头</param>
    /// <returns>响应内容</returns>
    public static string Post(string url, string data, Dictionary<string, string> headers = null)
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = "POST";
        request.ContentType = "application/x-www-form-urlencoded";

        if (headers != null)
        {
            foreach (var header in headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }
        }

        byte[] byteArray = Encoding.UTF8.GetBytes(data);
        request.ContentLength = byteArray.Length;

        using (Stream dataStream = request.GetRequestStream())
        {
            dataStream.Write(byteArray, 0, byteArray.Length);
        }

        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
        {
            return reader.ReadToEnd();
        }
    }

    /// <summary>
    /// 发送 POST 请求，数据为 JSON 格式
    /// </summary>
    /// <param name="url">请求的 URL</param>
    /// <param name="jsonData">请求的 JSON 数据</param>
    /// <param name="headers">请求头</param>
    /// <returns>响应内容</returns>
    public static string PostJson(string url, string jsonData, Dictionary<string, string> headers = null)
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = "POST";
        request.ContentType = "application/json";

        if (headers != null)
        {
            foreach (var header in headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }
        }

        byte[] byteArray = Encoding.UTF8.GetBytes(jsonData);
        request.ContentLength = byteArray.Length;

        using (Stream dataStream = request.GetRequestStream())
        {
            dataStream.Write(byteArray, 0, byteArray.Length);
        }

        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
        {
            return reader.ReadToEnd();
        }
    }
}