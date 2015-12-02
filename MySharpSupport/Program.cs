namespace MySharpSupport
{
    using System;
    using System.Reflection;

    using LeagueSharp;
    using LeagueSharp.Common;

    using MySharpSupport.Util;

    using Version = System.Version;

    internal class Program
    {
        public static Version Version;

        private static void Main(string[] args)
        {
            Version = Assembly.GetExecutingAssembly().GetName().Version;

            CustomEvents.Game.OnGameLoad += a =>
                {
                    try
                    {
                        //Load AI
                        AI.Init.DoInitWithDelay();

                        var type = Type.GetType("Support.Plugins." + ObjectManager.Player.ChampionName);

                        if (type != null)
                        {
                            Protector.Init();
                            //SpellDetector.Init();
                            DynamicInitializer.NewInstance(type);
                            return;
                        }

                        Helpers.PrintMessage(ObjectManager.Player.ChampionName + " not supported");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                };

            //Utils.EnableConsoleEditMode();

            //Drawing.OnDraw += a =>
            //{
            //    var offset = 0;
            //    foreach (var buff in ObjectManager.Player.Buffs)
            //    {
            //        Drawing.DrawText(100, 100 + offset, Color.Tomato,
            //            string.Format("{0} | {1} | {2} | {3} | {4} | {5} | {6}", buff.Name, buff.DisplayName,
            //                buff.Type.ToString(), buff.Count, buff.IsActive, buff.StartTime, buff.EndTime));
            //        offset += 15;
            //    }
            //};

            //Obj_AI_Base.OnProcessSpellCast += (sender, spell) =>
            //{
            //    if (!sender.IsValid<Obj_AI_Hero>())
            //    {
            //        return;
            //    }

            //    try
            //    {
            //        if (!Orbwalking.IsAutoAttack(spell.SData.Name))
            //        {
            //            var text = string.Format(
            //                "{0};{1};{2};{3};{4};{5};{6};{7};{8}{9}\n", Environment.TickCount, sender.BaseSkinName,
            //                spell.SData.Name, spell.SData.CastRadius[0], spell.SData.CastRange[0],
            //                spell.SData.CastRangeDisplayOverride[0], spell.SData.LineWidth, spell.SData.MissileSpeed,
            //                spell.SData.SpellCastTime, spell.SData.SpellTotalTime);
            //            File.AppendAllText("D:/OnProcessSpellCast-" + Game.Id + ".csv", text);
            //        }
            //    }
            //    catch (Exception e)
            //    {
            //        Console.WriteLine(e);
            //    }
            //};

            //GameObject.OnCreate += (sender, eventArgs) =>
            //{
            //    if (!sender.IsValid<Obj_SpellMissile>())
            //    {
            //        return;
            //    }

            //    try
            //    {
            //        var miss = (Obj_SpellMissile) sender;

            //        if (!miss.SpellCaster.IsValid<Obj_AI_Hero>())
            //        {
            //            return;
            //        }

            //        var text = string.Format(
            //            "{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10}\n", Environment.TickCount, miss.Type, miss.Name,
            //            miss.SData.Name, miss.SData.CastRadius[0], miss.SData.CastRange[0],
            //            miss.SData.CastRangeDisplayOverride[0], miss.SData.LineWidth, miss.SData.MissileSpeed,
            //            miss.SData.SpellCastTime, miss.SData.SpellTotalTime);
            //        File.AppendAllText("D:/Obj_SpellMissile-" + Game.Id + ".csv", text);
            //    }
            //    catch (Exception e)
            //    {
            //        Console.WriteLine(e);
            //    }
            //};
        }
    }
}