namespace gs
{
    // codes to pass to IGCodeListener.CustomCommand
    // this is basically just slightly better than complete hacks
    public enum CustomListenerCommands
    {
        ResetPosition = 0,      // object should be Vector3d
        ResetExtruder = 1       // object should be Vector3d
    }
}