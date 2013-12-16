using System;
using System.Collections.Generic;
using System.Text;

namespace Lab5
{
    static class GameConstants
    {
        //camera constants
        public const float CameraHeight = 25000.0f;
        public const float PlayfieldSizeX = 60f;
        public const float PlayfieldSizeY = 10f;
        public const float PlayfieldSizeZ = 60f;

        public const float PlayfieldmaxY = 20f;
        public const float PlayfieldminY = -20f;

        //Boss Constants
        public const int bossNum = 1;
        public const float BossMinSpeed = 1.0f;
        public const float BossMaxSpeed = 1.0f;
        public const float BossSpeedAdjustment = 2.5f;
        public const float BossScalar = 0.02f;

        //Dalek constants
        public const int NumDaleks = 10;
        public const float DalekMinSpeed = 10.0f;
        public const float DalekMaxSpeed = 10.0f;
        public const float DalekSpeedAdjustment = 2.5f;
        public const float DalekScalar = 0.01f;
        //collision constants
        public const float DalekBoundingSphereScale = 0.025f;  //50% size
        public const float ShipBoundingSphereScale = 0.5f;  //50% size
        public const float LaserBoundingSphereScale = 0.85f;  //50% size
        //bullet constants
        public const int NumLasers = 30;
        public const float LaserSpeedAdjustment = 5.0f;
        public const float LaserScalar = 3.0f;

    }
}
