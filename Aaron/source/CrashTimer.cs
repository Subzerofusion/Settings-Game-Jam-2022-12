using Godot;
using System;

public class CrashTimer : Timer
{
    public void OnCrashTimerTimeout()
    {
        GetTree().Quit();
    }
}
