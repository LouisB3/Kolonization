﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using KolonyTools;
using UnityEngine;
using Random = System.Random;

namespace Kolonization
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class KolonizationMonitor_Flight : KolonizationMonitor
    { }

    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class KolonizationMonitor_SpaceCenter : KolonizationMonitor
    { }

    [KSPAddon(KSPAddon.Startup.TrackingStation, false)]
    public class KolonizationMonitor_TStation : KolonizationMonitor
    { }

    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class KolonizationMonitor_Editor : KolonizationMonitor
    { }

    public class KolonizationMonitor : MonoBehaviour
    {
        private ApplicationLauncherButton kolonyButton;
        private Rect _windowPosition = new Rect(300, 60, 620, 400);
        private GUIStyle _windowStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _scrollStyle;
        private Vector2 scrollPos = Vector2.zero;
        private bool _hasInitStyles = false;

        void Awake()
        {
            var texture = new Texture2D(36, 36, TextureFormat.RGBA32, false);
            var textureFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Kolony.png");
            print("Loading " + textureFile);
            texture.LoadImage(File.ReadAllBytes(textureFile));
            this.kolonyButton = ApplicationLauncher.Instance.AddModApplication(GuiOn, GuiOff, null, null, null, null,
                ApplicationLauncher.AppScenes.ALWAYS, texture);
        }

        private void GuiOn()
        {
            RenderingManager.AddToPostDrawQueue(144, Ondraw);
        }

        public void Start()
        {
            if (!_hasInitStyles)
                InitStyles();
        }

        private void GuiOff()
        {
            RenderingManager.RemoveFromPostDrawQueue(144, Ondraw);
        }


        private void Ondraw()
        {
            _windowPosition = GUILayout.Window(10, _windowPosition, OnWindow, "Kolonization Statistics", _windowStyle);
        }

        private void OnWindow(int windowId)
        {
            GenerateWindow();
        }

        string ColorToHex(Color32 color)
        {
            string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
            return hex;
        }

        private void GenerateWindow()
        {
            GUILayout.BeginVertical();
            scrollPos = GUILayout.BeginScrollView(scrollPos, _scrollStyle, GUILayout.Width(600), GUILayout.Height(350));
            GUILayout.BeginVertical();
            try
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(String.Format("<color=#FFFFFF>Body Name</color>"), _labelStyle, GUILayout.Width(135));
                GUILayout.Label(String.Format("<color=#FFD900>Geology</color>"), _labelStyle, GUILayout.Width(80));
                GUILayout.Label(String.Format("<color=#FFD900>Botany</color>"), _labelStyle, GUILayout.Width(80));
                GUILayout.Label(String.Format("<color=#FFD900>Kolonization</color>"), _labelStyle, GUILayout.Width(80));
                GUILayout.EndHorizontal();

                var planetList = KolonizationManager.Instance.KolonizationInfo.Select(p => p.BodyIndex).Distinct();

                foreach (var p in planetList)
                {
                    var body = FlightGlobals.Bodies[p];
                    var geo = 0d;
                    var kol = 0d;
                    var bot = 0d;
                    foreach(var k in KolonizationManager.Instance.KolonizationInfo.Where(x=>x.BodyIndex == p))
                    {
                        geo += k.GeologyResearch;
                        bot += k.BotanyResearch;
                        kol += k.KolonizationResearch;
                    }
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(String.Format("<color=#FFFFFF>{0}</color>", body.bodyName), _labelStyle, GUILayout.Width(135));
                    GUILayout.Label(String.Format("<color=#FFD900>{0:n2}</color>", geo / 100d), _labelStyle, GUILayout.Width(80));
                    GUILayout.Label(String.Format("<color=#FFD900>{0:n2}</color>", bot / 100d), _labelStyle, GUILayout.Width(80));
                    GUILayout.Label(String.Format("<color=#FFD900>{0:n2}</color>", kol / 100d), _labelStyle, GUILayout.Width(80));
                    GUILayout.EndHorizontal();
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex.StackTrace);
            }
            finally
            {
                GUILayout.EndVertical();
                GUILayout.EndScrollView();
                GUILayout.EndVertical();
                GUI.DragWindow();
            }
        }

        internal void OnDestroy()
        {
            if (kolonyButton == null)
                return;
            ApplicationLauncher.Instance.RemoveModApplication(kolonyButton);
            kolonyButton = null;
        }

        private void InitStyles()
        {
            _windowStyle = new GUIStyle(HighLogic.Skin.window);
            _windowStyle.fixedWidth = 620f;
            _windowStyle.fixedHeight = 400f;
            _labelStyle = new GUIStyle(HighLogic.Skin.label);
            _buttonStyle = new GUIStyle(HighLogic.Skin.button);
            _scrollStyle = new GUIStyle(HighLogic.Skin.scrollView);
            _hasInitStyles = true;
        }
    }

    public class KolonizationDisplayStat
    {
        public string PlanetName { get; set; }
        public string vesselIdName { get; set; }
        public double StoredAmount { get; set; }
    }
}
