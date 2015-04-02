using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System.Collections.Generic;
using Color = System.Drawing.Color;

namespace garen
{
    internal class Program
    {
        public static Menu Menu;
        private static Obj_AI_Hero Player;
        public static List<Spell> SpellList = new List<Spell>();

        public static Orbwalking.Orbwalker Orbwalker;

        public static Spell W;
        public static Spell R;


        public static SpellSlot SumIgnite = ObjectManager.Player.GetSpellSlot("SummonerDot");

        public static void Main(string[] args)
        {
            Game.OnStart += Game_Start;
            if (Game.Mode == GameMode.Running)
            {
                Game_Start(new EventArgs());
            }
        }

        public static void Game_Start(EventArgs args)
        {
            Menu = new Menu("Garen", "Garen", true);

            var TargetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(TargetSelectorMenu);
            Menu.AddSubMenu(TargetSelectorMenu);


            Menu.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Menu.SubMenu("Orbwalking"));


            Menu.AddSubMenu(new Menu("Misc", "Misc"));
            Menu.SubMenu("Misc").AddItem(new MenuItem("RKS", "R Kill Steal").SetValue(true));
            Menu.SubMenu("Misc").AddItem(new MenuItem("Ignite", "Use Ignite").SetValue(false));
            Menu.SubMenu("Misc").AddItem(new MenuItem("useW", "Use w").SetValue(true));
           
            Menu.AddSubMenu(new Menu("Draw", "Draw"));
            Menu.SubMenu("Draw").AddItem(new MenuItem("drawKill", "Draw Killibility").SetValue(true));
            Menu.SubMenu("Draw").AddItem(new MenuItem("drawR", "Draw R range").SetValue(true));

            Menu.AddToMainMenu();

            Player = ObjectManager.Player;

            W = new Spell(SpellSlot.W);
            R = new Spell(SpellSlot.R, 400);

            Game.PrintChat("Garen Loaded.");

            Drawing.OnDraw += OnDraw;
            Game.OnUpdate += Game_OnUpdate;
        }

        public static void Game_OnUpdate(EventArgs args)
        {
            List<Vector2> pos = new List<Vector2>();
            bool useR = Menu.Item("RKS").GetValue<bool>();
            bool useW = Menu.Item("useW").GetValue<bool>();
            bool useIgnite = Menu.Item("Ignite").GetValue<bool>();


            if (useR && R.IsReady())
            {
                var t = TargetSelector.GetTarget(400, TargetSelector.DamageType.Magical);
                if (t.IsValidTarget())
                {
                    var dmg = Damage.GetDamageSpell(ObjectManager.Player, t, SpellSlot.R);
                    var igniteDmg = ObjectManager.Player.GetSummonerSpellDamage(t, Damage.SummonerSpell.Ignite);

                    if (t.Health < dmg.CalculatedDamage)
                    {
                            R.Cast(t, true);           
                    }
                    else if ((t.Health < dmg.CalculatedDamage + igniteDmg) && t != null && SumIgnite != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(SumIgnite) == SpellState.Ready && useIgnite)
                    {
                        Player.Spellbook.CastSpell(SumIgnite, t);
                        R.Cast(t, true);   
                    }
                }
            }
            if (isCCd.Class1.IsCCd(Player) && useW)
            {
                W.Cast(true);
            }
            if (useIgnite)
            {
                var t = TargetSelector.GetTarget(600, TargetSelector.DamageType.Physical);
                var igniteDmg = ObjectManager.Player.GetSummonerSpellDamage(t, Damage.SummonerSpell.Ignite);
                if (t != null && SumIgnite != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(SumIgnite) == SpellState.Ready)
                {
                    if (igniteDmg > t.Health)
                    {
                        Player.Spellbook.CastSpell(SumIgnite, t);
                    }
                }
            }
        

        }

        private static void OnDraw(EventArgs args)
        {
            if (Menu.Item("drawR").GetValue<bool>())
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, System.Drawing.Color.White);
            }
           
            if (Menu.Item("drawKillability").GetValue<bool>())
            {
                foreach (var tar in ObjectManager.Get<Obj_AI_Hero>().Where(unit => unit.IsEnemy && unit.IsVisible && !unit.IsDead))
                {
                    var wts = Drawing.WorldToScreen(tar.Position);
                    var DMG = (float)Player.GetSpellDamage(tar, SpellSlot.R);
                    if ((DMG >= tar.Health))
                    {
                        Drawing.DrawText(wts[0] - 20, wts[1] + 20, Color.Red, "ULT THEM!!");
                    }
                }
            }
        }
    }
}