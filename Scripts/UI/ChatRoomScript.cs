using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatRoom : MonoBehaviour
{

    public TMP_InputField inputField;       
    public Transform messagesContainer;      
    public GameObject messagePrefab;        

    private void Awake()
    {
        inputField = transform.Find("MessOfLocalPlayer").GetComponent<TMP_InputField>();
        messagesContainer = transform.Find("MessOfRoom");
    }

    private void Start()
    {
        if (inputField != null)
        {
            inputField.onEndEdit.AddListener(OnSendMessage);
        }
        else
        {
            Debug.LogError("Input field reference not found in MessOfLocalPlayer!");
        }
    }

    private void OnSendMessage(string message)
    {
        if (string.IsNullOrEmpty(message)) return;
        
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
         
            string senderName = UIManager.Instance.playerNameInput.text;
            NetworkManager.Instance.SendChatMessage(senderName, message);
            
            inputField.text = "";
            inputField.ActivateInputField();
        }
    }

    public void AddMessage(string senderName, string message)
    {
        if (messagesContainer == null || messagePrefab == null) return;

        GameObject messageObj = Instantiate(messagePrefab, messagesContainer);
        TextMeshProUGUI messageText = messageObj.GetComponent<TextMeshProUGUI>();
        messageText.text = $"{senderName}: {message}";

        Canvas.ForceUpdateCanvases();
 
    }

    private void OnDestroy()
    {
        if (inputField != null)
        {
            inputField.onEndEdit.RemoveListener(OnSendMessage);
        }
    }
}