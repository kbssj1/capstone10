﻿using UnityEngine;
using System.Collections;
using System.Net;

public static class BaseNetwork {

    public static string GetMyIp()
    {
        IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
        if(hostEntry.AddressList[2].AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
            return hostEntry.AddressList[0].ToString();
        else
            return hostEntry.AddressList[2].ToString();
    }

    public static void Base(ref WWWForm _form)
    {
        string _hash_origin = "love";
        string _hash = Md5Sum(_hash_origin);

        _form.AddField("hash", _hash);
    }

    private static string Md5Sum(string strToEncrypt)
    {
        System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
        byte[] bytes = ue.GetBytes(strToEncrypt);

        // encrypt bytes
        System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        byte[] hashBytes = md5.ComputeHash(bytes);

        // Convert the encrypted bytes back to a string (base 16)
        string hashString = "";

        for (int i = 0; i < hashBytes.Length; i++)
        {
            hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
        }

        return hashString.PadLeft(32, '0');
    }
}
