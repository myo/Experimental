#region LICENSE

// Copyright 2014 - 2014 Support
// Annie.cs is part of Support.
// Support is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// Support is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// You should have received a copy of the GNU General Public License
// along with Support. If not, see <http://www.gnu.org/licenses/>.

#endregion

#region

using System;
using AutoSharp.Utils;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace AutoSharp.Plugins
{
    public class Ryze : PluginBase
    {
        public Ryze()
        {
            Q = new Spell(SpellSlot.Q, 630);
            Q.SetTargetted(0.2f, float.MaxValue);

            W = new Spell(SpellSlot.W, 600);

            E = new Spell(SpellSlot.E, 600);
            E.SetTargetted(0.2f, float.MaxValue);

            R = new Spell(SpellSlot.R);
        }



        // some part from DevRyze

        public override void OnUpdate(EventArgs args)
        {
            
                if (Q.IsReady() && Heroes.Player.Distance(Target) < Q.Range)
                {
                    Q.Cast(Target);
                }

                if (Player.Distance(Target) >= 575 && !Target.IsFacing(Player) && W.IsReady() && Heroes.Player.Distance(Target) < W.Range)
                {
                    W.Cast(Target);
                }

                if (Target.IsValidTarget(W.Range) && W.IsReady() && Heroes.Player.Distance(Target) < W.Range)
                {
                    W.Cast(Target);
                }

                if (Target.IsValidTarget(E.Range) && W.CastCheck(Target, "ComboE"))
                {
                    E.Cast(Target);
                }

                if (R.IsReady())
                {
                    R.Cast();
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