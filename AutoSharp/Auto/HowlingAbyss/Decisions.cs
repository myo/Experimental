using System.Linq;
using AutoSharp.Utils;
using LeagueSharp;
using LeagueSharp.Common;

namespace AutoSharp.Auto.HowlingAbyss
{
    internal static class Decisions
    {
        internal static bool HealUp()
        {
            if (Heroes.Player.IsDead)
            {
                Program.Orbwalker.ActiveMode = MyOrbwalker.OrbwalkingMode.None;
                return true;
            }

            if (Heroes.Player.HealthPercent >= 75) return false;

            var closestEnemyBuff = HealingBuffs.EnemyBuffs.FirstOrDefault(eb => eb.IsVisible && eb.IsValid && eb.Position.Distance(Heroes.Player.Position) < 800 && (eb.Position.CountEnemiesInRange(600) == 0 || eb.Position.CountEnemiesInRange(600) < eb.Position.CountAlliesInRange(600)));
            var closestAllyBuff = HealingBuffs.AllyBuffs.FirstOrDefault(ab => ab.IsVisible && ab.IsValid);


            //BUFF EXISTANCE CHECKS;
            if ((closestAllyBuff == null && closestEnemyBuff == null)) return false;

            //BECAUSE WE CHECKED THAT BUFFS CAN'T BE BOTH NULL; IF ONE OF THEM IS NULL IT MEANS THE OTHER ISN'T.
            // ReSharper disable once PossibleNullReferenceException
            var buffPos = closestEnemyBuff != null ? closestEnemyBuff.Position.Randomize(0, 15) : closestAllyBuff.Position.Randomize(0,15);

            if (Heroes.Player.Position.Distance(buffPos) <= 800 && (Heroes.Player.CountEnemiesInRange(800) == 0 || Heroes.Player.CountEnemiesInRange(800) < Heroes.Player.CountAlliesInRange(800)))
            {
                Program.Orbwalker.SetOrbwalkingPoint(buffPos);
                return true;
            }

            //stay in fight if you can't instantly gratify yourself and u don't really need the buff
            if (Heroes.Player.HealthPercent >= 45 && Heroes.Player.CountEnemiesInRange(900) <= Heroes.Player.CountAlliesInRange(900) && Heroes.Player.Distance(buffPos) > 1000) return false;

            //IF BUFFPOS IS VECTOR ZERO OR NOT VALID SOMETHING MUST HAVE GONE WRONG
            if (!buffPos.IsValid()) return false;

            //MOVE TO BUFFPOS
            Program.Orbwalker.SetOrbwalkingPoint(buffPos);

            //STOP EVERYTHING ELSE TO DO THIS
            return true;
        }

        internal static bool Farm()
        {
            var minion = Wizard.GetFarthestMinion();
            var minionPos = minion != null ? minion.Position.Extend(HeadQuarters.AllyHQ.Position, 250).RandomizePosition() : Wizard.GetFarthestAllyTurret().RandomizePosition();
            //IF THERE ARE ALLIES AROUND US STOP ORBWALKING AROUND THE TURRET LIKE A RETARD
            if (Heroes.Player.Distance(Wizard.GetFarthestAllyTurret().Position) < 500 && Heroes.Player.CountAlliesInRange(1000) != 0 && Minions.AllyMinions.Count < 3) return false;
            //IF THERE ARE ENEMIES AROUND US OR THE MINION WE WONT FOLLOW HIM, WE WILL FIGHT!
            if ((minionPos.CountEnemiesInRange(1000) != 0 || Heroes.Player.CountEnemiesInRange(1000) != 0) && minionPos.CountAlliesInRange(1000) != 0) return false;
            //IF THE FARTHEST ALLY IS IN DANGER, WE SHALL FIGHT WITH HIM
            if (Heroes.AllyHeroes.OrderByDescending(h => h.Distance(HeadQuarters.AllyHQ)).FirstOrDefault().CountEnemiesInRange(1400) != 0) return false;
            //IF WERE FUGGD WE WILL FIGHT SKIP FARMING CUZ WE CANT FARM WHILE FUGGING XDD
            if (Heroes.Player.CountEnemiesInRange(1000) > Heroes.Player.CountAlliesInRange(1000)) return false;
            //FOLLOW MINION
            Program.Orbwalker.SetOrbwalkingPoint(minionPos.RandomizePosition());
            Program.Orbwalker.ActiveMode = MyOrbwalker.OrbwalkingMode.LaneClear;
            //IF I JUST FARM A BIT GUYS WE MIGHT WIN...
            return true;
        }

        internal static void Fight()
        {
            Program.Orbwalker.ActiveMode = Heroes.Player.CountEnemiesInRange(Heroes.Player.AttackRange) == 0 ? MyOrbwalker.OrbwalkingMode.LaneClear : MyOrbwalker.OrbwalkingMode.Combo;
            Program.Orbwalker.SetOrbwalkingPoint(Positioning.RandomlyChosenMove);
        }

        internal static bool ImSoLonely()
        {
            if (Heroes.AllyHeroes.All(h => h.IsDead) || Heroes.AllyHeroes.All(h=>h.InFountain()) || (Heroes.AllyHeroes.All(h => h.Distance(HeadQuarters.AllyHQ) < Heroes.Player.Distance(h))))
            {
                Program.Orbwalker.SetOrbwalkingPoint(Wizard.GetFarthestAllyTurret().Position.RandomizePosition());
                Program.Orbwalker.ActiveMode = Heroes.Player.Distance(Wizard.GetFarthestAllyTurret().Position) < 500 ? MyOrbwalker.OrbwalkingMode.LaneClear : MyOrbwalker.OrbwalkingMode.LaneClear;
                return true;
            }
            return false;
        }
    }
}
