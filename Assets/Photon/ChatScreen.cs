using UnityEngine;
using System.Collections;

using System.Collections.Generic;
public class ChatScreen : MonoBehaviour {
    
    private Game engine;
    public GameObject gameManager_GObj;
    public GUISkin SkinChat;
    public string TextMessage;
    public string lastmessage = "lastmessage";
    public bool Pause = false;
    
    public GUIStyle TextChatStyle;
    public GUIStyle boxStyle;
    public GUIStyle textFieldStyle;
    GUIStyle createStyle;
    private Vector2 chatScroll;
    public string NameSender;
    public static bool chat = false;
    internal const byte EV_CHAT = 105;
    internal const byte CHAT_MESSAGE = 64;
	
    public List<string> TextChat = new List<string>();
	private string pending;
	private bool pass1;
	private bool flag2 = false;
	private float nextChange;
	private bool init = true;
 

	void Start () 
    {
		TextChatStyle = SkinChat.GetStyle("TextChat");
		nextChange = Time.time + 0.1f;
        boxStyle = SkinChat.GetStyle("box");
        textFieldStyle = SkinChat.GetStyle("textField");
        createStyle = SkinChat.GetStyle("Create");
    }

    public void Initialize(Game engine)
    {
         this.engine = engine;
		 this.init = true;		 
    }

    public void IncomingMessage(Hashtable _hashtable)
    {
       
        string message = (string)_hashtable[CHAT_MESSAGE];
        TextChat.Add(message);
        chatScroll = new Vector2(0, TextChat.Count * 19);
    }
 

    void SendingMessage(string message)
    {           
        // block repeated messages  (TODO: for a certain time?)
        if (lastmessage == message)
            return;

        if (message != string.Empty)
        {
            pending = NameSender + ": " + message;
            Hashtable evInfo = new Hashtable();
            evInfo.Add((System.Object)CHAT_MESSAGE, (string)NameSender + ": " + message);
            engine.loadBalancingPeer.OpRaiseEvent(EV_CHAT, evInfo, true, 0);
            lastmessage = NameSender + ": " + message;
        }
        
    }

    void StatusPauseChat(bool _pause)
    {  
		flag2 = chat;
        chat = _pause;				
    }
	
	void StatusPause(bool _pause)
	{	
		Pause = _pause;
	}

    void Update()
    {
        if (!Pause && !chat)
				{					
					if (this.init)
					{
						gameManager_GObj.SendMessage("SetChat", false);
						chat = false;
						flag2 = false;                    
						TextMessage = string.Empty;	
						nextChange = Time.time + 0.1f;        
						this.init = false;
					}					
					else if(Input.GetKeyUp(KeyCode.KeypadEnter) || Input.GetKeyUp(KeyCode.Return))
					{
						if (nextChange > Time.time) return;
						gameManager_GObj.SendMessage("SetChat", true);
						chat = true;
						flag2 = true;
						this.nextChange = Time.time + 0.1f;
					}
				}			
    }
 
 	void OnGUI () 
    {


        if (!Pause)
        {			
            GUILayout.BeginArea(new Rect((int)((Screen.width -340)), (int)(Screen.height - 90), 340, 80));
			
		
			pass1 = !pass1;
			if (pass1)
			{
				if (pending != null) 
				{
					TextChat.Add(pending);
					pending = null;
                    chatScroll = new Vector2(0, TextChat.Count * 19);
				}			
	
			}
			
			if (TextChat.Count > 0) 
			{	
		      	GUILayout.BeginArea(new Rect(0, 0, 340, 45), boxStyle);	
					
			    DisplayMessages();
	            GUILayout.EndScrollView();
	            GUILayout.EndArea(); 	
			}

            GUILayout.BeginArea(new Rect(0, 50, 340, 60));
            GUILayout.BeginHorizontal();
                       
            
            if (chat)
            {
                DisplaySenderMessage();
            }
			else if (flag2 != chat)
			{
				SetFocus();
				flag2 = chat;
			}		
			else
			{
				GUILayout.BeginArea(new Rect(120, 0, 340, 100));
				GUILayout.Label("Press 'Enter' to start chat", createStyle);
				GUILayout.EndArea();
			}
        		
            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            GUILayout.EndArea();

        }
    }
        
        void SetFocus()
        {
            if (chat)
            {
                GUI.FocusControl("ChatField");
            }
            else
            {
                GUI.FocusControl("Null");
            }
        }

    void DisplayMessages()
    {
            chatScroll = GUILayout.BeginScrollView(chatScroll, GUILayout.Width(330));			
			
            if (TextChat.Count <= 0) return;     
            for(int i = 0; i<TextChat.Count; i++)
            {
				string textChat = TextChat[i];
                GUILayout.Label(textChat, TextChatStyle);				
            }
    }

    void DisplaySenderMessage()
    {
                GUI.SetNextControlName("ChatField"); 
                TextMessage = GUILayout.TextField(TextMessage, textFieldStyle,GUILayout.Width(260));
                GUI.SetNextControlName("Null");

				if (chat)
				{					
                if (
					GUILayout.Button("Send", createStyle, GUILayout.Width(50)) 
					|| Input.GetKeyUp(KeyCode.KeypadEnter) 
					|| Input.GetKeyUp(KeyCode.Return)
					)
					{
						if (nextChange > Time.time) return;
                        SendingMessage(TextMessage);
					    gameManager_GObj.SendMessage("SetChat", false);
						chat = false;
						flag2 = false;                    
						TextMessage = string.Empty;						
						this.nextChange = Time.time + 0.1f;
					}					
				}
		
                SetFocus();
    }
}
