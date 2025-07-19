using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class WebHookDiscord : MonoBehaviour
{
    // 여기에 당신의 웹훅 URL을 입력
    private const string WebhookUrl = "https://discord.com/api/webhooks/1360623729884926234/8SbVz9zylaP1Zx2MK3VYjNldb1fZbH-ImMQj4ZB9JNyl-h3S5IPsEyzIV5gqo0DTYjDn";

    // 메시지 전송 함수
    public void SendToDiscord(string message)
    {
        StartCoroutine(SendWebhook(message));
    }

    private IEnumerator SendWebhook(string content)
    {
        var payload = new DiscordWebhookMessage { content = content };

        string jsonPayload = JsonUtility.ToJson(payload);

        using (UnityWebRequest req = new UnityWebRequest(WebhookUrl, "POST"))
        {
            byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(jsonPayload);
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[DiscordWebhook] Error sending message: {req.error}");
            }
            else
            {
                Debug.Log("[DiscordWebhook] Message sent successfully.");
            }
        }
    }

    // 메시지 포맷용 클래스
    [System.Serializable]
    private class DiscordWebhookMessage
    {
        public string content;
    }
}
