using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;


namespace ElTalon
{
    /// <summary>
    ///     Handle all stuff what is going on with Talon.
    /// </summary>
    internal class Talon
    {

        private static String hero = "Talon";
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        private static Menu _menu;
        private static Orbwalking.Orbwalker _orbwalker;
        private static Spell Q, W, E, R;
        private static List<Spell> SpellList;

        // Items
        private static Items.Item Tiamat, Hydra, Youmuu;

        // Summoner spells
        private static SpellSlot Ignite;

        //this is godlike
        private const float Jqueryluckynumber = 400f;


        #region Gameloaded 

        public static void Game_OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.BaseSkinName != "Talon")
                return;

            AddNotification("ElTalon by jQuery v1.2");

            #region Spell Data

            // set spells
            Q = new Spell(SpellSlot.Q, 0);
            W = new Spell(SpellSlot.W, 650);
            E = new Spell(SpellSlot.E, 700);
            R = new Spell(SpellSlot.R, 650);

            SpellList = new List<Spell> { Q, E, W, R };

            // Ignite
            Ignite = Player.GetSpellSlot("summonerdot");

            // Items
            Tiamat = new Items.Item(3077, 400f);
            Youmuu = new Items.Item(3142, 0f);
            Hydra = new Items.Item(3074, 400f); 

            InitializeMenu();

            #endregion

