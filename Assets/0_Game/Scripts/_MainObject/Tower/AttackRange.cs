using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackRange : MonoBehaviour
{
    public Tower owner;

    // Khi bot đi vào vùng tầm bắn, ta sẽ duyệt danh sách bot (theo thứ tự trong list)
    // và chọn bot đầu tiên có khoảng cách từ tower <= owner.range.
    //private void OnTriggerEnter2D(Collider2D other)
    //{
    //    if (other.CompareTag("Bot"))
    //    {
    //        Bot bot = Cache.GetBot(other);
    //        List<Bot> bots = MapManager.Ins.listBot;
    //        foreach (Bot b in bots)
    //        {
    //            // Kiểm tra khoảng cách từ tower đến bot
    //            if (Vector3.Distance(owner.transform.position, b.transform.position) <= owner.range)
    //            {
    //                owner.TargetBot(b);
    //                break;
    //            }
    //        }
    //    }
    //}

    //private void OnTriggerStay2D(Collider2D other)
    //{
    //    if (other.CompareTag("Bot"))
    //    {
    //        // Nếu target của owner đã bị loại (hoặc ra khỏi tầm), ta duyệt lại list bot
    //        if (owner.target == null || Vector3.Distance(owner.transform.position, owner.target.transform.position) > owner.range)
    //        {
    //            List<Bot> bots = MapManager.Ins.listBot;
    //            foreach (Bot b in bots)
    //            {
    //                if (Vector3.Distance(owner.transform.position, b.transform.position) <= owner.range)
    //                {
    //                    owner.TargetBot(b);
    //                    break;
    //                }
    //            }
    //        }
    //    }
    //}

    //private void OnTriggerExit2D(Collider2D other)
    //{
    //    if (other.CompareTag("Bot"))
    //    {
    //        Bot bot = Cache.GetBot(other);
    //        if (owner.target == bot)
    //        {
    //            owner.RemoveTarget(bot);
    //        }
    //    }
    //}
}
