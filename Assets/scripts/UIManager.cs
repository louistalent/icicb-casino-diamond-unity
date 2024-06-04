using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnitySocketIO;
using UnitySocketIO.Events;
using System.Runtime.InteropServices;
using SimpleJSON;

public class UIManager : MonoBehaviour
{
    public TMP_Text info_Text;
    public TMP_Text walletAmount_Text;

    public TMP_InputField AmountField;

    public SocketIOController io;

    public Button BetBtn;

    private bool connectedToServer = false;
    public Texture[] diamondImages = new Texture[7];
    public GameObject[] benefitPanels = new GameObject[7];
    public GameObject[] diamondObjects = new GameObject[5];

    BetPlayer _player;

    // GameReadyStatus Send
    [DllImport("__Internal")]
    private static extern void GameReady(string msg);
    // Start is called before the first frame update
    void Start()
    {
        info_Text.text = "";
        AmountField.text = "10.0";
        _player = new BetPlayer();
        io.Connect();

        io.On("connect", (e) =>
        {
            connectedToServer = true;
            Debug.Log("Game started");

            io.On("game result", (res) =>
            {
                StartCoroutine(GameResult(res));
            });

            io.On("error message", (res) =>
            {
                ShowError(res);
            });
        });

        #if UNITY_WEBGL == true && UNITY_EDITOR == false
            GameReady("Ready");
        #endif
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ShowError(SocketIOEvent socketIOEvent)
    {
        var res = ReceiveJsonObject.CreateFromJSON(socketIOEvent.data);
        info_Text.text = res.errorMessage.ToString();
        BetBtn.interactable = true;
    }
    public void RequestToken(string data)
    {
        JSONNode usersInfo = JSON.Parse(data);
        Debug.Log("token=--------" + usersInfo["token"]);
        Debug.Log("amount=------------" + usersInfo["amount"]);
        Debug.Log("userName=------------" + usersInfo["userName"]);
        _player.token = usersInfo["token"];
        _player.username = usersInfo["userName"];

        float i_balance = float.Parse(usersInfo["amount"]);
        walletAmount_Text.text = i_balance.ToString("F3");
    }

    IEnumerator GameResult(SocketIOEvent socketIOEvent)
    {
        var res = ReceiveJsonObject.CreateFromJSON(socketIOEvent.data);
        StartCoroutine(UnvisibleDiamondGameObjects());
        StartCoroutine(SetDiamondImages(res.randomArray));
        yield return new WaitForSeconds(2.5f);
        BetBtn.interactable = true;
        SetGameResultPanel(res.amountCross);
        walletAmount_Text.text = res.amount.ToString("F2");
        if(res.amountCross !=0)
            info_Text.text = "You Win! You Earned " + res.earnAmount.ToString("F3");
        else if (res.amountCross==0)
            info_Text.text = "You Lose!";

    }

    public void MinBtn_Clicked()
    {
        AmountField.text = "10.0";
    }

    public void CrossBtn_Clicked()
    {
        float amount = float.Parse(AmountField.text);
        if (amount >= 100000f)
            AmountField.text = "100000.0";
        else
            AmountField.text = (amount * 2.0f).ToString("F2");
    }

    public void HalfBtn_Clicked()
    {
        float amount = float.Parse(AmountField.text);
        if (amount <= 10f)
            AmountField.text = "10.0";
        else
            AmountField.text = (amount / 2.0f).ToString("F2");
    }

    public void MaxBtn_Clicked()
    {
        float myTotalAmount = float.Parse(string.IsNullOrEmpty(walletAmount_Text.text) ? "0" : walletAmount_Text.text);
        if (myTotalAmount >= 100000f)
            AmountField.text = "100000.0";
        else if (myTotalAmount >= 10f && myTotalAmount < 100000f)
            AmountField.text = myTotalAmount.ToString("F2");
    }

    public void AmountField_Changed()
    {
        if (float.Parse(AmountField.text) < 10f)
            AmountField.text = "10.0";
        else if (float.Parse(AmountField.text) > 100000f)
        {
            AmountField.text = "100000.0";
        }
    }

    public void BetBtnClicked()
    {
        if (connectedToServer)
        {
            SetBenefitPanel_InitColor();
            info_Text.text = "";
            JsonType JObject = new JsonType();
            float myTotalAmount = float.Parse(string.IsNullOrEmpty(walletAmount_Text.text) ? "0" : walletAmount_Text.text);
            float betamount = float.Parse(string.IsNullOrEmpty(AmountField.text) ? "0" : AmountField.text);
            if (betamount <= myTotalAmount)
            {
                JObject.userName = _player.username;
                JObject.betAmount = betamount;
                JObject.token = _player.token;
                JObject.amount = myTotalAmount;
                io.Emit("bet info", JsonUtility.ToJson(JObject));
                Debug.Log(JsonUtility.ToJson(JObject));
                BetBtn.interactable = false;
            }
            else
                info_Text.text = "Not enough Funds";
        }
        else
            info_Text.text = "Can't connect to Game Server!";
        
    }

    IEnumerator UnvisibleDiamondGameObjects()
    {
        for (int i = 0; i < 5; i++)
            diamondObjects[i].SetActive(false);
        yield return new WaitForSeconds(1f);
    }

    IEnumerator SetDiamondImages(int[] randomArray)
    {
        for(int i = 0; i < 5; i++)
        {
            diamondObjects[i].SetActive(true);
            diamondObjects[i].GetComponent<RawImage>().texture = diamondImages[randomArray[i]];
            yield return new WaitForSeconds(0.5f);
        }
    }

    void SetBenefitPanel_InitColor()
    {
        for(int i = 0; i < 7; i++)
        {
            benefitPanels[i].GetComponent<Image>().color = new Color(255,255,255,0.07f);
        }
    }

    void SetGameResultPanel(float index)
    {
        if(index==0f)
            benefitPanels[0].GetComponent<Image>().color = new Color(0, 255, 125, 1);
        else if(index==0.1f)
            benefitPanels[1].GetComponent<Image>().color = new Color(0, 255, 125, 1);
        else if (index == 2f)
            benefitPanels[2].GetComponent<Image>().color = new Color(0, 255, 125, 1);
        else if (index == 3f)
            benefitPanels[3].GetComponent<Image>().color = new Color(0, 255, 125, 1);
        else if (index == 4f)
            benefitPanels[4].GetComponent<Image>().color = new Color(0, 255, 125, 1);
        else if (index == 5f)
            benefitPanels[5].GetComponent<Image>().color = new Color(0, 255, 125, 1);
        else if (index == 50f)
            benefitPanels[6].GetComponent<Image>().color = new Color(0, 255, 125, 1);
    }


}

public class BetPlayer
{
    public string username;
    public string token;
}
