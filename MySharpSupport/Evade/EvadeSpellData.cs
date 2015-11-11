namespace MySharpSupport.Evade
{
    using LeagueSharp;

    public enum SpellValidTargets
    {
        AllyMinions,

        EnemyMinions,

        AllyWards,

        EnemyWards,

        AllyChampions,

        EnemyChampions
    }

    /// <summary>
    ///     Class containing the needed info about the evading spells.
    /// </summary>
    internal class EvadeSpellData
    {
        public EvadeSpellData()
        {
        }

        public EvadeSpellData(string name, int dangerLevel)
        {
            this.Name = name;
            this._dangerLevel = dangerLevel;
        }

        public int _dangerLevel;

        public bool CanShieldAllies;

        public string CheckSpellName = "";

        public int Delay;

        public bool FixedRange;

        public bool Invert;

        public bool IsBlink;

        public bool IsDash;

        public bool IsInvulnerability;

        public bool IsMovementSpeedBuff;

        public bool IsShield;

        public bool IsSpellShield;

        public bool IsSummonerSpell;

        public float MaxRange;

        public MoveSpeedAmount MoveSpeedTotalAmount;

        public string Name;

        public bool RequiresPreMove;

        public bool SelfCast;

        public SpellSlot Slot;

        public int Speed;

        public SpellValidTargets[] ValidTargets;

        public delegate float MoveSpeedAmount();

        public bool IsTargetted
        {
            get
            {
                return this.ValidTargets != null;
            }
        }

        //public int DangerLevel
        //{
        //    get
        //    {
        //        if (Config.Menu.Item("DangerLevel" + Name) != null)
        //        {
        //            return Config.Menu.Item("DangerLevel" + Name).ConfigValue<Slider>().Value;
        //        }
        //        return _dangerLevel;
        //    }
        //}

        //public bool Enabled
        //{
        //    get
        //    {
        //        if (Config.Menu.Item("Enabled" + Name) != null)
        //        {
        //            return Config.Menu.Item("Enabled" + Name).ConfigValue<bool>();
        //        }
        //        return true;
        //    }
        //}

        public bool IsReady()
        {
            return ((this.CheckSpellName == ""
                     || ObjectManager.Player.Spellbook.GetSpell(this.Slot).Name == this.CheckSpellName)
                    && ((this.IsSummonerSpell
                         && ObjectManager.Player.Spellbook.CanUseSpell(this.Slot) == SpellState.Ready)
                        || (!this.IsSummonerSpell
                            && ObjectManager.Player.Spellbook.CanUseSpell(this.Slot) == SpellState.Ready)));
        }
    }

    internal class DashData : EvadeSpellData
    {
        public DashData(
            string name,
            SpellSlot slot,
            float range,
            bool fixedRange,
            int delay,
            int speed,
            int dangerLevel)
        {
            this.Name = name;
            this.MaxRange = range;
            this.Slot = slot;
            this.FixedRange = fixedRange;
            this.Delay = delay;
            this.Speed = speed;
            this._dangerLevel = dangerLevel;
            this.IsDash = true;
        }
    }

    internal class BlinkData : EvadeSpellData
    {
        public BlinkData(
            string name,
            SpellSlot slot,
            float range,
            int delay,
            int dangerLevel,
            bool isSummonerSpell = false)
        {
            this.Name = name;
            this.MaxRange = range;
            this.Slot = slot;
            this.Delay = delay;
            this._dangerLevel = dangerLevel;
            this.IsSummonerSpell = isSummonerSpell;
            this.IsBlink = true;
        }
    }

    internal class InvulnerabilityData : EvadeSpellData
    {
        public InvulnerabilityData(string name, SpellSlot slot, int delay, int dangerLevel)
        {
            this.Name = name;
            this.Slot = slot;
            this.Delay = delay;
            this._dangerLevel = dangerLevel;
            this.IsInvulnerability = true;
        }
    }

    internal class ShieldData : EvadeSpellData
    {
        public ShieldData(string name, SpellSlot slot, int delay, int dangerLevel, bool isSpellShield = false)
        {
            this.Name = name;
            this.Slot = slot;
            this.Delay = delay;
            this._dangerLevel = dangerLevel;
            this.IsSpellShield = isSpellShield;
            this.IsShield = !this.IsSpellShield;
        }
    }

    internal class MoveBuffData : EvadeSpellData
    {
        public MoveBuffData(string name, SpellSlot slot, int delay, int dangerLevel, MoveSpeedAmount amount)
        {
            this.Name = name;
            this.Slot = slot;
            this.Delay = delay;
            this._dangerLevel = dangerLevel;
            this.MoveSpeedTotalAmount = amount;
            this.IsMovementSpeedBuff = true;
        }
    }
}