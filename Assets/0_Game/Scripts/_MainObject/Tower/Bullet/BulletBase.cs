using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.TextCore.Text;

public abstract class BulletBase : GameUnit
{
    protected Tower owner;
    protected Transform target;
    protected float damage;
    public float speed = 30f;
    public BulletType blType;
    private float startTime = 0f;
    private float endTime = 2f;
    public virtual void Initialize(Tower shooter, Transform target, float damage)
    {
        this.owner = shooter;
        this.target = target;
        this.damage = damage;
    }
    public void Update()
    {
        transform.Translate(Vector3.up * Time.deltaTime * speed, Space.Self);
        startTime += Time.deltaTime; // update time each frame

        if (startTime >= endTime) // check time 
        {
            OnDespawn();
        }
    }
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bot"))
        {
            Bot bot = Cache.GetBot(other);

            if (owner == null) return;
            if (bot == null) return;
            if (bot != null && bot.isDespawn == false)
            {
                bot.TakeDamage(10f);
                OnDespawn();
            }
            else
            {
                OnDespawn();
            }
        }
    }
    public void OnDespawn()
    {
        if (owner != null)
        {
            SimplePool.Despawn(this);
        }
    }
}
