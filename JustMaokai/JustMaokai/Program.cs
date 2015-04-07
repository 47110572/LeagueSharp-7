﻿using System;
using System.Diagnostics.Eventing.Reader;
using System.Drawing.Printing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Common.Data;
using Color = System.Drawing.Color;
using JustOlaf;

namespace JustMaokai
{
    internal class Program
    {
        public const string ChampName = "Maokai";
        public static HpBarIndicator Hpi = new HpBarIndicator();
        public static Menu Config;
        public static Orbwalking.Orbwalker Orbwalker;
        public static Spell Q, W, E, R, Smite;
        public static SpellSlot smiteSlot = SpellSlot.Unknown;
        //Credits to Kurisu for Smite Stuff :^)
        public static readonly int[] SmitePurple = { 3713, 3726, 3725, 3726, 3723 };
        public static readonly int[] SmiteGrey = { 3711, 3722, 3721, 3720, 3719 };
        public static readonly int[] SmiteRed = { 3715, 3718, 3717, 3716, 3714 };
        public static readonly int[] SmiteBlue = { 3706, 3710, 3709, 3708, 3707 };

        private static SpellSlot Ignite;
        private static readonly Obj_AI_Hero player = ObjectManager.Player;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnLoad;

        }

        private static void OnLoad(EventArgs args)
        {
            if (player.ChampionName != ChampName)
                return;

            Notifications.AddNotification("JustMaokai - [V.1.0.1.0]", 8000);

            Killsteal();
            GetSmiteSlot();
           
            //Ability Information - Range - Variables.
            Q = new Spell(SpellSlot.Q, 600);
            Q.SetSkillshot(0.50f, 110f, 1200f, false, SkillshotType.SkillshotLine);
            W = new Spell(SpellSlot.W, 525);
            E = new Spell(SpellSlot.E, 1100);
            E.SetSkillshot(1f, 250f, 1500f, false, SkillshotType.SkillshotCircle);
            R = new Spell(SpellSlot.R, 625);

            Config = new Menu(player.ChampionName, player.ChampionName, true);
            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            //Mana
            Config.AddSubMenu(new Menu("Mana", "Mana"));
            Config.SubMenu("Mana").AddItem(new MenuItem("qmana", "[Q] Mana %").SetValue(new Slider(10, 100, 0)));
            Config.SubMenu("Mana").AddItem(new MenuItem("wmana", "[W] Mana %").SetValue(new Slider(10, 100, 0)));
            Config.SubMenu("Mana").AddItem(new MenuItem("emana", "[E] Mana %").SetValue(new Slider(10, 100, 0)));
            Config.SubMenu("Mana").AddItem(new MenuItem("rmana", "[R] Mana %").SetValue(new Slider(15, 100, 0)));
            
            //Combo
            Config.AddSubMenu(new Menu("Combo", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQ", "Use Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseW", "Use W").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseE", "Use E").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("manualr", "Cast R Manual").SetValue(new KeyBind('R', KeyBindType.Press)));
            Config.SubMenu("Combo").AddItem(new MenuItem("Rkil", "Cancel Ult If Target Killable"));
            Config.SubMenu("Combo").AddItem(new MenuItem("Rene", "Min Enemies for R").SetValue(new Slider(2, 1, 5)));
            Config.SubMenu("Combo").AddItem(new MenuItem("useSmiteCombo", "Use Smite").SetValue(true));
           
            //Harass
            Config.AddSubMenu(new Menu("Harass", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("hQ", "Use Q").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("hW", "Use W").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("hE", "Use E").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("harassmana", "Mana Percentage").SetValue(new Slider(30, 100, 0)));

            //Item
            Config.AddSubMenu(new Menu("Item", "Item"));
            Config.SubMenu("Item").AddItem(new MenuItem("useGhostblade", "Use Youmuu's Ghostblade").SetValue(true));
            Config.SubMenu("Item").AddItem(new MenuItem("UseBOTRK", "Use Blade of the Ruined King").SetValue(true));
            Config.SubMenu("Item").AddItem(new MenuItem("eL", "  Enemy HP Percentage").SetValue(new Slider(80, 100, 0)));
            Config.SubMenu("Item").AddItem(new MenuItem("oL", "  Own HP Percentage").SetValue(new Slider(65, 100, 0)));
            Config.SubMenu("Item").AddItem(new MenuItem("UseBilge", "Use Bilgewater Cutlass").SetValue(true));
            Config.SubMenu("Item").AddItem(new MenuItem("HLe", "  Enemy HP Percentage").SetValue(new Slider(80, 100, 0)));
            Config.SubMenu("Item").AddItem(new MenuItem("UseIgnite", "Use Ignite").SetValue(true));

            //Laneclear
            Config.AddSubMenu(new Menu("Laneclear", "Laneclear"));
            Config.SubMenu("Laneclear").AddItem(new MenuItem("laneQ", "Use Q").SetValue(true));
            Config.SubMenu("Laneclear").AddItem(new MenuItem("laneW", "Use W").SetValue(true));
            Config.SubMenu("Laneclear").AddItem(new MenuItem("laneE", "Use E").SetValue(true));
            Config.SubMenu("Laneclear").AddItem(new MenuItem("laneclearmana", "Mana Percentage").SetValue(new Slider(30, 100, 0)));
                        
            //JungleClear
            Config.AddSubMenu(new Menu("Jungleclear", "Jungleclear"));
            Config.SubMenu("Jungleclear").AddItem(new MenuItem("jungleQ", "Use Q").SetValue(true));
            Config.SubMenu("Jungleclear").AddItem(new MenuItem("jungleW", "Use W").SetValue(true));
            Config.SubMenu("Jungleclear").AddItem(new MenuItem("jungleE", "Use E").SetValue(true));
            Config.SubMenu("Jungleclear").AddItem(new MenuItem("jungleclearmana", "Mana Percentage").SetValue(new Slider(30, 100, 0)));

            //Draw
            Config.AddSubMenu(new Menu("Draw", "Draw"));
            Config.SubMenu("Draw").AddItem(new MenuItem("Draw_Disabled", "Disable All Spell Drawings").SetValue(false));
            Config.SubMenu("Draw").AddItem(new MenuItem("Qdraw", "Draw Q Range").SetValue(new Circle(true, Color.Orange)));
            Config.SubMenu("Draw").AddItem(new MenuItem("Wdraw", "Draw W Range").SetValue(new Circle(true, Color.AntiqueWhite)));
            Config.SubMenu("Draw").AddItem(new MenuItem("Edraw", "Draw E Range").SetValue(new Circle(true, Color.Green)));
            Config.SubMenu("Draw").AddItem(new MenuItem("Rdraw", "Draw R Range").SetValue(new Circle(true, Color.Red)));
            
            //Misc
            Config.AddSubMenu(new Menu("Misc", "Misc"));
            Config.SubMenu("Misc").AddItem(new MenuItem("Ksq", "Killsteal with Q").SetValue(false));
            Config.SubMenu("Misc").AddItem(new MenuItem("Ksq", "Killsteal with W").SetValue(false));
            Config.SubMenu("Misc").AddItem(new MenuItem("DrawD", "Damage Indicator").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("interrupt", "Interrupt Spells").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("antigap", "AntiGapCloser").SetValue(true));
            
            Config.AddToMainMenu();
            Drawing.OnDraw += OnDraw;
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnEndScene += OnEndScene;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            }

        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (W.IsReady() && sender.IsValidTarget(W.Range) && Config.Item("interrupt").GetValue<bool>())
                W.CastOnUnit(sender);
        }

        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Q.IsReady() && gapcloser.Sender.IsValidTarget(Q.Range) && Config.Item("antigap").GetValue<bool>())
                Q.CastIfHitchanceEquals(gapcloser.Sender, HitChance.High);
        }
                       
        public static string GetSmiteType()
        {
            if (SmiteBlue.Any(id => Items.HasItem(id)))
            {
                return "s5_summonersmiteplayerganker";
            }
            if (SmiteRed.Any(id => Items.HasItem(id)))
            {
                return "s5_summonersmiteduel";
            }
            if (SmiteGrey.Any(id => Items.HasItem(id)))
            {
                return "s5_summonersmitequick";
            }
            if (SmitePurple.Any(id => Items.HasItem(id)))
            {
                return "itemsmiteaoe";
            }
            return "summonersmite";
        }

        private static void OnEndScene(EventArgs args)
        {
            if (Config.SubMenu("Draw").Item("DrawD").GetValue<bool>())
            {
                foreach (var enemy in
                    ObjectManager.Get<Obj_AI_Hero>().Where(ene => !ene.IsDead && ene.IsEnemy && ene.IsVisible))
                {
                    Hpi.unit = enemy;
                    Hpi.drawDmg(CalcDamage(enemy), Color.Green);
                }
            }
        }

        private static void combo()
        {
            var target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValidTarget()) //if there is no target or target isn't valid it will return; (It won't combo)
                return;

            if (Config.Item("useSmiteCombo").GetValue<bool>())
            {
                UseSmiteOnChamp(target);
            }

            var wmana = Config.Item("wmana").GetValue<Slider>().Value;
            if (W.IsReady() && target.IsValidTarget(W.Range) && Config.Item("UseW").GetValue<bool>())
                W.CastOnUnit(target);

            var emana = Config.Item("emana").GetValue<Slider>().Value;
            if (E.IsReady() && target.IsValidTarget(E.Range) && player.ManaPercent >= emana)
                E.CastIfHitchanceEquals(target, HitChance.High);

            var qmana = Config.Item("qmana").GetValue<Slider>().Value;
            if (Q.IsReady() && Config.Item("UseQ").GetValue<bool>() && target.IsValidTarget(Q.Range)
            && player.ManaPercent >= qmana)
                Q.CastIfHitchanceEquals(target, HitChance.High);
           
            var rmana = Config.Item("rmana").GetValue<Slider>().Value;
            if (R.IsReady() && Config.Item("UseR").GetValue<bool>() && target.IsValidTarget(R.Range) && player.ManaPercent >= rmana)
            Ult();

            var rDmg = player.GetSpellDamage(target, SpellSlot.R);
            if (Config.Item("Rkill").GetValue<bool>() && target.HealthPercent <= rDmg)
            R.Cast();
            
            if (Config.Item("manualr").GetValue<KeyBind>().Active && R.IsReady())
            R.Cast();

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                items();
            }

        private static void Ult()
        {
            int enemys = Utility.CountEnemiesInRange(650);
            var rmana = Config.Item("rmana").GetValue<Slider>().Value;
            var PR = player.Mana * 100 / player.MaxMana;

            if (Config.Item("Rene").GetValue<Slider>().Value <= enemys && PR >= rmana)
            {
                R.Cast();
            }

        }

        private static int CalcDamage(Obj_AI_Base target)
        {
            //The only dmg spells olaf has are E and Q (Added those and removed R/W)
            var aa = player.GetAutoAttackDamage(target, true) * (1 + player.Crit);
            var damage = aa;
            Ignite = player.GetSpellSlot("summonerdot");

            if (Ignite.IsReady())
                damage += player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);

            if (Items.HasItem(3153) && Items.CanUseItem(3153))
                damage += player.GetItemDamage(target, Damage.DamageItems.Botrk); //ITEM BOTRK

            if (Items.HasItem(3144) && Items.CanUseItem(3144))
                damage += player.GetItemDamage(target, Damage.DamageItems.Bilgewater); //ITEM BOTRK

            if (R.IsReady() && Config.Item("UseR").GetValue<bool>()) // rdamage
            {
                if (R.IsReady())
                {
                    damage += R.GetDamage(target);
                }
            }

            if (Q.IsReady() && Config.Item("UseQ").GetValue<KeyBind>().Active) // qdamage
            {

                damage += Q.GetDamage(target);
            }

            if (E.IsReady() && Config.Item("UseE").GetValue<KeyBind>().Active) // edamage
            {

                damage += E.GetDamage(target);
            }

            if (W.IsReady() && Config.Item("UseW").GetValue<KeyBind>().Active) // wdamage
            {

                damage += W.GetDamage(target);
            }
            return (int)damage;
        }

        private static float IgniteDamage(Obj_AI_Hero target)
        {
            if (Ignite == SpellSlot.Unknown || player.Spellbook.CanUseSpell(Ignite) != SpellState.Ready)
                return 0f;
            return (float)player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
        }

        private static void UnderTower()
        {
            var Target = TargetSelector.GetTarget(W.Range + W.Width, TargetSelector.DamageType.Magical);

            if (Utility.UnderTurret(Target, false) && W.IsReady() && Config.Item("UnderTower").GetValue<bool>())
            {
                W.Cast(Target);
            }
        }

        private static void Killsteal()
        {
            foreach (Obj_AI_Hero target in
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(hero => hero.IsValidTarget(Q.Range) && !hero.HasBuffOfType(BuffType.Invulnerability) && hero.IsEnemy)
                )
            {
                var qDmg = player.GetSpellDamage(target, SpellSlot.Q);
                if (Config.Item("ksQ").GetValue<bool>() && target.IsValidTarget(Q.Range) && target.Health <= qDmg)
                {
                    Q.CastIfHitchanceEquals(target, HitChance.High);
                }
                var wDmg = player.GetSpellDamage(target, SpellSlot.W);
                if (Config.Item("ksW").GetValue<bool>() && target.IsValidTarget(W.Range) && target.Health <= wDmg)
                {
                    W.CastOnUnit(target);
                }
            }
        }

        
        private static void items()
        {
            Ignite = player.GetSpellSlot("summonerdot");
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (target == null || !target.IsValidTarget())
                return;

            var botrk = ItemData.Blade_of_the_Ruined_King.GetItem();
            var Ghost = ItemData.Youmuus_Ghostblade.GetItem();
            var cutlass = ItemData.Bilgewater_Cutlass.GetItem();

            if (botrk.IsReady() && botrk.IsOwned(player) && botrk.IsInRange(target)
            && target.HealthPercent <= Config.Item("eL").GetValue<Slider>().Value
            && Config.Item("UseBOTRK").GetValue<bool>())

                botrk.Cast(target);

            if (botrk.IsReady() && botrk.IsOwned(player) && botrk.IsInRange(target)
                && target.HealthPercent <= Config.Item("oL").GetValue<Slider>().Value
                && Config.Item("UseBOTRK").GetValue<bool>())

                botrk.Cast(target);

            if (cutlass.IsReady() && cutlass.IsOwned(player) && cutlass.IsInRange(target) &&
                target.HealthPercent <= Config.Item("HLe").GetValue<Slider>().Value
                && Config.Item("UseBilge").GetValue<bool>())

                cutlass.Cast(target);

            if (Ghost.IsReady() && Ghost.IsOwned(player) && target.IsValidTarget(E.Range)
                && Config.Item("useGhostblade").GetValue<bool>())

                Ghost.Cast();

            if (player.Distance(target.Position) <= 600 && IgniteDamage(target) >= target.Health &&
                Config.Item("UseIgnite").GetValue<bool>())
                player.Spellbook.CastSpell(Ignite, target);
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            Killsteal();

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    harass();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    Laneclear();
                    Jungleclear();
                    break;
            }
        }

        private static void harass()
        {
            var harassmana = Config.Item("harassmana").GetValue<Slider>().Value;
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
            if (target == null || !target.IsValidTarget())
                return;

           if (W.IsReady()
                && Config.Item("hW").GetValue<bool>()
                && player.ManaPercent >= harassmana)

                W.CastOnUnit(target);
          
            if (Q.IsReady()
               && Config.Item("hQ").GetValue<bool>()
               && target.IsValidTarget(Q.Range)
               && player.ManaPercent >= harassmana)

               Q.CastIfHitchanceEquals(target, HitChance.High);

            if (E.IsReady()
                && Config.Item("hE").GetValue<bool>()
                && target.IsValidTarget(E.Range)
                && player.ManaPercent >= harassmana)

                E.CastIfHitchanceEquals(target, HitChance.High);
        }

        private static void Laneclear()
        {
            var lanemana = Config.Item("laneclearmana").GetValue<Slider>().Value;

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear
                && Config.Item("laneQ").GetValue<bool>()
                && player.ManaPercent >= lanemana)

                Q.CastIfHitchanceEquals(minion, HitChance.High);

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear
               && Config.Item("laneW").GetValue<bool>()
               && player.ManaPercent >= lanemana)

                W.CastOnUnit(minion);

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear
                && Config.Item("laneE").GetValue<bool>()
                && player.ManaPercent >= lanemana)

                E.CastIfHitchanceEquals(minion, HitChance.High);

        }


        private static void Jungleclear()
        {
            var jlanemana = Config.Item("jungleclearmana").GetValue<Slider>().Value;

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear
                && Config.Item("jungleW").GetValue<bool>()
                && player.ManaPercent >= jlanemana)

                W.CastOnUnit(minion);

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear
                && Config.Item("jungleE").GetValue<bool>()
                && player.ManaPercent >= jlanemana)

                E.CastIfHitchanceEquals(minion, HitChance.High);

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear
                && Config.Item("jungleQ").GetValue<bool>()
                && player.ManaPercent >= jlanemana)

                Q.CastIfHitchanceEquals(minion, HitChance.High);
        }

        private static void OnDraw(EventArgs args)
        {
            {

            }

            if (Config.Item("Draw_Disabled").GetValue<bool>())
                return;

            if (Config.Item("Qdraw").GetValue<Circle>().Active)
                if (Q.Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, Q.IsReady() ? Config.Item("Qdraw").GetValue<Circle>().Color : Color.Red);
            
            if (Config.Item("Wdraw").GetValue<Circle>().Active)
                if (W.Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, W.IsReady() ? Config.Item("Wdraw").GetValue<Circle>().Color : Color.Green);

            if (Config.Item("Edraw").GetValue<Circle>().Active)
                if (E.Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range - 1,
                        E.IsReady() ? Config.Item("Edraw").GetValue<Circle>().Color : Color.Red);

            if (Config.Item("Rdraw").GetValue<Circle>().Active)
                if (R.Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range, R.IsReady() ? Config.Item("Rdraw").GetValue<Circle>().Color : Color.White);

            var orbtarget = Orbwalker.GetTarget();
            Render.Circle.DrawCircle(orbtarget.Position, 100, Color.DarkOrange, 10);
        }


        public static void UseSmiteOnChamp(Obj_AI_Hero target)
        {
            if (target.IsValidTarget(E.Range) && smiteSlot != SpellSlot.Unknown &&
                ObjectManager.Player.Spellbook.CanUseSpell((smiteSlot)) == SpellState.Ready &&
                (GetSmiteType() == "s5_summonersmiteplayerganker" ||
                 GetSmiteType() == "s5_summonersmiteduel"))
            {
                ObjectManager.Player.Spellbook.CastSpell(smiteSlot, target);
            }
        }

        public static void GetSmiteSlot()
        {
            foreach (
                var spell in
                    ObjectManager.Player.Spellbook.Spells.Where(
                        spell => String.Equals(spell.Name, GetSmiteType(), StringComparison.CurrentCultureIgnoreCase)))
            {
                smiteSlot = spell.Slot;
                Smite = new Spell(smiteSlot, 700);
                return;
            }
        }

        public static Obj_AI_Base minion { get; set; }
    }
}
