using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeDefender
    {
        // Story-only roles: paper executive, sonic executive, evasive executive, driver, secretary, approver.
        [System.NonSerialized] public int storyRole,storyWave;
        public void ConfigureExecutive(int role,int wave,float health)
        {
            storyRole=role;storyWave=wave;Health=MaxHealth=health;
            attackClock=1.5f+wave*.2f;UpdateLabel();
        }
    }
}
