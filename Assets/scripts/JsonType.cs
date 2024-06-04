using UnityEngine;
using System.Collections;

public class JsonType
{
    public string userName;
    public float betAmount;
    public bool isBetting;
    public string token;
    public float amount;
}

public class ReceiveJsonObject
{
    public double amount;
    public float amountCross;
    public float earnAmount;
    public int[] randomArray;
    public string errorMessage;
    public ReceiveJsonObject()
    {
    }
    public static ReceiveJsonObject CreateFromJSON(string data)
    {
        return JsonUtility.FromJson<ReceiveJsonObject>(data);
    }
}