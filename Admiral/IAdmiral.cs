using System;

namespace BattleshipsAda
{
    public interface IAdmiral
    {
        public string Name { get; }
        
        public Board Board { get; }
        public Board TargetBoard { get; }
        public Fleet Fleet { get; }

        public abstract Tuple<int, int> RequestAttackCoords();

        public abstract bool IsDefeated();

        public abstract void SetupFleet();
    }
}