            //subscribe to event
            Game.OnGameUpdate += OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.AfterAttack += AfterAttack;
        }

        #endregion

        #region OnGameUpdate

        private static void OnGameUpdate(EventArgs args)
        {
            switch (_orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                break;

                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                break;

                case Orbwalking.OrbwalkingMode.LaneClear:
                    LaneClear();
               break;
            }
        }

        #endregion

        #region Harass

        private static void LaneClear()
        {
            var minion = MinionManager.GetMinions(Player.ServerPosition, W.Range).FirstOrDefault();
            if (minion == null || minion.Name.ToLower().Contains("ward")) return;

            var qWaveClear = _menu.Item("WaveClearQ").GetValue<bool>();
            var wWaveClear = _menu.Item("WaveClearW").GetValue<bool>();
            var eWaveClear = _menu.Item("WaveClearE").GetValue<bool>();
            var hydraClear = _menu.Item("HydraClear").GetValue<bool>();
            var tiamatClear = _menu.Item("TiamatClear").GetValue<bool>();
            var bestFarmLocation = MinionManager.GetBestCircularFarmLocation(MinionManager.GetMinions(W.Range, MinionTypes.All, MinionTeam.Enemy).Select(m => m.ServerPosition.To2D()).ToList(), W.Width, W.Range);
            var minions = MinionManager.GetMinions(Player.ServerPosition, W.Range, MinionTypes.All, MinionTeam.NotAlly);

            if (Player.ManaPercentage() >= _menu.Item("LaneClearMana").GetValue<Slider>().Value)
            {
                if (qWaveClear && Q.IsReady() && minion.IsValidTarget())
                {
                    Q.Cast(Player);
                }

                if (wWaveClear && W.IsReady() && minion.IsValidTarget())
                {
                    W.Cast(bestFarmLocation.Position);
                    //W.CastOnUnit(minion);
                }
                if (eWaveClear && E.IsReady())
                {
                    E.CastOnUnit(minion);
                }

                if (Items.CanUseItem(3074) && hydraClear && minion.IsValidTarget(Hydra.Range) && minions.Count() > 1)
                    Items.UseItem(3074);

                if (Items.CanUseItem(3077) && tiamatClear && minion.IsValidTarget(Tiamat.Range) && minions.Count() > 1)
                    Items.UseItem(3077);
            }
        }

        #endregion  

        #region Harass

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);
            if (target == null || !target.IsValid)
            {
                return;
            }

            var qHarass = _menu.Item("HarassQ").GetValue<bool>();
            var wHarass = _menu.Item("HarassW").GetValue<bool>();
            var eHarass = _menu.Item("HarassE").GetValue<bool>();

            foreach (var spell in SpellList.Where(y => y.IsReady()))
            {
                if (Player.ManaPercentage() >= _menu.Item("HarassMana").GetValue<Slider>().Value)
                {                             
                   if (spell.Slot == SpellSlot.Q && qHarass && Q.IsReady() && Player.Distance(target) <= Jqueryluckynumber && Q.IsReady())
                    {   
                        Q.Cast(Player);
                    }

                    if (spell.Slot == SpellSlot.W && wHarass && W.IsReady())
                    {
                        W.CastIfHitchanceEquals(target, HitChance.High);
                        //W.CastOnUnit(target);
                    }

                    if (spell.Slot == SpellSlot.E && eHarass && E.IsReady())
                    {
                        E.Cast(target);
                    }
                }
            }
        }

        #endregion

        #region itemusage

        private static void fightItems()
        {
            var target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);

            var TiamatItem = _menu.Item("UseTiamat").GetValue<bool>();
            var HydraItem = _menu.Item("UseHydra").GetValue<bool>();
   
            if (Items.CanUseItem(3074) && HydraItem && Player.Distance(target) <= Jqueryluckynumber)
                Items.UseItem(3074);

            if (Items.CanUseItem(3077) && TiamatItem && Player.Distance(target) <= Jqueryluckynumber)
                Items.UseItem(3077);
        }

        #endregion

        private static void AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            switch (_orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    if (unit.IsMe && Q.IsReady() && target is Obj_AI_Hero)
                    {
                        Q.Cast();
                        fightItems();
                        Orbwalking.ResetAutoAttackTimer();
                    }
                break;
            }
        }

        #region Combo

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);
            if (target == null || !target.IsValid)
            {
                return;
            }

            var wCombo = _menu.Item("WCombo").GetValue<bool>();
            var eCombo = _menu.Item("ECombo").GetValue<bool>();
            var rCombo = _menu.Item("RCombo").GetValue<bool>();
            var onlyKill = _menu.Item("RWhenKill").GetValue<bool>();
            var smartUlt = _menu.Item("SmartUlt").GetValue<bool>();
            var Youmuuitem = _menu.Item("UseYoumuu").GetValue<bool>();
            var ultCount = _menu.Item("rcount").GetValue<Slider>().Value;

            var comboDamage = GetComboDamage(target);
            var getUltComboDamage = GetUltComboDamage(target);

            foreach (var spell in SpellList.Where(x => x.IsReady()))
            {

                if (spell.Slot == SpellSlot.W && wCombo && W.IsReady())
                {
                    W.CastOnUnit(target);
                }

                if (spell.Slot == SpellSlot.E && eCombo && E.IsReady())
                {
                    E.CastOnUnit(target);
                }

                //only kill with ult
                if (onlyKill && E.IsReady() && rCombo && Q.IsReady() && ObjectManager.Get<Obj_AI_Hero>().Count(hero => hero.IsValidTarget(R.Range)) >= ultCount)
                {
                    if (comboDamage >= target.Health)
                    {
                        R.CastOnUnit(Player);
                    }
                }

                // When fighting and target can we killed with ult it will ult
                if (onlyKill && R.IsReady() && smartUlt)
                {
                    if (getUltComboDamage >= target.Health)
                    {
                        R.CastOnUnit(Player);
                    }
                }

                //not active
                if (!onlyKill && E.IsReady() && rCombo && Q.IsReady() && ObjectManager.Get<Obj_AI_Hero>().Count(hero => hero.IsValidTarget(R.Range)) >= ultCount)
                {
                    R.CastOnUnit(Player);
                }

                if (Youmuuitem && Player.Distance(target) <= Jqueryluckynumber && Youmuu.IsReady())
                {
                    Youmuu.Cast(Player);
                }
            }

            //ignite when killable
            if (Player.Distance(target) <= 600 && IgniteDamage(target) >= target.Health &&
                _menu.Item("UseIgnite").GetValue<bool>())
            {
                Player.Spellbook.CastSpell(Ignite, target);
            }
        }

        #endregion

        //new logic such OP 
        #region GetUltComboDamage   

        private static float GetUltComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;

            if (R.IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.R);
            }

            return (float)damage;
        }

        #endregion


        #region GetComboDamage   

        private static float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;

            if (Q.IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);
            }

            if (W.IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.W);
            }

            if (E.IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);
            }

            if (R.IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.R);
            }

            return (float)damage;
        }

        #endregion

        #region Ignite

        private static float IgniteDamage(Obj_AI_Hero target)
        {
            if (Ignite == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(Ignite) != SpellState.Ready)
            {
                return 0f;
            }
            return (float)Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
        }

        #endregion

        #region Drawings

        private static void Drawing_OnDraw(EventArgs args)
        {

            if (_menu.Item("Drawingsoff").GetValue<bool>())
                return;

            if (_menu.Item("DrawW").GetValue<bool>())
                if (W.Level > 0)
                    Utility.DrawCircle(Player.Position, W.Range, W.IsReady() ? Color.Green : Color.Red);

            if (_menu.Item("DrawE").GetValue<bool>())
                if (E.Level > 0)
                    Utility.DrawCircle(Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);

            if (_menu.Item("DrawR").GetValue<bool>())
                if (E.Level > 0)
                    Utility.DrawCircle(Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);
       
                Utility.HpBarDamageIndicator.DamageToUnit = GetComboDamage;
                Utility.HpBarDamageIndicator.Enabled = _menu.Item("DrawComboDamage").GetValue<bool>();
        }

        #endregion


        #region Notification

        public static void AddNotification(String text)
        {
            var notification = new Notification(text, 10000);
            Notifications.AddNotification(notification);
        }

        #endregion

        #region Menu

        private static void InitializeMenu()
        {
            _menu = new Menu("ElTalon", hero, true);

            //Orbwalker
            var orbwalkerMenu = new Menu("Orbwalker", "orbwalker");
            _orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            _menu.AddSubMenu(orbwalkerMenu);

            //TargetSelector
            var targetSelector = new Menu("Target Selector", "TargetSelector");
            TargetSelector.AddToMenu(targetSelector);
            _menu.AddSubMenu(targetSelector);

            //Combo
            var comboMenu = _menu.AddSubMenu(new Menu("Combo", "Combo"));
            comboMenu.AddItem(new MenuItem("fsfsafsaasffsadddd111dsasd", ""));
            comboMenu.AddItem(new MenuItem("QCombo", "Use Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("WCombo", "Use W").SetValue(true));
            comboMenu.AddItem(new MenuItem("ECombo", "Use E").SetValue(true));
            comboMenu.AddItem(new MenuItem("RCombo", "Use R").SetValue(true));
            comboMenu.AddItem(new MenuItem("fsfsafsaasffsa", ""));
            comboMenu.AddItem(new MenuItem("RWhenKill", "Use R only when killable").SetValue(true));
            comboMenu.AddItem(new MenuItem("SmartUlt", "Use smart ult").SetValue(true));
            comboMenu.AddItem(new MenuItem("rcount", "Min target to R >= ")).SetValue(new Slider(1, 1, 5));
            comboMenu.AddItem(new MenuItem("UseIgnite", "Use Ignite in combo when killable").SetValue(true));
            comboMenu.AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            comboMenu.SubMenu("Items").AddItem(new MenuItem("UseTiamat", "Use Tiamat").SetValue(true));
            comboMenu.SubMenu("Items").AddItem(new MenuItem("UseHydra", "Use Hydra").SetValue(true));
            comboMenu.SubMenu("Items").AddItem(new MenuItem("UseYoumuu", "Use Youmuu").SetValue(true));

            //Harass
            var harassMenu = _menu.AddSubMenu(new Menu("Harass", "H"));
            harassMenu.AddItem(new MenuItem("fsfsafsaasffsadddd", ""));
            harassMenu.AddItem(new MenuItem("HarassQ", "Use Q").SetValue(true));
            harassMenu.AddItem(new MenuItem("HarassW", "Use W").SetValue(true));
            harassMenu.AddItem(new MenuItem("HarassE", "Use E").SetValue(false));

            harassMenu.SubMenu("HarassMana").AddItem(new MenuItem("HarassMana", "[Harass] Minimum Mana").SetValue(new Slider(30, 0, 100)));
            harassMenu.AddItem(new MenuItem("HarassActive", "Harass!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));

            //Waveclear
            var waveClearMenu = _menu.AddSubMenu(new Menu("WaveClear", "waveclear"));
            waveClearMenu.AddItem(new MenuItem("fsfsafsaasffsadddd111", ""));
            waveClearMenu.AddItem(new MenuItem("WaveClearQ", "Use Q").SetValue(true));
            waveClearMenu.AddItem(new MenuItem("WaveClearW", "Use W").SetValue(true));
            waveClearMenu.AddItem(new MenuItem("WaveClearE", "Use E").SetValue(false));
            waveClearMenu.SubMenu("LaneClearMana").AddItem(new MenuItem("LaneClearMana", "[WaveClear] Minimum Mana").SetValue(new Slider(30, 0, 100)));
            waveClearMenu.AddItem(new MenuItem("fsfsafsaasffsadddd11sss1", ""));
            
            // item usage
            waveClearMenu.SubMenu("Items").AddItem(new MenuItem("HydraClear", "Use hydra").SetValue(true));
            waveClearMenu.SubMenu("Items").AddItem(new MenuItem("TiamatClear", "Use tiamat").SetValue(true));
            waveClearMenu.AddItem(new MenuItem("WaveClearActive", "WaveClear!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            //Misc
            var miscMenu = _menu.AddSubMenu(new Menu("Drawings", "Misc"));
            miscMenu.AddItem(new MenuItem("Drawingsoff", "Drawings off").SetValue(false));
            miscMenu.AddItem(new MenuItem("DrawW", "Draw W").SetValue(true));
            miscMenu.AddItem(new MenuItem("DrawE", "Draw E").SetValue(true));
            miscMenu.AddItem(new MenuItem("DrawR", "Draw R").SetValue(true));
            miscMenu.AddItem(new MenuItem("DrawComboDamage", "Draw combo damage").SetValue(true));


            //Supersecretsettings - soon
            /*var supersecretsettings = _menu.AddSubMenu(new Menu("SuperSecretSettings", "supersecretsettings"));
            supersecretsettings.AddItem(new MenuItem("DontEUnderTower", "[SSS] Dont E under tower").SetValue(false));*/

            //Here comes the moneyyy, money, money, moneyyyy
            var credits = _menu.AddSubMenu(new Menu("Credits", "jQuery"));
            credits.AddItem(new MenuItem("Thanks", "Powered by:"));
            credits.AddItem(new MenuItem("jQuery", "jQuery"));
            credits.AddItem(new MenuItem("fassfassf", ""));
            credits.AddItem(new MenuItem("Paypal", "Paypal:"));
            credits.AddItem(new MenuItem("Email", "info@zavox.nl"));

            _menu.AddToMainMenu();
        }

        #endregion
    }
}