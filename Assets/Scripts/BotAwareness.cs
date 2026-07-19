using UnityEngine;

public class BotAwareness : MonoBehaviour
{
    public BotController bot;

    void OnTriggerStay(Collider other)
    {
        if (!other.gameObject.CompareTag("Edible")) return;

        float dist = Vector3.Distance(bot.transform.position, other.transform.position);

        if (BotController.GetBoxScale(other.gameObject) >= BotController.GetBoxScale(bot.gameObject))
        {
            bool sentient = other.TryGetComponent<BotController>(out _) || other.TryGetComponent<Player>(out _);
            if (!sentient) return;

            if (dist < bot.closestThreatDist)
            {
                bot.closestThreatDist = dist;
                bot.threat = other.gameObject;
            }
        }
        else
        {
            if (dist < bot.closestTargetDist)
            {
                bot.closestTargetDist = dist;
                bot.target = other.gameObject;
            }
        }
    }
}
