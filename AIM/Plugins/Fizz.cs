using System;
using AutoSharp.Utils;
using LeagueSharp;
using LeagueSharp.Common;

namespace AutoSharp.Plugins
{
    public class Fizz : PluginBase
    {
        public static bool UseEAgain;

        public Fizz()
        {
            Q = new Spell(SpellSlot.Q, 560);
            W = new Spell(SpellSlot.W, 0);
            E = new Spell(SpellSlot.E, 370);
            R = new Spell(SpellSlot.R, 1275);

            E.SetSkillshot(0.5f, 120, 1300, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.5f, 250f, 1200f, false, SkillshotType.SkillshotLine);
            UseEAgain = true;
        }

        public override void OnUpdate(EventArgs args)
        {
            if (ComboMode)
            {
                Combo(Target);
            }
        }

        // combo from sigma series
        private void Combo(Obj_AI_Hero target)
        {
            if (target != null)
            {
                if (target.IsValidTarget(Q.Range) && Q.IsReady())
                {
                    Q.Cast(target);
                    return;
                }
                //castItems(target);
                if (target.IsValidTarget(R.Range) && R.IsReady())
                {
                    R.Cast(target, true);
                }
                if (target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)) && W.IsReady())
                {
                    W.Cast(Player);
                    return;
                }
                if (target.IsValidTarget(800) && E.IsReady() && UseEAgain)
                {
                    if (target.IsValidTarget(370 + 250) && ESpell.Name == "FizzJump")
                    {
                        E.Cast(target, true);
                        UseEAgain = false;
                        Utility.DelayAction.Add(250, () => UseEAgain = true);
                    }
                    if (target.IsValidTarget(370 + 150) && target.IsValidTarget(330) == false &&
                        ESpell.Name == "fizzjumptwo")
                    {
                        E.Cast(target, true);
                    }
                }
            }
        }

        public override void ComboMenu(Menu config)
        {
            config.AddBool("ComboQ", "Use Q", true);
            config.AddBool("ComboW", "Use W", true);
            config.AddBool("ComboE", "Use E", true);
            config.AddBool("ComboR", "Use R", true);
        }

        public static Spellbook SpellBook = ObjectManager.Player.Spellbook;
        public static SpellDataInst ESpell = SpellBook.GetSpell(SpellSlot.E);
    }
}