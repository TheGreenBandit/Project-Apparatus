using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Globalization;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using UnityEngine.UIElements;
using GameNetcodeStuff;
using static ProjectApparatus.Features;
using System.Runtime.CompilerServices;

namespace ProjectApparatus
{
    public static class PAUtils
    {
        static public BindingFlags protectedFlags = (BindingFlags.NonPublic | BindingFlags.Instance);

        [DllImport("User32.dll")]
        public static extern short GetAsyncKeyState(int key);

        public static bool IsPlayerValid(PlayerControllerB plyer)
        {
            return (plyer != null &&
                    !plyer.disconnectedMidGame &&
                    !plyer.playerUsername.Contains("Player #"));
        }

        public static Color GetLootColor(int value)
        {
            if (value <= 15) return Settings.Instance.settingsData.c_smallLoot;
            if (value > 15 && value <= 35) return Settings.Instance.settingsData.c_medLoot;
            if (value >= 36) return Settings.Instance.settingsData.c_bigLoot;
            else return Settings.Instance.settingsData.c_Loot;
        }

        public static void ForEach<T>(IEnumerable<T> objects, Action<T> action)
        {
            foreach (T obj in objects)
            {
                if (obj != null)
                {
                    action.Invoke(obj);
                }
            }
        }

        public static void SetValue(object instance, string variableName, object value, BindingFlags bindingFlags)
        {
            Type type = instance.GetType();
            FieldInfo fieldInfo = type.GetField(variableName, bindingFlags);
            fieldInfo?.SetValue(instance, value);
        }
        public static object GetValue(object instance, string variableName, BindingFlags bindingFlags)
        {
            Type type = instance.GetType();
            FieldInfo fieldInfo = type.GetField(variableName, bindingFlags);
            if (fieldInfo != null)
                return fieldInfo.GetValue(instance);

            return default(object);
        }

        public static object CallMethod(object instance, string methodName, BindingFlags bindingFlags, params object[] parameters)
        {
            Type type = instance.GetType();
            MethodInfo methodInfo = type.GetMethod(methodName, bindingFlags);

            if (methodInfo != null)
            {
                object result = methodInfo.Invoke(instance, parameters);
                return result;
            }

            return null;
        }

        public static string ConvertFirstLetterToUpperCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
            return textInfo.ToTitleCase(input);
        }

        public static string TruncateString(string inputStr, int charLimit)
        {
            if (inputStr.Length <= charLimit)
            {
                return inputStr;
            }
            else
            {
                return inputStr.Substring(0, charLimit - 3) + "...";
            }
        }

        public static bool WorldToScreen(Camera camera, Vector3 world, out Vector3 screen)
        {
            screen = camera.WorldToViewportPoint(world);
            screen.x *= (float)UnityEngine.Screen.width;
            screen.y *= (float)UnityEngine.Screen.height;
            screen.y = (float)UnityEngine.Screen.height - screen.y;
            return screen.z > 0f;
        }

        public static float GetDistance(Vector3 pos1, Vector3 pos2)
        {
            return (float)Math.Round((double)Vector3.Distance(pos1, pos2));
        }

