﻿using Harmony12;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CallOfTheWild
{
    namespace LoadIcons
    {
        // Loosely based on https://forum.unity.com/threads/generating-sprites-dynamically-from-png-or-jpeg-files-in-c.343735/
        public static class Image2Sprite
        {
            public static string icons_folder = "";
            public static Sprite Create(string filePath)
            {
                var bytes = File.ReadAllBytes(icons_folder + filePath);
                var texture = new Texture2D(64, 64, TextureFormat.DXT5, false);
                texture.LoadImage(bytes);
                return Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0, 0));
            }
        }
    }


    [HarmonyPatch(typeof(CustomPortraitsManager), "GetPortraitFolderPath", typeof(string))]
    static class GetPortraitFolderPath_Patch
    {
        private static bool Prefix(CustomPortraitsManager __instance, string id, ref string __result)
        {
            if (id.Contains("CallOfTheWild"))
            {
                __result = Path.Combine(UnityModManagerNet.UnityModManager.modsPath + @"/CallOfTheWild/Icons/Portraits", id.Replace("CallOfTheWild", ""));
                return false;
            }

            return true;
        }
    }
}
