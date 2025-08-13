using System;

// Business logic interface (Single Responsibility)
public interface IUtilityService
{
    object GetHealth();
    object GetTime();
    object GenerateGuid();
}
