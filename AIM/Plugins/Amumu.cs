using System;
using AutoSharp.Utils;
using LeagueSharp;
using LeagueSharp.Common;

namespace AutoSharp.Plugins
{
    public class Amumu : PluginBase
    {
        private bool _wUse;

        public Amumu()
        {
            Q = new Spell(SpellSlot.Q, 1100);
            Q.SetSkillshot(
                Q.Instance.SData.SpellCastTime, Q.Instance.SData.LineWidth, Q.Instance.SData.MissileSpeed, true,
                SkillshotType.SkillshotLine);


            W = new Spell(SpellSlot.W, 300);
            E = new Spell(SpellSlot.E, 350);
            R = new Spell(SpellSlot.R, 550);
        }

        public override void OnUpdate(EventArgs args)
        {
            if (ComboMode)
            {
                var qPred = Q.GetPrediction(Target);

                if (Q.IsReady() && Heroes.Player.Distance(Target) < Q.Range)
                {
                    Q.Cast(qPred.CastPosition);
                }

                if (W.IsReady() && !_wUse && Player.CountEnemiesInRange(R.Range) >= 1)
                {
                    W.Cast();
                    _wUse = true;
                }
                if (_wUse && Player.CountEnemiesInRange(R.Range) == 0)
                {
                    W.Cast();
                    _wUse = false;
                }

                if (E.IsReady() && Heroes.Player.Distance(Target) < E.Range)
                {
                    E.Cast();
                }

                if (R.IsReady() && Heroes.Player.Distance(Target) < R.Range)
                {
                    R.CastIfWillHit(Target, 2);
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
    }
}