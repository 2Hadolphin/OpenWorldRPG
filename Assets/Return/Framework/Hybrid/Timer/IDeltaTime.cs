using System;

public interface IDeltaTime : IDisposable
{
    void AddDeltaTime(float deltaTime);
}

