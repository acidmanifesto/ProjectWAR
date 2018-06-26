﻿using System.Collections.Generic;
using System.Linq;
using Common.Database.World.Battlefront;
using GameData;
using NLog;
using WorldServer.World.BattleFronts.Keeps;

namespace WorldServer.World.Battlefronts.Apocalypse
{
    public class UpperTierBattleFrontManager : IBattleFrontManager
    {
        public List<RegionMgr> RegionMgrs { get; }
        public List<RVRProgression> BattleFrontProgressions { get; }
        private static readonly Logger ProgressionLogger = LogManager.GetLogger("RVRProgressionLogger");
        /// <summary>
        /// The RacialPair that is currently active.
        /// </summary>
        public RVRProgression ActiveBattleFront { get; set; }

        public UpperTierBattleFrontManager(List<RVRProgression> _RVRT4Progressions, List<RegionMgr> regionMgrs)
        {
            BattleFrontProgressions = _RVRT4Progressions;
            RegionMgrs = regionMgrs;
        }

        /// <summary>
        /// Log the status of all battlefronts 
        /// </summary>
        public void AuditBattleFronts(int tier)
        {
            foreach (var regionMgr in RegionMgrs)
            {
                if (regionMgr.GetTier() == tier)
                {
                    foreach (var objective in regionMgr.ndbf.Objectives)
                    {
                        ProgressionLogger.Debug($"{regionMgr.RegionName} {objective.Name} {objective.FlagState} {objective.State}");
                    }
                    foreach (var keep in regionMgr.ndbf.Keeps)
                    {
                        ProgressionLogger.Debug($"{regionMgr.RegionName} {keep.Name} {keep.KeepStatus} ");
                    }
                }
            }
        }


        /// <summary>
        /// Log the status of all battlefronts 
        /// </summary>
        public void LockBattleFronts(int tier)
        {
            foreach (var regionMgr in RegionMgrs)
            {
                if (regionMgr.GetTier() == tier)
                {
                    foreach (var objective in regionMgr.ndbf.Objectives)
                    {
                        objective.LockObjective(Realms.REALMS_REALM_NEUTRAL, false);
                        ProgressionLogger.Debug($" Locking to Neutral {objective.Name} {objective.FlagState} {objective.State}");
                    }
                    foreach (var keep in regionMgr.ndbf.Keeps)
                    {
                        keep.LockKeep(Realms.REALMS_REALM_NEUTRAL, false, false);
                        ProgressionLogger.Debug($" Locking to Neutral {keep.Name} {keep.KeepStatus} ");
                    }
                }
            }
        }

        

        public void OpenActiveBattlefront()
        {
            var activeRegion = RegionMgrs.Single(x => x.RegionId == this.ActiveBattleFront.RegionId);
            ProgressionLogger.Info($" Opening battlefront in {activeRegion.RegionName} Zone : {this.ActiveBattleFront.ZoneId} {this.ActiveBattleFrontName}");
            
            activeRegion.ndbf.VictoryPointProgress.Reset(activeRegion.ndbf);
            activeRegion.ndbf.LockingRealm = Realms.REALMS_REALM_NEUTRAL;

            foreach (var flag in activeRegion.ndbf.Objectives)
            {
                if (this.ActiveBattleFront.ZoneId == flag.ZoneId)
                    flag.UnlockObjective();
            }

            foreach (Keep keep in activeRegion.ndbf.Keeps)
            {
                if (this.ActiveBattleFront.ZoneId == keep.ZoneId)
                    keep.NotifyPairingUnlocked();
            }
        }

        public void LockBattleFront(Realms realm)
        {
            var activeRegion = RegionMgrs.Single(x => x.RegionId == this.ActiveBattleFront.RegionId);
            ProgressionLogger.Info($" Locking battlefront in {activeRegion.RegionName} Zone : {this.ActiveBattleFront.ZoneId} {this.ActiveBattleFrontName}");
            
            foreach (var flag in activeRegion.ndbf.Objectives)
            {
                if (this.ActiveBattleFront.ZoneId == flag.ZoneId)
                    flag.LockObjective(realm, true);
            }

            foreach (Keep keep in activeRegion.ndbf.Keeps)
            {
                if (this.ActiveBattleFront.ZoneId == keep.ZoneId)
                    keep.LockKeep(realm, true, true);
            }

            activeRegion.ndbf.LockBattleFront(realm);

        }

        /// <summary>
        /// Set the Active Pairing to be null. Not expected to be needed.
        /// </summary>
        public RVRProgression ResetBattleFrontProgression()
        {
            ProgressionLogger.Debug($" Resetting battlefront progression...");
            // HACK
            ActiveBattleFront = GetBattleFrontByName("Praag");
            ProgressionLogger.Debug($"Active : {this.ActiveBattleFrontName}");
            return ActiveBattleFront;
        }

        public RVRProgression GetBattleFrontByName(string name)
        {
            return BattleFrontProgressions.Single(x => x.Description.Contains(name));
        }

        public RVRProgression GetBattleFrontByBattleFrontId(int id)
        {
            return BattleFrontProgressions.Single(x => x.BattleFrontId == id);
        }

        public string ActiveBattleFrontName
        {
            get => ActiveBattleFront.Description;
            set => ActiveBattleFront.Description = value;
        }

        /// <summary>
        /// </summary>
        public RVRProgression AdvanceBattleFront(Realms lockingRealm)
        {
            if (lockingRealm == Realms.REALMS_REALM_ORDER)
            {
                var newBattleFront = GetBattleFrontByBattleFrontId(ActiveBattleFront.OrderWinProgression);
                ProgressionLogger.Debug($"Order Win : Advancing Battlefront from {this.ActiveBattleFrontName} to {newBattleFront.Description}");
                return ActiveBattleFront = newBattleFront;
            }

            if (lockingRealm == Realms.REALMS_REALM_DESTRUCTION)
            {
                var newBattleFront = GetBattleFrontByBattleFrontId(ActiveBattleFront.DestWinProgression);
                ProgressionLogger.Debug($"Destruction Win : Advancing Battlefront from {this.ActiveBattleFrontName} to {newBattleFront.Description}");
                return ActiveBattleFront = newBattleFront;
            }
            return ResetBattleFrontProgression();
        }
    }
}