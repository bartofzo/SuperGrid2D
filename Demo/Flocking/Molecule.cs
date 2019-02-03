using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SuperGrid2D;

public class Molecule : Particle
{

    public override IConvex2D Shape => new Point(this.Position);


    protected override void Start()
    {
        base.Start();
    }

    public override void UpdateParticle()
    {
        base.UpdateParticle();

        Vector2 newPos = Position + Velocity;

       
        Particle nearest = Flocking.NearestParticle(this);
        if (nearest != null)
        {
            float coll = 0.5f;
            Vector2 myNormal = Velocity.normalized;
            Vector2 otherNormal = nearest.Velocity.normalized;

            Line myLine = new Line(Position , Position + myNormal * coll);
            Line otherLine = new Line(nearest.Position, nearest.Position + otherNormal * coll);

            if (!myLine.NoContactCertainty(otherLine) && !otherLine.NoContactCertainty(myLine))
            {
                // Collision
                Vector2 myVel = Velocity;
                this.Velocity = Vector2.Reflect(myVel, new Vector2(-nearest.Velocity.y, nearest.Velocity.x));
                nearest.Velocity = Vector2.Reflect(nearest.Velocity, new Vector2(-myVel.y, myVel.x));



                //nearest.HandledAction = true;
                //this.HandledAction = true;

                this.spriteRenderer.color = Color.red;
                nearest.spriteRenderer.color = Color.red;
            }
        }

        //HandledAction = false;
        Velocity = Vector2.ClampMagnitude(Velocity, Flocking.MaxVelocity);
        Position += Velocity;
        BounceEdge();
    }

    protected override void Update()
    {
        base.Update();
        this.spriteRenderer.color = Color.Lerp(this.spriteRenderer.color, Color.white, Time.deltaTime * 10);
    }
}
