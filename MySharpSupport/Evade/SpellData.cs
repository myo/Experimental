namespace MySharpSupport.Evade
{
    using LeagueSharp;

    public class SpellData
    {
        public SpellData()
        {
        }

        public SpellData(
            string championName,
            string spellName,
            SpellSlot slot,
            SkillShotType type,
            int delay,
            int range,
            int radius,
            int missileSpeed,
            bool addHitbox,
            bool fixedRange,
            int defaultDangerValue)
        {
            this.ChampionName = championName;
            this.SpellName = spellName;
            this.Slot = slot;
            this.Type = type;
            this.Delay = delay;
            this.Range = range;
            this.RawRadius = radius;
            this.MissileSpeed = missileSpeed;
            this.AddHitbox = addHitbox;
            this.FixedRange = fixedRange;
            this.DangerValue = defaultDangerValue;
        }

        public bool AddHitbox;

        public bool CanBeRemoved = false;

        public bool Centered;

        public string ChampionName;

        public CollisionObjectTypes[] CollisionObjects = { };

        public int DangerValue;

        public int Delay;

        public bool DisableFowDetection = false;

        public bool DontAddExtraDuration;

        public bool DontCross = false;

        public bool DontRemove = false;

        public int ExtraDuration;

        public string[] ExtraMissileNames = { };

        public int ExtraRange = -1;

        public string[] ExtraSpellNames = { };

        public bool FixedRange;

        public bool ForceRemove = false;

        public string FromObject = "";

        public string[] FromObjects = { };

        public int Id = -1;

        public bool Invert;

        public bool IsDangerous = false;

        public int MissileAccel = 0;

        public bool MissileDelayed;

        public bool MissileFollowsUnit;

        public int MissileMaxSpeed;

        public int MissileMinSpeed;

        public int MissileSpeed;

        public string MissileSpellName = "";

        public float MultipleAngle;

        public int MultipleNumber = -1;

        public int RingRadius;

        public SpellSlot Slot;

        public string SpellName;

        public string ToggleParticleName = "";

        public SkillShotType Type;

        public string MenuItemName
        {
            get
            {
                return this.ChampionName + " - " + this.SpellName;
            }
        }

        public int Radius
        {
            get
            {
                return (!this.AddHitbox) ? this.RawRadius : this.RawRadius + (int)ObjectManager.Player.BoundingRadius;
            }
            set
            {
                this.RawRadius = value;
            }
        }

        public int Range
        {
            get
            {
                return this.RawRange;
            }
            set
            {
                this.RawRange = value;
            }
        }

        public int RawRadius { get; private set; }

        public int RawRange { get; private set; }
    }
}