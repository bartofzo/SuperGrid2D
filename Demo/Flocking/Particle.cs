using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SuperGrid2D;

public abstract class Particle : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    private Vector2 prevPos;
    private Vector2 prevVel;

    public bool HandledAction;

    public virtual void UpdateParticle()
    {
        prevPos = Position;
        prevVel = Velocity;
    }

    public abstract IConvex2D Shape { get; }
    public int Key { get; private set; }

    public Vector2 Position;
    public Vector2 Velocity;

    protected virtual void Start()
    {
        this.Key = Flocking.RegisterParticle(this);

        // Choose random starting position velocity and accel
        this.Position = Camera.main.ScreenToWorldPoint(new Vector2(Camera.main.pixelWidth * Random.value, Camera.main.pixelHeight * Random.value));
        Velocity = Vector2.ClampMagnitude(new Vector2(Random.value, Random.value), Flocking.MaxVelocity);

        this.transform.position = Position;
    }

    protected void BounceEdge()
    {
        if (Position.x < Flocking.TopLeft.x)
        {
            Position.x = Flocking.TopLeft.x;
            Velocity.x = -Velocity.x;
        }
        else if (Position.x > Flocking.BottomRight.x)
        {
            Position.x = Flocking.BottomRight.x;
            Velocity.x = -Velocity.x;
        }

        if (Position.y < Flocking.TopLeft.y)
        {
            Position.y = Flocking.TopLeft.y;
            Velocity.y = -Velocity.y;
        }
        else if (Position.y > Flocking.BottomRight.y)
        {
            Position.y = Flocking.BottomRight.y;
            Velocity.y = -Velocity.y;
        }
    }

    protected virtual void Update()
    {
        Vector2 smoothPosition = Flocking.SmoothVector(prevPos, Position);
        Vector2 smoothVelocity = Flocking.SmoothVector(prevVel, Velocity);

        this.transform.SetPositionAndRotation(smoothPosition, Quaternion.Euler(0, 0, -90 + Mathf.Rad2Deg * Mathf.Atan2(smoothVelocity.y, smoothVelocity.x)));
    }
}
