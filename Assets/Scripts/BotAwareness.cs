using UnityEngine;

public class BotAwareness : MonoBehaviour
{
    public BotController bot;

    void OnTriggerStay(Collider other)
    {
        if (!other.gameObject.CompareTag("Edible")) return;

        float dist = Vector3.Distance(bot.transform.position, other.transform.position);

        if (BotController.GetScale(other.gameObject) >= BotController.GetScale(bot.gameObject))
        {
            // Debug.Log("[bot] threat");
            // if (Random.Range(0, 100) == 0)
            //     Debug.Log($"[bot] threat current {dist} vs closest {bot.closestThreatDist}");
            if (dist < bot.closestThreatDist)
            {
                bot.closestThreatDist = dist;
                bot.threat = other.gameObject;
            }
        }
        else
        {
            // Debug.Log("[bot] target");
            if (dist < bot.closestTargetDist)
            {
                bot.closestTargetDist = dist;
                bot.target = other.gameObject;
            }
        }
    }
}
