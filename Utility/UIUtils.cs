using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ACSL.Tutorials;
using ACSL.Interaction;
using ACSL.HUD;

namespace ACSL.Utility
{
    public static class UIUtils
    {
        private static TooltipCanvas m_TooltipCanvas;

        private static GameObject m_Tooltip_Subtitle;
        private static GameObject m_Tooltip_Object;
        private static GameObject m_Tooltip_Objective;

        private static GameObject m_UI_DismissIcon;
        private static GameObject m_UI_ItemHeader;

        private static GameObject m_UI_ReplayMission;
        private static GameObject m_UI_ReplayTutorial;
        //private static GameObject m_UI_ButtonElement;

        private static Material m_Scanline_UI_Text = null;
        private static Material m_Scanline_UI_Image = null;
        private static Material m_Scanline_UI_Icon = null;

        /// <summary>
        /// Creates a subtitle with a lerp in, on,and out time.
        /// </summary>
        /// <param name="inOnOut">x = Lerp in Time. y = On screen Time. z = Lerp out Time.</param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static Tooltip_Subtitle CreateSubtitle(Vector3 inOnOut, string text)
        {
            Tooltip_Subtitle t = GameObject.Instantiate(Prefab_Tooltip_Subtitle).GetComponent<Tooltip_Subtitle>();
            t.TooltipTarget = GameUtils.LocalController.gameObject;
            t.transform.SetParent(GameUtils.LocalController.GetComponentInChildren<TooltipCanvas>(true).transform);
            t.transform.localRotation = Quaternion.identity;
            t.transform.localPosition = Vector3.zero;

            t.ActiveOnSubtitleCanvas = false;
            t.m_LerpInTime = inOnOut.x;
            t.m_OnScreenTime = inOnOut.y;
            t.m_LerpOutTime = inOnOut.z;
            t.GetComponent<Text>().text = text;

            return t;
        }
        public static Tooltip_Object CreateObjectTooltip(GameObject tooltipTarget, bool isObjective = false)
        {
            Tooltip_Object t = GameObject.Instantiate(isObjective ? Prefab_Tooltip_Objective : Prefab_Tooltip_Object).GetComponent<Tooltip_Object>();
            t.TooltipTarget = tooltipTarget;
            t.transform.SetParent(TooltipManager.Instance.transform);
            return t;
        }
        public static Material Material_ScanlineText
        {
            get { return m_Scanline_UI_Text != null ? m_Scanline_UI_Text : m_Scanline_UI_Text = Resources.Load("Shaders/Materials/Scanline_UI_Text", typeof(Material)) as Material; }
        }
        public static Material Material_ScanlineImage
        {
            get { return m_Scanline_UI_Image != null ? m_Scanline_UI_Image : m_Scanline_UI_Image = Resources.Load("Shaders/Materials/Scanline_UI_Image", typeof(Material)) as Material; }
        }
        public static Material Material_ScanlineIcon
        {
            get { return m_Scanline_UI_Icon != null ? m_Scanline_UI_Icon : m_Scanline_UI_Icon = Resources.Load("Shaders/Materials/Scanline_UI_Icon", typeof(Material)) as Material; }
        }
        public static GameObject Prefab_Tooltip_Subtitle
        {
            get { return m_Tooltip_Subtitle != null ? m_Tooltip_Subtitle : m_Tooltip_Subtitle = Resources.Load("Prefabs/UI/Icons/Tooltip_Subtitle", typeof(GameObject)) as GameObject; }
        }
        public static GameObject Prefab_Tooltip_Object
        {
            get { return m_Tooltip_Object != null ? m_Tooltip_Object : m_Tooltip_Object = Resources.Load("Prefabs/UI/Icons/Tooltip_Object", typeof(GameObject)) as GameObject; }
        }
        public static GameObject Prefab_Tooltip_Objective
        {
            get { return m_Tooltip_Objective != null ? m_Tooltip_Objective : m_Tooltip_Objective = Resources.Load("Prefabs/UI/Icons/Tooltip_Objective", typeof(GameObject)) as GameObject; }
        }
        public static GameObject Prefab_UI_DissmissIcon
        {
            get { return m_UI_DismissIcon != null ? m_UI_DismissIcon : m_UI_DismissIcon = Resources.Load("Prefabs/UI/Dismissable", typeof(GameObject)) as GameObject; }
        }
        public static GameObject Prefab_UI_ItemHeader
        {
            get { return m_UI_ItemHeader != null ? m_UI_ItemHeader : m_UI_ItemHeader = Resources.Load("Prefabs/UI/ItemHeader", typeof(GameObject)) as GameObject; }
        }
        //public static GameObject Prefab_UI_ButtonElement
        //{
        //    get { return m_UI_ButtonElement != null ? m_UI_ButtonElement : m_UI_ButtonElement = Resources.Load("Prefabs/UI/ButtonElement", typeof(GameObject)) as GameObject; }
        //}
        public static GameObject Prefab_UI_ReplayMission
        {
            get { return m_UI_ReplayMission != null ? m_UI_ReplayMission : m_UI_ReplayMission = Resources.Load("Prefabs/UI/ReplayMission", typeof(GameObject)) as GameObject; }
        }
        public static GameObject Prefab_UI_ReplayTutorial
        {
            get { return m_UI_ReplayTutorial != null ? m_UI_ReplayTutorial : m_UI_ReplayTutorial = Resources.Load("Prefabs/UI/ReplayTutorial", typeof(GameObject)) as GameObject; }
        }
        public static TooltipCanvas Canvas
        {
            get { return m_TooltipCanvas; }
            set { m_TooltipCanvas = value; }
        }
    }
}
