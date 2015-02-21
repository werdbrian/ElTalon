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

            AddNotification("ElTalon by jQuery v1.1");

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

        #region itemusage

        private static void clearItems()
        {
            if (Items.CanUseItem(3074))
                Items.UseItem(3074);

            if (Items.CanUseItem(3077))
                Items.UseItem(3077);
        }

        #endregion

        #region Harass

        private static void LaneClear()
        {
            var minion = MinionManager.GetMinions(Player.ServerPosition, W.Range).FirstOrDefault();
            clearItems();
            if (minion == null || minion.Name.ToLower().Contains("ward")) return;

            var qWaveClear = _menu.Item("WaveClearQ").GetValue<bool>();
            var wWaveClear = _menu.Item("WaveClearW").GetValue<bool>();
            var eWaveClear = _menu.Item("WaveClearE").GetValue<bool>();

            if (Player.ManaPercentage() >= _menu.Item("LaneClearMana").GetValue<Slider>().Value)
            {
                if (qWaveClear && Q.IsReady())
                {
                    Q.Cast(Player);
                }

                if (wWaveClear && W.IsReady())
                {
                    W.CastOnUnit(minion);
                }
                if (eWaveClear && E.IsReady())
                {
                    E.CastOnUnit(minion);
                }
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
                        W.CastOnUnit(target);
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
            var Youmuuitem = _menu.Item("UseYoumuu").GetValue<bool>();
            var ultCount = _menu.Item("rcount").GetValue<Slider>().Value;

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

                if (spell.Slot == SpellSlot.R && rCombo && R.IsReady() 
                    && ObjectManager.Get<Obj_AI_Hero>().Count(hero => hero.IsValidTarget(R.Range)) >= ultCount)
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

            if (Ignite != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(Ignite) == SpellState.Ready)
            {
                damage += ObjectManager.Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);
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

            waveClearMenu.AddItem(new MenuItem("WaveClearActive", "WaveClear!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            //Misc
            var miscMenu = _menu.AddSubMenu(new Menu("Drawings", "Misc"));
            miscMenu.AddItem(new MenuItem("Drawingsoff", "Drawings off").SetValue(false));
            miscMenu.AddItem(new MenuItem("DrawW", "Draw W").SetValue(true));
            miscMenu.AddItem(new MenuItem("DrawE", "Draw E").SetValue(true));
            miscMenu.AddItem(new MenuItem("DrawR", "Draw R").SetValue(true));

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