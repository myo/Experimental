namespace MySharpSupport.Plugins
{
    using System;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    using MySharpSupport.Util;

    public class Annie : PluginBase
    {
        public Annie()
        {
            this.Q = new Spell(SpellSlot.Q, 650);
            this.W = new Spell(SpellSlot.W, 625);
            this.E = new Spell(SpellSlot.E);
            this.R = new Spell(SpellSlot.R, 600);

            this.Q.SetTargetted(250, 1400);
            this.W.SetSkillshot(600, (float)(50 * Math.PI / 180), float.MaxValue, false, SkillshotType.SkillshotCone);
            this.R.SetSkillshot(250, 200, float.MaxValue, false, SkillshotType.SkillshotCircle);
        }

        public override void ComboMenu(Menu config)
        {
            config.AddBool("ComboQ", "Use Q", true);
            config.AddBool("ComboW", "Use W", true);
            config.AddBool("ComboR", "Use R", true);
        }

        public override void InterruptMenu(Menu config)
        {
            config.AddBool("Interrupt.Q", "Use Q to Interrupt Spells", true);
            config.AddBool("Interrupt.W", "Use W to Interrupt Spells", true);
        }

        public override void OnPossibleToInterrupt(Obj_AI_Hero target, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (args.DangerLevel < Interrupter2.DangerLevel.High || target.IsAlly)
            {
                return;
            }
            if (this.GetPassiveStacks() >= 4)
            {
                if (this.Q.CastCheck(target, "Interrupt.Q"))
                {
                    this.Q.Cast(target);
                    return;
                }
                if (this.W.CastCheck(target, "Interrupt.W"))
                {
                    this.W.CastOnUnit(target);
                    return;
                }
            }
            if (this.GetPassiveStacks() == 3)
            {
                if (this.E.IsReady())
                {
                    this.E.Cast();
                    if (this.Q.CastCheck(target, "Interrupt.Q"))
                    {
                        this.Q.Cast(target);
                        return;
                    }
                    if (this.W.CastCheck(target, "Interrupt.W"))
                    {
                        this.W.CastOnUnit(target);
                        return;
                    }
                }
                if (this.Q.CastCheck(target, "Interrupt.Q") && this.W.CastCheck(target, "Interrupt.W"))
                {
                    this.Q.Cast(target);
                    this.W.CastOnUnit(target);
                }
            }
        }

        public override void OnUpdate(EventArgs args)
        {
            if (this.ComboMode)
            {
                if (this.Q.CastCheck(this.Target, "ComboQ"))
                {
                    this.Q.Cast(this.Target, false);
                }
                if (this.W.CastCheck(this.Target, "ComboW"))
                {
                    this.W.Cast(this.Target, true);
                }

                if (this.R.CastCheck(this.Target, "ComboR"))
                {
                    this.R.Cast(this.Target, true);
                }
                this.CastE();
            }
        }

        private void CastE()
        {
            if (this.GetPassiveStacks() < 4 && !ObjectManager.Player.IsRecalling())
            {
                this.E.Cast();
            }
        }

        //sosharp love xSalice
        private int GetPassiveStacks()
        {
            var buffs =
                ObjectManager.Player.Buffs.Where(
                    buff => (buff.Name.ToLower() == "pyromania" || buff.Name.ToLower() == "pyromania_particle"));
            var buffInstances = buffs as BuffInstance[] ?? buffs.ToArray();
            if (!buffInstances.Any())
            {
                return 0;
            }
            var buf = buffInstances.First();
            var count = buf.Count >= 4 ? 4 : buf.Count;
            return buf.Name.ToLower() == "pyromania_particle" ? 4 : count;
        }
    }
}