        public static void SendChatMessage(string str, int playerid = -1)
        {
            string finalString = str;
            if (HUDManager.Instance.lastChatMessage == finalString) // Bypass chat spam prevention
                finalString += "\r";
            HUDManager.Instance.AddTextToChatOnServer(finalString, playerid);
        }
    }

    public static class UI
    {
        public enum Tabs
        {
            Start = 0,
            Self,
            Misc,
            ESP,
            Players,
            Graphics,
            Upgrades,
            Settings
        }

        public static Tabs nTab = 0;
        public static string strTooltip = null;

        public static void Reset()
        {
            strTooltip = null;
        }

        public static void TabContents(string strTabName, UI.Tabs tabToDisplay, Action tabContent)
        {
            if (nTab == tabToDisplay)
            {
                if (strTabName != null)
                    Header(strTabName);
                tabContent.Invoke();
            }
        }

        public static bool CenteredButton(string strName)
        {
            bool bReturn;
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            bReturn = GUILayout.Button(strName, GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            return bReturn;
        }

        public static void Tab<T>(string strTabName, ref T iTab, T iTabEle, bool bCenter = false)
        {
            if (bCenter ? CenteredButton(strTabName) : GUILayout.Button(strTabName))
            {
                iTab = iTabEle;
                Settings.Instance.windowRect.height = 400f;
            }
        }

        public static void Header(string str)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(str, GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        public static void ColorPicker(string str, ref Color col)
        {
            GUILayout.Label(str + " (R: " + Mathf.RoundToInt(col.r * 255f)
                + ", G: " + Mathf.RoundToInt(col.g * 255f)
                + ", B: " + Mathf.RoundToInt(col.b * 255f) + ")",
                Array.Empty<GUILayoutOption>());

            GUILayout.BeginHorizontal();
            col.r = GUILayout.HorizontalSlider(col.r, 0, 1f, GUILayout.Width(80));
            col.g = GUILayout.HorizontalSlider(col.g, 0, 1f, GUILayout.Width(80));
            col.b = GUILayout.HorizontalSlider(col.b, 0, 1f, GUILayout.Width(80));
            GUILayout.EndHorizontal();
        }

        public static bool Checkbox(ref bool var, string option, string tooltip = "")
        {
            bool previousValue = var;

            var = GUILayout.Toggle(var, option, Array.Empty<GUILayoutOption>());

            Rect lastRect = GUILayoutUtility.GetLastRect();
            if (lastRect.Contains(Event.current.mousePosition))
                strTooltip = tooltip;

            return previousValue != var;
        }

        public static void Button(string option, string tooltip, Action action)
        {
            if (GUILayout.Button(option))
                action.Invoke();

            Rect lastRect = GUILayoutUtility.GetLastRect();
            if (lastRect.Contains(Event.current.mousePosition))
                strTooltip = tooltip;
        }

        public static void RenderTooltip()
        {
            if (!Settings.Instance.settingsData.b_Tooltips || string.IsNullOrEmpty(strTooltip))
                return;

            GUIStyle tooltipStyle = GUI.skin.label;
            GUIContent tooltipContent = new GUIContent(strTooltip);
            float tooltipWidth = tooltipStyle.CalcSize(tooltipContent).x + 10f;
            float tooltipHeight = tooltipStyle.CalcHeight(tooltipContent, tooltipWidth - 10f) + 10f;

            Vector2 mousePos = Event.current.mousePosition;
            Color theme = Settings.Instance.settingsData.c_Theme;
            GUI.color = new Color(theme.r, theme.g, theme.b, 0.8f);

            Rect tooltipRect = new Rect(mousePos.x + 20f, mousePos.y + 20f, tooltipWidth, tooltipHeight);
            GUI.Box(tooltipRect, GUIContent.none);

            GUI.color = Color.white;
            GUI.Label(new Rect(tooltipRect.x + 5f, tooltipRect.y + 5f, tooltipWidth - 10f, tooltipHeight - 10f), strTooltip);
        }

        public static void Keybind(IBind Bind, ref int Key)
        {
            if(Bind.settingKeybind)
            {
                GUILayout.Button("Press any button to bind");
                Event e = Event.current;
                    
                if ((e.isMouse || e.isKey) && e.keyCode != KeyCode.Escape)
                {
                    Key = (int)e.keyCode;
                    Bind.settingKeybind = false;
                }
            }
            else
            {
                UI.Button(Key.ToString(), "Press to change keybind", () => Bind.settingKeybind = true);
            }

            GUILayout.Button(Key.ToString());
            Rect lastRect = GUILayoutUtility.GetLastRect();
            Event guiEvent = Event.current;

            if (lastRect.Contains(guiEvent.mousePosition))
            {
                for (int i = 0; i < 256; i++)
                {
                    if (i == (int)Keys.LButton
                        || i == (int)Keys.Insert) continue;
                    if (i > 6 && Event.current.type != EventType.KeyDown) continue;

                    if ((PAUtils.GetAsyncKeyState(i) & 1) != 0)
                    {
                        Key = (i == (int)Keys.Escape) ? 0 : i;
                        break;
                    }
                }
            }
        }
        public static Texture2D MakeTexture(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            Texture2D texture = new Texture2D(width, height);
            texture.SetPixels(pixels);
            texture.Apply();

            return texture;
        }

        public static Texture2D MakeGradientTexture(int width, int height, Color startColor, Color endColor, bool isHorizontal = true)
        {
            Color[] pixels = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float t = isHorizontal ? (float)x / (width - 1) : (float)y / (height - 1);
                    pixels[y * width + x] = Color.Lerp(startColor, endColor, t);
                }
            }

            Texture2D texture = new Texture2D(width, height);
            texture.SetPixels(pixels);
            texture.Apply();

            return texture;
        }
    }

    public static class Render
    {
        private class RingArray
        {
            public Vector2[] Positions { get; private set; }

            public RingArray(int numSegments)
            {
                Positions = new Vector2[numSegments];
                var stepSize = 360f / numSegments;
                for (int i = 0; i < numSegments; i++)
                {
                    var rad = Mathf.Deg2Rad * stepSize * i;
                    Positions[i] = new Vector2(Mathf.Sin(rad), Mathf.Cos(rad));
                }
            }
        }

        public static Color Color
        {
            get { return GUI.color; }
            set { GUI.color = value; }
        }

        public static void Line(Vector2 from, Vector2 to, float thickness, Color color)
        {
            Color = color;
            Line(from, to, thickness);
        }
        public static void Line(Vector2 from, Vector2 to, float thickness)
        {
            var delta = (to - from).normalized;
            var angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
            GUIUtility.RotateAroundPivot(angle, from);
            Box(from, Vector2.right * (from - to).magnitude, thickness, false);
            GUIUtility.RotateAroundPivot(-angle, from);
        }

        public static void Box(Vector2 position, Vector2 size, float thickness, Color color, bool centered = true)
        {
            Color = color;
            Box(position, size, thickness, centered);
        }
        public static void Box(Vector2 position, Vector2 size, float thickness, bool centered = true)
        {
            var upperLeft = centered ? position - size / 2f : position;
            GUI.DrawTexture(new Rect(position.x, position.y, size.x, thickness), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(position.x, position.y, thickness, size.y), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(position.x + size.x, position.y, thickness, size.y), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(position.x, position.y + size.y, size.x + thickness, thickness), Texture2D.whiteTexture);
        }

        public static void Cross(Vector2 position, Vector2 size, float thickness, Color color)
        {
            Color = color;
            Cross(position, size, thickness);
        }
        public static void Cross(Vector2 position, Vector2 size, float thickness)
        {
            GUI.DrawTexture(new Rect(position.x - size.x / 2f, position.y, size.x, thickness), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(position.x, position.y - size.y / 2f, thickness, size.y), Texture2D.whiteTexture);
        }

        public static void Dot(Vector2 position, Color color)
        {
            Color = color;
            Dot(position);
        }
        public static void Dot(Vector2 position)
        {
            Box(position - Vector2.one, Vector2.one * 2f, 1f);
        }

        public static void String(GUIStyle Style, float X, float Y, float W, float H, string str, Color col, bool centerx = false, bool centery = false)
        {
            GUIContent content = new GUIContent(str);

            Vector2 size = Style.CalcSize(content);
            float fX = centerx ? (X - size.x / 2f) : X,
                fY = centery ? (Y - size.y / 2f) : Y;

            Style.normal.textColor = Color.black;
            GUI.Label(new Rect(fX, fY, size.x, H), str, Style);

            Style.normal.textColor = col;
            GUI.Label(new Rect(fX + 1f, fY + 1f, size.x, H), str, Style);
        }

        public static void Circle(Vector2 center, float radius, float thickness, Color color)
        {
            Color = color;
            Vector2 previousPoint = center + new Vector2(radius, 0);

            for (int i = 1; i <= 360; i++)
            {
                float angle = i * Mathf.Deg2Rad;
                Vector2 nextPoint = center + new Vector2(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle));
                Line(previousPoint, nextPoint, thickness);
                previousPoint = nextPoint;
            }
        }

        public static void FilledCircle(Vector2 center, float radius, Color color)
        {
            Color = color;
            float sqrRadius = radius * radius;

            for (float y = -radius; y <= radius; y++)
                for (float x = -radius; x <= radius; x++)
                    if (x * x + y * y <= sqrRadius)
                        Line(center + new Vector2(x, y), center + new Vector2(x + 1, y), 1f);
        }
    }
}