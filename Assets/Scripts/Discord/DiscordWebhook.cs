using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DiscordWebhook : MonoBehaviour
{
    // DOCU
    //
    // Attach this script to a game object.
    // This script will send a message to the discord server whenever the "play" is selected.
    //

    /*
    string webhook_link = "https://discord.com/api/webhooks/956633672600137878/TX4FtA8g8Y7fp6EkBVn-YsE7VfYzKF43M33xUN6U1JPaxB_yJAdPYQqmyjb21saRxN3X";

    void Start()
    {
        StartCoroutine(SendWebhook(webhook_link, "Some Message To the Server", (success) =>
        {
            if (success)
                Debug.Log("Message Sent");
        }));
    }

    IEnumerator SendWebhook(string link, string message, System.Action<bool> action)
    {
        WWWForm form = new WWWForm();
        form.AddField("content", message);
        using (UnityWebRequest www = UnityWebRequest.Post(link, form))
        {
            yield return www.SendWebRequest();

            if(www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
                action(false);
            }
            else
            {
                action(true);
            }

        }
    }
    */
}
