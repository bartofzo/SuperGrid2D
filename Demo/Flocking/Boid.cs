using SuperGrid2D;
using UnityEngine;

public class Boid : Particle
{


    public override IConvex2D Shape => new Point(this.Position);




    protected override void Start()
    {
        base.Start();
    }

    public override void UpdateParticle()
    {
        base.UpdateParticle();

        // Do some flocking
        Vector2 velSum = Vector2.zero;
        Vector2 posSum = Vector2.zero;
        Vector2 sepSum = Vector2.zero;

        int count = 0;

        foreach (var other in Flocking.InRadius(this, Flocking.AroundRadius))
        {
            velSum += other.Velocity;
            posSum += other.Position;
            sepSum += (Position - other.Position) / (0.001f + Vector2.SqrMagnitude(other.Position - this.Position));
            count++;
        }

        Vector2 alignment = Vector2.zero;
        Vector2 cohesion = Vector2.zero;
        Vector2 seperation = Vector2.zero;


        if (count > 0)
        {
            alignment = velSum / count;
            alignment = alignment - Velocity;
            alignment = Vector2.ClampMagnitude(alignment, Flocking.MaxAlignForce);

            cohesion = posSum / count;
            cohesion -= Position;
            cohesion = Vector2.ClampMagnitude(cohesion, Flocking.MaxVelocity);
            cohesion = Velocity - cohesion;

            cohesion = Vector2.ClampMagnitude(cohesion, Flocking.MaxCohesionForce);

            seperation = sepSum / count;
            seperation = Vector2.ClampMagnitude(seperation, Flocking.MaxSeperationForce);
        }


        Vector2 accel = alignment + cohesion + seperation;
        Velocity = Vector2.ClampMagnitude(Velocity + accel, Flocking.MaxVelocity);
        Position += Velocity;
        BounceEdge();
    }
}
