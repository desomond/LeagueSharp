using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace Lux
{
    internal class Program
    {
        public static Menu Menu;
        private static Obj_AI_Hero Player;
        public static List<Spell> SpellList = new List<Spell>();

        public static Orbwalking.Orbwalker Orbwalker;

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;


        public static SpellSlot SumIgnite = ObjectManager.Player.GetSpellSlot("SummonerDot");

        public static void Main(string[] args)
        {
            Game.OnGameStart += Game_Start;
            if (Game.Mode == GameMode.Running)
            {
                Game_Start(new EventArgs());
            }
        }

        public static void Game_Start(EventArgs args)
        {
            Menu = new Menu("Lux", "Lux", true);

            var TargetSelectorMenu = new Menu("Target Selector", "Target Selector");
            SimpleTs.AddToMenu(TargetSelectorMenu);
            Menu.AddSubMenu(TargetSelectorMenu);


            Menu.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Menu.SubMenu("Orbwalking"));


            //------------Combo
            Menu.AddSubMenu(new Menu("Combo", "Combo"));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseQ", "Use Q").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseE", "Use E").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseR", "Use R").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("DFG", "Use DFG").SetValue(true));

            Menu.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));
            //-------------end Combo


            Menu.AddSubMenu(new Menu("Harass", "Harass"));
            Menu.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(false));
            Menu.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));
            Menu.SubMenu("Harass").AddItem(new MenuItem("HarassActive", "Harass!").SetValue(new KeyBind("A".ToCharArray()[0], KeyBindType.Press)));

            Menu.AddSubMenu(new Menu("Clear", "Clear"));
            Menu.SubMenu("Clear").AddItem(new MenuItem("UseEFarm2", "Use E").SetValue(true));
            Menu.SubMenu("Clear").AddItem(new MenuItem("ClearActive", "Clear!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            Menu.AddSubMenu(new Menu("Misc", "Misc"));
            Menu.SubMenu("Misc").AddItem(new MenuItem("UseW", "Use W when low --not implemented").SetValue(false));
            Menu.SubMenu("Misc").AddItem(new MenuItem("RKS", "R Kill Steal").SetValue(true));
            Menu.SubMenu("Misc").AddItem(new MenuItem("Ignite", "Use Ignite").SetValue(false));
            Menu.SubMenu("Misc").AddItem(new MenuItem("QandE", "Use Q E combo whenever possible").SetValue(false));

            Menu.AddToMainMenu();

            Player = ObjectManager.Player;

            Q = new Spell(SpellSlot.Q, 1175);
            W = new Spell(SpellSlot.W, 1075);
            E = new Spell(SpellSlot.E, 1100);
            R = new Spell(SpellSlot.R, 3340);

            Q.SetSkillshot(.25f, 80f, 1275, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(.15f, 275f, 1500, true, SkillshotType.SkillshotLine);
            E.SetSkillshot(.15f, 275f, 1500, true, SkillshotType.SkillshotCircle);
            R.SetSkillshot(.7f, 190f, 999999, true, SkillshotType.SkillshotLine);


            Game.PrintChat("Lux Loaded.");

            //Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnUpdate;
        }

        public static void Game_OnUpdate(EventArgs args)
        {
            List<Vector2> pos = new List<Vector2>();
            bool eFarm = Menu.Item("UseEFarm2").GetValue<bool>();
            bool qHarass = Menu.Item("UseQHarass").GetValue<bool>();
            bool eHarass = Menu.Item("UseEHarass").GetValue<bool>();
            var harassActive = Menu.Item("HarassActive").GetValue<KeyBind>().Active;
            var clearActive = Menu.Item("ClearActive").GetValue<KeyBind>().Active;
            var comboActive = Menu.Item("ComboActive").GetValue<KeyBind>().Active;

            if (harassActive)
            {
                if (qHarass && Q.IsReady())
                {
                    var t = SimpleTs.GetTarget(1175, SimpleTs.DamageType.Magical);
                    if (t.IsValidTarget())
                    {
                        var predQ = Q.GetPrediction(t);

                        if (predQ.Hitchance == HitChance.Medium || predQ.Hitchance == HitChance.High)
                        {
                                Q.Cast(predQ.CastPosition);
                        }
                    }
                }
                if (eHarass && E.IsReady())
                {
                    var t = SimpleTs.GetTarget(1100, SimpleTs.DamageType.Magical);
                    if (t.IsValidTarget())
                    { 
                        var predE = E.GetPrediction(t);

                        if (predE.Hitchance == HitChance.Medium || predE.Hitchance == HitChance.High)
                        {
                                E.Cast(predE.CastPosition);
                                E.Cast();
                        }
                    }
                }
            }

            if (clearActive && eFarm && E.IsReady())
            {

                var Minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, 1100, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.Health);
                foreach (var minion in Minions)
                {
                    if (minion != null)
                    {
                        pos.Add(minion.Position.To2D());
                    }
                }

                var pred = LeagueSharp.Common.MinionManager.GetBestCircularFarmLocation(pos, 275, 1100);
                if (pos.Any())
                {
                    var eCast = E.GetCircularFarmLocation(pos);
                    E.Cast(eCast.Position);
                    E.Cast();
                }

            }

            if (comboActive)
            {

                var t = SimpleTs.GetTarget(1100, SimpleTs.DamageType.Magical);
                var t2 = SimpleTs.GetTarget(750, SimpleTs.DamageType.Magical);
                var Qdmg = DamageLib.getDmg(t, DamageLib.SpellType.Q);
                var Edmg = DamageLib.getDmg(t, DamageLib.SpellType.E);
                var Rdmg = DamageLib.getDmg(t, DamageLib.SpellType.R);
                if (t.HasBuff("luxilluminatingfraulein"))
                {
                    Rdmg = DamageLib.getDmg(t, DamageLib.SpellType.R) + DamageLib.CalcMagicDmg((10 + (8 * Player.Level) + (0.20 * ObjectManager.Player.FlatMagicDamageMod)), t);
                }
  

                
                var DFGdmg = DamageLib.getDmg(t, DamageLib.SpellType.DFG);

                if (t.IsValidTarget() || t2.IsValidTarget())
                {
                    if ((t.Health < Qdmg)&&Q.IsReady())
                    {
                        var predQ = Q.GetPrediction(t);
                        if ((predQ.Hitchance == HitChance.Medium || predQ.Hitchance == HitChance.High))
                        {
                            Q.Cast(predQ.CastPosition);
                        }
                    }
                    else if ((t.Health < Qdmg + Edmg) && Q.IsReady()&&E.IsReady())
                    {
                        var predQ = Q.GetPrediction(t);
                        var predE = E.GetPrediction(t);
                        if ((predQ.Hitchance == HitChance.Medium || predQ.Hitchance == HitChance.High) && (predE.Hitchance == HitChance.Medium || predE.Hitchance == HitChance.High))
                        {
                            Q.Cast(predQ.CastPosition);
                            E.Cast(predE.CastPosition);
                            E.Cast();
                        }
                    }
                    else if ((t.Health < Qdmg + Edmg + Rdmg) && Q.IsReady() && E.IsReady()&&R.IsReady())
                    {
                        var predQ = Q.GetPrediction(t);
                        var predE = E.GetPrediction(t);
                        var predR = R.GetPrediction(t);
                        if ((predQ.Hitchance == HitChance.Medium || predQ.Hitchance == HitChance.High) && (predE.Hitchance == HitChance.Medium || predE.Hitchance == HitChance.High))
                        {
                            Q.Cast(predQ.CastPosition);
                            E.Cast(predE.CastPosition);
                            R.Cast(predR.CastPosition);
                            E.Cast();
                        }
                    }
                    else if ((t2.Health < (Qdmg + Edmg + Rdmg) + ((Qdmg + Edmg + Rdmg) * .15) + DFGdmg) && Q.IsReady() && E.IsReady() && R.IsReady() && LeagueSharp.Common.Items.CanUseItem("3128"))
                    {
                        var predQ = Q.GetPrediction(t2);
                        var predE = E.GetPrediction(t2);
                        var predR = R.GetPrediction(t2);
                        if ((predQ.Hitchance == HitChance.Medium || predQ.Hitchance == HitChance.High) && (predE.Hitchance == HitChance.Medium || predE.Hitchance == HitChance.High))
                        {
                            LeagueSharp.Common.Items.UseItem("3128");
                            Q.Cast(predQ.CastPosition);
                            E.Cast(predE.CastPosition);
                            R.Cast(predR.CastPosition);
                            E.Cast();
                        }
                    }
                }


                if (Q.IsReady())
                {
                    var t3 = SimpleTs.GetTarget(1175, SimpleTs.DamageType.Magical);
                    if (t3.IsValidTarget())
                    {
                        var predQ = Q.GetPrediction(t3);
                        if (predQ.Hitchance == HitChance.Medium || predQ.Hitchance == HitChance.High)
                        {
                            Q.Cast(predQ.CastPosition);
                            Q.Cast();
                        }
                    }
                }
    
                if (E.IsReady())
                {
                    var t3 = SimpleTs.GetTarget(1100, SimpleTs.DamageType.Magical);
                    if (t3.IsValidTarget())
                    {
                        var predE = E.GetPrediction(t3);
                        if (predE.Hitchance == HitChance.Medium || predE.Hitchance == HitChance.High)
                        {
                            E.Cast(predE.CastPosition);
                            E.Cast();
                        }
                    }
                }
            }
            if (Menu.Item("RKS").GetValue<bool>() && R.IsReady())
            {
                var t = SimpleTs.GetTarget(3340, SimpleTs.DamageType.Magical);
                if (t.IsValidTarget())
                {
                    var dmg = DamageLib.getDmg(t, DamageLib.SpellType.R);
                   
                    if (t.HasBuff("luxilluminatingfraulein"))
                    {
                        dmg = DamageLib.getDmg(t, DamageLib.SpellType.R) + DamageLib.CalcMagicDmg((10 + (8*Player.Level) +(0.20 * ObjectManager.Player.FlatMagicDamageMod)), t);
                    }

                    if (t.Health < dmg)
                    {
                        var pred = R.GetPrediction(t);

                        if (pred.Hitchance == HitChance.Medium || pred.Hitchance == HitChance.High)
                        {
                                R.Cast(pred.CastPosition);
                        }
                    }
                 }
            }
            if (Menu.Item("QandE").GetValue<bool>() && Q.IsReady() && E.IsReady())
            {
                var t = SimpleTs.GetTarget(1150, SimpleTs.DamageType.Magical);
                if (t.IsValidTarget())
                {
                    var predQ = Q.GetPrediction(t);
                    var predE = E.GetPrediction(t);
                        if ((predQ.Hitchance == HitChance.Medium || predQ.Hitchance == HitChance.High)&&(predE.Hitchance == HitChance.Medium || predE.Hitchance == HitChance.High))
                        {
                                Q.Cast(predQ.CastPosition);
                                E.Cast(predE.CastPosition);
                                E.Cast();
                        }
                }
            }
            if (Menu.Item("Ignite").GetValue<bool>())
            {
                var t = SimpleTs.GetTarget(600, SimpleTs.DamageType.Physical);
                var igniteDmg = DamageLib.getDmg(t, DamageLib.SpellType.IGNITE);
                if (t != null && SumIgnite != SpellSlot.Unknown && ObjectManager.Player.SummonerSpellbook.CanUseSpell(SumIgnite) == SpellState.Ready)
                {
                    if (igniteDmg > t.Health)
                    {
                        ObjectManager.Player.SummonerSpellbook.CastSpell(SumIgnite, t);
                    }
                }
            }
        }
    }
}