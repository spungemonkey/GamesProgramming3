using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Lab5
{
    struct Bolt
    {
        public Vector3 position; //position of the bolt
        public Vector3 direction; //ddirection of the bolt
        public float speed; //speed the bolt travels at
        public bool isActive; //determines if the bolt exists or not

        public void Update(float delta)
        {
            position += direction * speed *
                        GameConstants.BoltSpeedAdjustment * delta; //updates the position of the bolt

            //removes the bolt if it leaves the game world
            if (position.X > GameConstants.PlayfieldSizeX || 
                position.X < -GameConstants.PlayfieldSizeX ||
                position.Z > GameConstants.PlayfieldSizeZ ||
                position.Z < -GameConstants.PlayfieldSizeZ)
                isActive = false;
        }
    }
}
