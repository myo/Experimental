namespace MySharpSupport.Plugins
{
    using System;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SharpDX;

    using MySharpSupport.Util;

    using ActiveGapcloser = LeagueSharp.Common.ActiveGapcloser;

    public class Thresh : PluginBase
    {
        public Thresh()
        {
            this.Q = new Spell(SpellSlot.Q, 1025);
            this.W = new Spell(SpellSlot.W, 950);
            this.E = new Spell(SpellSlot.E, 400);
            this.R = new Spell(SpellSlot.R, 400);

            this.Q.SetSkillshot(0.5f, 70f, 1900, true, SkillshotType.SkillshotLine);
        }

        private const int QFollowTime = 3000;

        private Obj_AI_Hero _qTarget;

        private int _qTick;

        private bool FollowQ
        {
            get
            {
                return Environment.TickCount <= this._qTick + QFollowTime;
            }
        }

        private bool FollowQBlock
        {
            get
            {
                return Environment.TickCount - this._qTick >= QFollowTime;
            }
        }

        public override void ComboMenu(Menu config)
        {
            config.AddBool("ComboQ", "Use Q", true);
            config.AddBool("ComboQFollow", "Use Q Follow", true);
            config.AddBool("ComboW", "Use W for Engage", true);
            config.AddBool("ComboE", "Use E", true);
            config.AddSlider("ComboHealthE", "Push Targets away if low HP", 20, 1, 100);
            config.AddBool("ComboR", "Use R", true);
            config.AddSlider("ComboCountR", "Targets in range to Ult", 2, 1, 5);
        }

        public override void HarassMenu(Menu config)
        {
            config.AddBool("HarassQ", "Use Q", true);
            config.AddBool("HarassW", "Use W for Safe", true);
            config.AddBool("HarassE", "Use E", true);
            config.AddSlider("HarassHealthE", "Push Targets away if low HP", 20, 1, 100);
        }

        public override void InterruptMenu(Menu config)
        {
            config.AddBool("GapcloserE", "Use E to Interrupt Gapcloser", true);

            config.AddBool("InterruptE", "Use E to Interrupt Spells", true);
        }

        public override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (gapcloser.Sender.IsAlly)
            {
                return;
            }

            if (this.E.CastCheck(gapcloser.Sender, "GapcloserE"))
            {
                this.E.Cast(gapcloser.Start);
            }
        }

        public override void OnPossibleToInterrupt(Obj_AI_Hero target, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (args.DangerLevel < Interrupter2.DangerLevel.High || target.IsAlly)
            {
                return;
            }

            if (this.E.CastCheck(target, "InterruptE"))
            {
                this.E.Cast(target.Position);
            }
        }

        public override void OnUpdate(EventArgs args)
        {
            try
            {
                if (this._qTarget != null)
                {
                    if (Environment.TickCount - this._qTick >= QFollowTime)
                    {
                        this._qTarget = null;
                    }
                }

                if (this.ComboMode)
                {
                    if (this.Q.CastCheck(this.Target, "ComboQ") && this.FollowQBlock)
                    {
                        if (this.Q.Cast(this.Target) == Spell.CastStates.SuccessfullyCasted)
                        {
                            this._qTick = Environment.TickCount;
                            this._qTarget = this.Target;
                        }
                    }
                    if (this.Q.CastCheck(this._qTarget, "ComboQFollow"))
                    {
                        if (this.FollowQ)
                        {
                            this.Q.Cast();
                        }
                    }

                    if (this.W.CastCheck(this.Target, "ComboW"))
                    {
                        this.EngageFriendLatern();
                    }

                    if (this.E.CastCheck(this.Target, "ComboE"))
                    {
                        if (Helpers.AllyBelowHp(this.ConfigValue<Slider>("ComboHealthE").Value, this.E.Range) != null)
                        {
                            this.E.Cast(this.Target.Position);
                        }
                        else
                        {
                            this.E.Cast(Helpers.ReversePosition(ObjectManager.Player.Position, this.Target.Position));
                        }
                    }

                    if (this.R.CastCheck(this.Target, "ComboR"))
                    {
                        if (Helpers.EnemyInRange(this.ConfigValue<Slider>("ComboCountR").Value, this.R.Range))
                        {
                            this.R.Cast();
                        }
                    }
                }

                if (this.HarassMode)
                {
                    if (this.Q.CastCheck(this.Target, "HarassQ") && this.FollowQBlock)
                    {
                        this.Q.Cast(this.Target);
                    }

                    if (this.W.CastCheck(this.Target, "HarassW"))
                    {
                        this.SafeFriendLatern();
                    }

                    if (this.E.CastCheck(this.Target, "HarassE"))
                    {
                        if (Helpers.AllyBelowHp(this.ConfigValue<Slider>("HarassHealthE").Value, this.E.Range) != null)
                        {
                            this.E.Cast(this.Target.Position);
                        }
                        else
                        {
                            this.E.Cast(Helpers.ReversePosition(ObjectManager.Player.Position, this.Target.Position));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        ///     Credit
        ///     https://github.com/LXMedia1/UltimateCarry2/blob/master/LexxersAIOCarry/Thresh.cs
        /// </summary>
        private void EngageFriendLatern()
        {
            if (!this.W.IsReady())
            {
                return;
            }

            var bestcastposition = new Vector3(0f, 0f, 0f);

            foreach (var friend in
                HeroManager.Allies.Where(
                    hero =>
                    !hero.IsMe && hero.Distance(this.Player) <= this.W.Range + 300
                    && hero.Distance(this.Player) <= this.W.Range - 300 && hero.Health / hero.MaxHealth * 100 >= 20
                    && this.Player.CountEnemiesInRange(150) >= 1))
            {
                var center = this.Player.Position;
                const int points = 36;
                var radius = this.W.Range;
                const double slice = 2 * Math.PI / points;

                for (var i = 0; i < points; i++)
                {
                    var angle = slice * i;
                    var newX = (int)(center.X + radius * Math.Cos(angle));
                    var newY = (int)(center.Y + radius * Math.Sin(angle));
                    var p = new Vector3(newX, newY, 0);
                    if (p.Distance(friend.Position) <= bestcastposition.Distance(friend.Position))
                    {
                        bestcastposition = p;
                    }
                }

                if (friend.Distance(ObjectManager.Player) <= this.W.Range)
                {
                    this.W.Cast(bestcastposition, true);
                    return;
                }
            }

            if (bestcastposition.Distance(new Vector3(0f, 0f, 0f)) >= 100)
            {
                this.W.Cast(bestcastposition, true);
            }
        }

        /// <summary>
        ///     Credit
        ///     https://github.com/LXMedia1/UltimateCarry2/blob/master/LexxersAIOCarry/Thresh.cs
        /// </summary>
        private void SafeFriendLatern()
        {
            if (!this.W.IsReady())
            {
                return;
            }

            var bestcastposition = new Vector3(0f, 0f, 0f);

            foreach (var friend in
                HeroManager.Allies.Where(
                    hero =>
                    !hero.IsMe && hero.Distance(ObjectManager.Player) <= this.W.Range + 300
                    && hero.Distance(ObjectManager.Player) <= this.W.Range - 200
                    && hero.Health / hero.MaxHealth * 100 >= 20 && !hero.IsDead))
            {
                foreach (var enemy in HeroManager.Enemies)
                {
                    if (friend == null || !(friend.Distance(enemy) <= 300))
                    {
                        continue;
                    }

                    var center = ObjectManager.Player.Position;
                    const int points = 36;
                    var radius = this.W.Range;
                    const double slice = 2 * Math.PI / points;

                    for (var i = 0; i < points; i++)
                    {
                        var angle = slice * i;
                        var newX = (int)(center.X + radius * Math.Cos(angle));
                        var newY = (int)(center.Y + radius * Math.Sin(angle));
                        var p = new Vector3(newX, newY, 0);
                        if (p.Distance(friend.Position) <= bestcastposition.Distance(friend.Position))
                        {
                            bestcastposition = p;
                        }
                    }

                    if (friend.Distance(ObjectManager.Player) <= this.W.Range)
                    {
                        this.W.Cast(bestcastposition, true);
                        return;
                    }
                }

                if (bestcastposition.Distance(new Vector3(0f, 0f, 0f)) >= 100)
                {
                    this.W.Cast(bestcastposition, true);
                }
            }
        }
    }
}