using Common.Database.World.MythicAbility;
using WorldServer.World.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldServer.World.MythicAbility
{    public class AbilityComponentVariables
    {
        public ushort ID;
        public string A00;
        public string Values;
        public ushort Multipliers;
        public ushort ActivationDelay;
        public ushort Duration;
        public ushort Flags;
        public ushort IconAlwaysVisible;
        public ushort Operation;
        public ushort Interval;
        public ushort Radius;
        public ushort ConeAngle;
        public ushort FlightSpeed;
        public ushort A15;
        public ushort MaxTargets;
        public ushort Description;

        public ushort baseDamage;

        
        public AbilityComponentVariables()
        {
            int baseDamage = 0;
            int _Multipliers = Convert.ToInt32(Multipliers);
            int _Values = Convert.ToInt32(Values);

            WorldServer.World.Abilities.Components.AbilityInfo _AbilityInfo = new WorldServer.World.Abilities.Components.AbilityInfo();
            int Level = _AbilityInfo.Level;

            baseDamage = (((Level - 1) * (1 / 6) * _Values) + _Values) * _Multipliers;
            Console.WriteLine(baseDamage);
            Console.ReadKey();
        }
        

        
    }
}
