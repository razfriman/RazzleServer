using System;
using System.Collections.Generic;

namespace RazzleServer.Scripts
{
    public abstract class EventScript : AMapleScript
    {
        public List<AScriptCharacter> Characters = new List<AScriptCharacter>();
        public event Action OnFinish;
        public event Action<int, int, int, int> OnSpawnMobs;
        public event Action<int, int, Point, Point> OnRandomSpawnMobs;

        public virtual void AddCharacter(AScriptCharacter sChar)
        {
            Characters.Add(sChar);
        }

        public virtual void Finish()
        {
            OnFinish();
        }

        public virtual void SpawnMobs(int mobId, int count, int x, int y)
        {
            OnSpawnMobs(mobId, count, x, y);
        }

        public virtual void RandomSpawnMobs(int mobId, int count, Point maxPos, Point minPos)
        {
            OnRandomSpawnMobs(mobId, count, maxPos, minPos);
        }
    }
}
