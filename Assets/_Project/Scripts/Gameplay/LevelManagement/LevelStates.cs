namespace _Project.Scripts.Gameplay.LevelManagement
{
    public enum LevelState
    {
        Idle,
        BusArriving,
        CheckWaitingPassengers,
        WaitingForInput,
        BusDeparting,
        BusClearingAndSpawnNext,
        LevelCompleted,
        LevelFailed
    }
}