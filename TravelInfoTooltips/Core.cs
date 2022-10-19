using BattleTech;
using BattleTech.UI;
using BattleTech.UI.Tooltips;
using Harmony;
using Localize;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TravelInfoTooltips{
  public class TravelInfoTooltip: EventTrigger {
    public override void OnPointerEnter(PointerEventData data) {
      Log.M?.TWL(0, "TravelInfoTooltip.OnPointerEnter",true);
    }
    public override void OnPointerExit(PointerEventData data) {
      Log.M?.TWL(0, "TravelInfoTooltip.OnPointerExit", true);
    }
    public override void OnPointerDown(PointerEventData data) {
      Log.M?.TWL(0, "TravelInfoTooltip.OnPointerDown", true);
    }
    public override void OnPointerUp(PointerEventData data) {
      Log.M?.TWL(0, "TravelInfoTooltip.OnPointerUp", true);
    }

  }
  [HarmonyPatch(typeof(SGSystemViewPopulator))]
  [HarmonyPatch("SetupTooltips")]
  [HarmonyPatch(MethodType.Normal)]
  public static class SGSystemViewPopulator_SetupTooltips {
    public static void Postfix(SGSystemViewPopulator __instance) {
      try {
        StringBuilder text = new StringBuilder();
        Log.M?.TWL(0, $"travel to {__instance.simState.Starmap.CurSelected.System.Def.Description.Id}");
        foreach (var travelEntry in __instance.simState.Starmap.PendingTravelOrder.SubEntries) {
          Log.M?.WL(1,$"{travelEntry.Description} cost:{travelEntry.Cost}");
          text.AppendLine(new Localize.Text("{0} {1}", travelEntry.Description, $"{travelEntry.Cost} Days").ToString());
        }
        BaseDescriptionDef tooltipData = new BaseDescriptionDef("TRAVEL_COST_TOOLTIP_ID","TRAVEL TIME", text.ToString(), string.Empty);
        foreach(var txt in __instance.SystemTravelTime) {
          Image img = txt.gameObject.GetComponentInChildren<Image>();
          if (img == null) {
            GameObject background = new GameObject("txt_background");
            background.transform.SetParent(txt.transform);
            background.transform.localPosition = Vector3.zero;
            background.transform.localRotation = Quaternion.identity;
            background.transform.localScale = Vector3.one;
            img = background.AddComponent<Image>();
            img.rectTransform.sizeDelta = txt.rectTransform.sizeDelta;
            img.rectTransform.pivot = txt.rectTransform.pivot;
            img.rectTransform.position = txt.rectTransform.position;
          }
          img.color = Color.clear;
          img.enabled = true;
          HBSTooltip tooltip = img.gameObject.GetComponent<HBSTooltip>();
          if (tooltip == null) { tooltip = img.gameObject.AddComponent<HBSTooltip>(); }
          if (tooltip.defaultStateData == null) { tooltip.defaultStateData = new HBSTooltipStateData(); }
          tooltip.defaultStateData.SetObject(tooltipData);
        }
      } catch(Exception e) {
        Log.Err?.TWL(0, e.ToString(), true);
      }
    }
  }
  public class Settings {
    public bool debugLog { get; set; }
    public Settings() { debugLog = true; }
  }
  public static class Core {
    public static Settings settings = new Settings();
    public static void Init(string directory, string settingsJson) {
      Log.BaseDirectory = directory;
      Log.InitLog();
      Log.Err?.TWL(0, "Initing... " + directory + " version: " + Assembly.GetExecutingAssembly().GetName().Version, true);
      try {
        Core.settings = JsonConvert.DeserializeObject<TravelInfoTooltips.Settings>(settingsJson);
        var harmony = HarmonyInstance.Create("ru.kmission.travelinfotooltips");
        harmony.PatchAll(Assembly.GetExecutingAssembly());
      } catch (Exception e) {
        Log.LogWrite(e.ToString(), true);
      }
    }
  }
}
