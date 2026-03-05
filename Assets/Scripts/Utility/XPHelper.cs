using UnityEngine;
using System;

public static class XPHelper
{

    public static float GetXPToNextLevel(float currentXP)
    {
        // a = 50; //How fast the difficulty ramps up
        // b = 100; //Early-game pace
        // c = 0; //Starting offset
        
        // 1. Determine current level
        double levelFloat = (-100.0 + Math.Sqrt(10000 + 200.0 * currentXP)) / 100.0;
        int level = (int)Math.Floor(levelFloat);

        // 2. XP needed to go from level -> level+1
        int xpToNextLevel = 100 * level + 150;

        return xpToNextLevel;
    }
    public static float GetCurrentXPLevel(float currentXP)
    {
        double a = 50; //How fast the difficulty ramps up
        double b = 100; //Early-game pace
        double c = 0; //Starting offset
        // Solve aL^2 + bL + c <= xp
        // L = (-b + sqrt(b^2 - 4a(c - xp))) / (2a)
        double disc = b * b - 4 * a * (c - currentXP);
        if (disc < 0) return 0; // not enough XP (or invalid params)

        double L = (-b + Math.Sqrt(disc)) / (2 * a);
        int level = (int)Math.Floor(L);

        // Safety clamp
        if (level < 0) level = 0;
        return level;
    }
    public static float GetXPLevelFloor(float currentXP)
    {
        // a = 50; //How fast the difficulty ramps up
        // b = 100; //Early-game pace
        // c = 0; //Starting offset
        
        // 1. Determine current level
        double levelFloat = (-100.0 + Math.Sqrt(10000 + 200.0 * currentXP)) / 100.0;
        int level = (int)Math.Floor(levelFloat);

        // 2. XP needed to go from level -> level+1
        //int xpToNextLevel = 100 * level + 150;

        // 3. Total XP required to reach next level
        int totalXpNext = 50 * (level + 1) * (level + 1) + 100 * (level + 1);
        int levelFloor = 50 * (level) * (level) + 100 * (level);

        // 4. XP left
        //return totalXpNext - currentXP;
        return levelFloor;
    }

    public static float GetProgressPercentage(float currentXP)
    {
        float levelFloor = GetXPLevelFloor(currentXP);
        float xpRemaining = GetXPToNextLevel(currentXP);
        float xpProgress = currentXP - levelFloor;
        float levelCeiling = xpProgress + xpRemaining;
        float progressPercent = (100 / levelCeiling) * xpProgress;
        return progressPercent;
    }

}