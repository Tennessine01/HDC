using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.TextCore.Text;

public abstract class BulletBase : GameUnit
{
    protected Tower owner;
    protected Transform target;
    protected float damage;
    public float speed = 10f;
    public BulletType blType;
    public virtual void Initialize(Tower shooter, Transform target, float damage)
    {
        this.owner = shooter;
        this.target = target;
        this.damage = damage;
    }
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(Constants.TAG_BOT))
        {
            Bot TargetBot = collision.GetComponent<Bot>();
        }
    }

}
