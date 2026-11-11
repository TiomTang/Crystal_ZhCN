using System.Drawing;
using Server.MirEnvir;
using Shared;

namespace Server.MirObjects
{
    /// <summary>
    /// Pet side of MonsterObject. Crystal does not model pets as a separate type: taming and
    /// summoning both assign Master on an existing monster, and a tame returns to the wild by
    /// clearing it again. Everything that depends on the owner's stance lives here so the rules
    /// stay in one place instead of being re-derived from Master.PMode at every call site.
    /// </summary>
    public partial class MonsterObject
    {
        /// <summary>True when this object has an owner (summoned, tamed or hired).</summary>
        public bool IsPet
        {
            get { return Master != null; }
        }

        /// <summary>True for a temporary tame, which returns to the wild when TameTime expires.</summary>
        public bool IsTamed
        {
            get { return Master != null && TameTime > 0; }
        }

        /// <summary>True when the owner's stance lets pets walk.</summary>
        public bool MasterAllowsMove
        {
            get
            {
                return Master == null ||
                    Master.PMode == PetMode.MoveOnly ||
                    Master.PMode == PetMode.Both ||
                    Master.PMode == PetMode.FocusMasterTarget;
            }
        }

        /// <summary>True when the owner's stance lets pets attack.</summary>
        public bool MasterAllowsAttack
        {
            get
            {
                return Master == null ||
                    Master.PMode == PetMode.AttackOnly ||
                    Master.PMode == PetMode.Both ||
                    Master.PMode == PetMode.FocusMasterTarget;
            }
        }

        /// <summary>True when the owner's stance lets pets pick their own targets.</summary>
        public bool MasterAllowsSearch
        {
            get
            {
                return Master == null ||
                    Master.PMode == PetMode.AttackOnly ||
                    Master.PMode == PetMode.Both;
            }
        }

        /// <summary>True when the owner's stance makes pets drop their target (follow and rest).</summary>
        public bool MasterClearsTarget
        {
            get
            {
                return Master != null &&
                    (Master.PMode == PetMode.MoveOnly || Master.PMode == PetMode.None);
            }
        }

        /// <summary>
        /// Owner-facing state for this tick: forced recall when the maps differ, unfreezing once the
        /// pet is back on the owner's map, normal recall when the owner walks away, and clearing the
        /// target in follow and rest stances. Returns false when the pet must stay frozen and skip
        /// the rest of its AI.
        /// </summary>
        protected bool ProcessPetStatus()
        {
            if (Master != null && Master.CurrentMap != null)
            {
                bool masterAllowsPets = !Master.CurrentMap.Info.NoPets || IgnoresNoPetRestriction;
                bool needsRecall = CurrentMap != Master.CurrentMap;

                // If frozen AND on different map AND master allows pets — force recall
                if (Frozen && needsRecall && masterAllowsPets)
                {
                    PetRecall();
                    return false;
                }

                // If frozen but already on correct map — unfreeze
                if (Frozen && !needsRecall && masterAllowsPets)
                {
                    Frozen = false;
                    PMode = PetMode.Both;
                }
            }

            // Still frozen = do nothing
            if (Frozen) return false;

            if (Master == null) return true;

            Map masterMap = Master.CurrentMap;
            Point masterLocation = Master.CurrentLocation;

            if (MasterAllowsMove && masterMap != null)
            {
                if (!Functions.InRange(CurrentLocation, masterLocation, Globals.DataRange) || CurrentMap != masterMap)
                    PetRecall();
            }

            if (MasterClearsTarget)
            {
                Target = null;
            }

            return true;
        }
    }
}
