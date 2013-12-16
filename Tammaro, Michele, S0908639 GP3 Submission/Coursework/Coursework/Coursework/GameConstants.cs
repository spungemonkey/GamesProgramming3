using System;
using System.Collections.Generic;
using System.Text;

namespace Lab5
{
    static class GameConstants
    {
        //camera constants
        public const float CameraHeight = 25000.0f;
        //World Constants - size
        public const float PlayfieldSizeX = 50f; //Max and Min size of the world on the X axis
        public const float PlayfieldSizeY = 9.0f; //Max and Min size of the world on the Y axis
        public const float PlayfieldSizeZ = 60f; //Max and Min size of the world on the Z axis
        public const float PlayfieldmaxY = 15f; //Max field size for enemies
        public const float PlayfieldminY = -15f; //min field size for enemies

        //Enemy Constants
        public const float posSpeed = 25.0f; //speed that the enemy moves up on the y axis
        public const float negSpeed = -25.0f; // speed that the enemy moves down on the yaxis

        //Boss Constants
        public const float BossScalar = 0.02f; //value to scale the boss model

        //Dreadful Flying Glove constants
        public const int NumDFG = 10; //Total number of DFG spawned at any one time
        public const float DFGMinSpeed = 10.0f; // Minimum speed of DFG. These values are the same so they travel at a constant speed
        public const float DFGMaxSpeed = 10.0f; //Maximum speed  of DFG
        public const float DFGSpeedAdjustment = 7.5f; //Determines what speed the
        public const float DFGScalar = 0.5f; //value to scale the DFG Model

                //collision constants
        public const float DFGBoundingSphereScale = 0.75f;  //50% size
        public const float YSBoundingSphereScale = 0.5f;  //50% size
        public const float BoltBoundingSphereScale = 0.85f;  //50% size

        //bullet constants
        public const int NumBolts = 3; //Total number of bolts
        public const int NumBossBolts = 1; //total number of boss bolts
        public const float BoltSpeedAdjustment = 7.0f; //speed of the bolts
        public const float BossBoltSpeedAdjustment = 12.0f; //speed of the bosses bolts
        public const float BoltScalar = 8.0f; //bolt scale for the models

        //waypoint constants
        public const int NumWaypoints = 8;


    }
}
