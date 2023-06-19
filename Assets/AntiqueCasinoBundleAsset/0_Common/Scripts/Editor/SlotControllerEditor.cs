using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using System;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

namespace Mkey
{
    [CustomEditor(typeof(SlotController))]
    public class SlotControllerEditor : Editor
    {
        public static Color32[] colorMap = new Color32[]
       {
            new Color32(255, 53, 13, 255), new Color(255, 94, 13, 255), new Color32(255, 134, 13, 255), new Color32(255, 175, 13, 255), new Color32(255, 215, 13, 255), new Color32(255, 255, 13, 255),
            new Color32(215, 255, 13, 255), new Color32(175, 255, 13, 255), new Color32(134, 255, 13, 255), new Color32(94, 255, 13, 255), new Color32(53, 255, 13, 255), new Color32(13, 255, 13, 255),
            new Color32(13, 255, 53, 255), new Color32(13, 255, 94, 255), new Color32(13, 255, 134, 255), new Color32(13, 255, 175, 255), new Color32(13, 255, 215, 255), new Color32(13, 255, 255, 255),
            new Color32(13, 215, 255, 255), new Color32(13, 175, 255, 255), new Color32(13, 134, 255, 255), new Color32(13, 94, 255, 255), new Color32(13, 53, 255, 255), new Color32(13, 13, 255, 255),
            new Color32(53, 13, 255, 255), new Color32(94, 13, 255, 255), new Color32(134, 13, 255, 255), new Color32(175, 13, 255, 255), new Color32(215, 13, 255, 255), new Color32(255, 13, 255, 255),
            new Color32(255, 13, 215, 255), new Color32(255, 13, 175, 255), new Color32(255, 13, 134, 255), new Color32(255, 13, 94, 255), new Color32(255, 13, 53, 255),new Color32(255, 13, 13, 255)
       };


        private static Color32[] colorMapShuffled18 = new Color32[]
        {
            new Color32(215,255,13,255),new Color32(13,53,255,255),new Color32(13,255,53,255),new Color32(255,134,13,255),new Color32(13,255,134,255),new Color32(255,13,215,255),
            new Color32(13,255,215,255),new Color32(215,13,255,255),new Color32(134,255,13,255),new Color32(255,13,53,255),new Color32(255,53,13,255),new Color32(53,255,13,255),
            new Color32(255,13,134,255),new Color32(255,215,13,255),new Color32(13,134,255,255),new Color32(134,13,255,255),new Color32(13,215,255,255),new Color32(53,13,255,255)
        };

        private static Color32[] colorMapShuffled36 = new Color32[]
       {
            new Color32(255,53,13,255),new Color32(255,255,255,255),new Color32(255,134,13,255),new Color32(255,175,13,255),new Color32(255,215,13,255),new Color32(255,255,13,255),
            new Color32(215,255,13,255),new Color32(175,255,13,255),new Color32(134,255,13,255),new Color32(94,255,13,255),new Color32(53,255,13,255),new Color32(13,255,13,255),
            new Color32(13,255,53,255),new Color32(13,255,94,255),new Color32(13,255,134,255),new Color32(13,255,175,255),new Color32(13,255,215,255),new Color32(13,255,255,255),
            new Color32(13,215,255,255),new Color32(13,175,255,255),new Color32(13,134,255,255),new Color32(13,94,255,255),new Color32(13,53,255,255),new Color32(13,13,255,255),
            new Color32(53,13,255,255),new Color32(94,13,255,255),new Color32(134,13,255,255),new Color32(175,13,255,255),new Color32(215,13,255,255),new Color32(255,13,255,255),
            new Color32(255,13,215,255),new Color32(255,13,175,255),new Color32(255,13,134,255),new Color32(255,13,94,255),new Color32(255,13,53,255),new Color32(255,13,13,255),
        };

        SlotController slotController;
        private ReorderableList payTableList;
        string[] choisesWithAny;
        string[] choises;

        private void OnEnable()
        {
            payTableList = new ReorderableList(serializedObject, serializedObject.FindProperty("payTable"),
                 true, true, true, true);

            payTableList.onRemoveCallback += RemoveCallback;
            payTableList.drawElementCallback += OnDrawCallback;
            payTableList.onAddCallback += OnAddCallBack;
            payTableList.onSelectCallback += OnSelectCallBack;
            payTableList.drawHeaderCallback += DrawHeaderCallBack;
            payTableList.onChangedCallback += OnChangeCallBack;
            payTableList.elementHeightCallback += OnElementHeightCallback;
            //  payTableList.onAddDropdownCallback += OnAddDropDownCallBack;

            ptFocusedIndex = -1;
            ptActiveIndex = -1;

            iconsSP = serializedObject.FindProperty("slotIcons");
    }

        private void OnDisable()
        {
            if (payTableList != null)
            {
                payTableList.onRemoveCallback -= RemoveCallback;
                payTableList.drawElementCallback -= OnDrawCallback;
                payTableList.onAddCallback -= OnAddCallBack;
                payTableList.onSelectCallback -= OnSelectCallBack;
                payTableList.drawHeaderCallback -= DrawHeaderCallBack;
                payTableList.onChangedCallback -= OnChangeCallBack;
                payTableList.onAddDropdownCallback -= OnAddDropDownCallBack;
                payTableList.elementHeightCallback -= OnElementHeightCallback;
            }
        }

        bool showPrefabs;
        bool showPayTable;
        bool showMajor;
        bool showTweenTarg;
        bool showOptions;
        bool showRotOptions;
        bool showDefault;
        bool showRef;
        bool showScatter;
        bool showJackPot;
        bool showLevelProgress;
        bool showDev;
        bool showEvents;
        bool haveIcons;

        SerializedProperty iconsSP;

        public override void OnInspectorGUI()
        {
            slotController = (SlotController)target;
            if (!slotController) return;

            choisesWithAny = slotController.GetIconNames(true);
            choises = slotController.GetIconNames(false);
            haveIcons = (choises != null && choises.Length > 0);
            serializedObject.Update();

            EditorExt.ShowProperties(serializedObject, new string[] {"machineID" }, false);

            #region main reference
            EditorExt.ShowPropertiesBoxFoldOut(serializedObject, "Main references: ", new string[] { "controls" ,"winController",
                                                                                                     "spinSound", "looseSound", "winCoinsSound",
                                                                                                     "winFreeSpinSound" }, ref showRef, false);
            #endregion main reference

            #region icons
            EditorExt.ShowPropertiesBox(serializedObject, new string[] { "slotIcons", "winSymbolBehaviors" }, true);
            #endregion icons

            #region payTable
            EditorExt.ShowReordListBoxFoldOut("Pay Table", payTableList, ref showPayTable);
            #endregion payTable

            #region special major
            ShowMajorChoise("Special Major Symbols", ref showMajor);
            #endregion spaecial major

            #region prefabs
            EditorExt.ShowPropertiesBoxFoldOut(serializedObject, "Prefabs: ", new string[]{ "tilePrefab", "particlesStars", "BigWinPrefab", "FreeGamesPUPrefab" }, ref showPrefabs, false);
            #endregion prefabs

            #region slotGroups
            EditorExt.ShowPropertiesBox(serializedObject, new string[] { "slotGroupsBeh" }, true);
            #endregion slotGroups

            #region tweenTargets
            EditorExt.ShowPropertiesBoxFoldOut(serializedObject, "Tween targets: ", new string[] { "bottomJumpTarget", "topJumpTarget" },ref showTweenTarg, true);
            #endregion tweenTargets

            #region spin options
            EditorExt.ShowPropertiesBoxFoldOut(serializedObject, "Spin options: ", new string[] {
                "inRotType",
                "inRotTime",
                "inRotAngle",
                "outRotType",
                "outRotTime",
                "outRotAngle",
                "mainRotateType",
                "mainRotateTime",
                "mainRotateTimeRandomize"
            }, ref showRotOptions, false);
            #endregion spin options

            #region options
            EditorExt.ShowPropertiesBoxFoldOut(serializedObject, "Options: ", new string[] {
                "RandomGenerator",
                "winLineFlashing",
                "winSymbolParticles",
                "useLineBetMultiplier",
                "useLineBetFreeSpinMultiplier",
                "debugPredictSymbols"
            }, ref showOptions, false);
            #endregion options

            #region levelprogress
            EditorExt.ShowPropertiesBoxFoldOut(serializedObject, "Level progress: ", new string[] {
                "useLineBetProgressMultiplier",
                "loseSpinLevelProgress",
                "winSpinLevelProgress",
            }, ref showLevelProgress, false);
            #endregion levelprogress

            #region events
            EditorExt.ShowPropertiesBoxFoldOut(serializedObject, "Events: ", new string[] {
            "StartFreeGamesEvent" ,
            "EndFreeGamesEvent"   ,
            "AnyWinShowEvent"
            }, ref showEvents, false);
            #endregion events

            #region calculate
            EditorGUILayout.BeginHorizontal("box");
            if (GUILayout.Button("Calculate"))
            {
                DataWindow.Init();
                float sum, sumPreeSpins;
                string[,] probTable = slotController.CreatePropabilityTable();
                string [,] payTable = slotController.CreatePayTable(out sum, out sumPreeSpins);
                DataWindow.SetData(probTable, payTable, sum, sumPreeSpins);
            }
            EditorGUILayout.EndHorizontal();
            #endregion calculate

            #region default
            EditorGUILayout.BeginVertical("box");
            EditorGUI.indentLevel += 1;
            EditorGUILayout.Space();
            if (showDefault = EditorGUILayout.Foldout(showDefault, "Default Inspector"))
            {
                DrawDefaultInspector();
            }
            EditorGUILayout.Space();
            EditorGUI.indentLevel -= 1;
            EditorGUILayout.EndVertical();
            #endregion default

            #region dev
            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUI.indentLevel += 1;
                EditorGUILayout.Space();
                if (showDev = EditorGUILayout.Foldout(showDev, "Development"))
                {
                    EditorExt.ShowProperties(serializedObject, new string[] { "payTableJsonString" }, false);

                    if (GUILayout.Button("Rebuild pay table lines"))
                    {
                        slotController.RebuildLines();
                    }

                    if (GUILayout.Button("Get paytable json"))
                    {
                        Debug.Log("Json viewer - " + "http://jsonviewer.stack.hu/");
                        Debug.Log("paytable json: " + slotController.PaytableToJsonString());
                    }

                    if (!string.IsNullOrEmpty(slotController.payTableJsonString) && GUILayout.Button("Set paytable from json"))
                    {
                        Debug.Log("paytable json: " + slotController.payTableJsonString);
                        slotController.SetPayTableFromJson();
                    }
                }
                EditorGUILayout.Space();
                EditorGUI.indentLevel -= 1;
                EditorGUILayout.EndVertical();
            }
            #endregion dev

            serializedObject.ApplyModifiedProperties();
        }

        #region icons
        private void DisplayIcons(bool auto, string [] fields)
        {
            for (int i = 0; i < iconsSP.arraySize; i++)
            {
                SerializedProperty MyListRef = iconsSP.GetArrayElementAtIndex(i);
                SerializedProperty icon = MyListRef.FindPropertyRelative("iconSprite");
                SerializedProperty useWild = MyListRef.FindPropertyRelative("useWildSubstitute");
                SerializedProperty iconBlur = MyListRef.FindPropertyRelative("iconBlur");
                SerializedProperty winBehs = MyListRef.FindPropertyRelative("privateWinBehaviors");


                // Display the property fields in two ways.

                if (!auto)
                {// Choose to display automatic or custom field types. This is only for example to help display automatic and custom fields.
                 //1. Automatic, No customization <-- Choose me I'm automatic and easy to setup
                 //   EditorGUILayout.LabelField(icon.);
                    EditorGUILayout.PropertyField(icon);
                    EditorGUILayout.PropertyField(iconBlur);
                    EditorGUILayout.PropertyField(useWild);
                    EditorGUILayout.PropertyField(winBehs);

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Array Fields");

                   
                }
                else
                {
                    EditorGUILayout.LabelField("Customizable Field With GUI");
                    winBehs.objectReferenceValue = EditorGUILayout.ObjectField("My Custom Go", winBehs.objectReferenceValue, typeof(GameObject), true);
                    icon.intValue = EditorGUILayout.IntField("My Custom Int", icon.intValue);
                    useWild.floatValue = EditorGUILayout.FloatField("My Custom Float", useWild.floatValue);
                    iconBlur.vector3Value = EditorGUILayout.Vector3Field("My Custom Vector 3", iconBlur.vector3Value);

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Array Fields");
                }
            }
        }
        #endregion icons

        #region payTableList CallBacks
        private void OnAddDropDownCallBack(Rect buttonRect, ReorderableList list)
        {
        }

        private void OnChangeCallBack(ReorderableList list)
        {
           // Debug.Log("onchange");
        }

        private void DrawHeaderCallBack(Rect rect)
        {
            EditorGUI.LabelField(rect, "Pay Table");
        }

        private void OnSelectCallBack(ReorderableList list)
        {
        }

        private void OnAddCallBack(ReorderableList list)
        {
            if (slotController == null || slotController.slotGroupsBeh == null || slotController.slotGroupsBeh.Length == 0) return;
            if (slotController.payTable != null && slotController.payTable.Count > 0)
            {
                slotController.payTable.Add(new PayLine(slotController.payTable[slotController.payTable.Count - 1]));
            }
            else
                slotController.payTable.Add(new PayLine());
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
           // Debug.Log("OnAddCallBack");
        }

        private int ptFocusedIndex = -1;
        private int ptActiveIndex = -1;
        private void OnDrawCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            EditorGUI.LabelField(rect, (index + 1).ToString());
            
            rect.y += 2;
            rect.x += 20;
            int count = (slotController && slotController.slotGroupsBeh != null && slotController.slotGroupsBeh.Length>0) ? slotController.slotGroupsBeh.Length : 5;
            ShowPayLine(choisesWithAny, rect, count, 70, 20, index, isActive, isFocused, slotController.payTable[index]);
         //   Debug.Log("inex " + index + " ;active: " + isActive + " ; focused: " + isFocused);
            if (isFocused) ptFocusedIndex = index;
            if (isActive) ptActiveIndex = index;

        }

        private void RemoveCallback(ReorderableList list)
        {
            if (EditorUtility.DisplayDialog("Warning!", "Are you sure?", "Yes", "No"))
            {
                slotController.payTable.RemoveAt(list.index); //ReorderableList.defaultBehaviours.DoRemoveButton(list);
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }
        }

        private float OnElementHeightCallback(int index)
        {
            Repaint();
            float height = EditorGUIUtility.singleLineHeight; 
            var element = payTableList.serializedProperty.GetArrayElementAtIndex(index);
            if (element.FindPropertyRelative("showEvent").boolValue && index==ptActiveIndex)
            {
                UnityEvent ue = slotController.payTable[index].LineEvent;
                int length = (ue!=null) ? ue.GetPersistentEventCount() : 0;
                float emptyLength = (length > 0) ? 3.5f : 5.5f;
                height = EditorGUIUtility.singleLineHeight * emptyLength + length * EditorGUIUtility.singleLineHeight*2.5f;
               // height = 120;
            }
            return height;
        }
        #endregion payTableList  CallBacks

        #region showChoise EditorGuiLayOut
        private void ShowMajorChoise(string bName, ref bool fOut)
        {
            if (!haveIcons) return;

            EditorGUILayout.BeginVertical("box");
            EditorGUI.indentLevel += 1;
            EditorGUILayout.Space();
            if (fOut = EditorGUILayout.Foldout(fOut, bName))
            {
                ShowWildChoiseLO(choises);
                ShowScatterChoiseLO(choises);
                if (slotController.useScatter)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("scatterPayTable"), true);
            }
            EditorGUILayout.Space();
            EditorGUI.indentLevel -= 1;
            EditorGUILayout.EndVertical();
        }

        private void ShowWildChoiseLO(string[] sChoise)
        {
            EditorGUILayout.BeginHorizontal();
            EditorExt.ShowProperties(serializedObject, new string[] { "useWild" }, false);
            if (slotController.useWild)
            {
                int choiseIndex = slotController.wild_id;
                int oldIndex = choiseIndex;
                choiseIndex = EditorGUILayout.Popup(choiseIndex, sChoise);
                slotController.wild_id = choiseIndex;
                if (oldIndex != choiseIndex) EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }
            EditorGUILayout.EndHorizontal();
            if (slotController.useWild)
            {
                EditorGUILayout.BeginHorizontal();
                EditorExt.ShowProperties(serializedObject, new string[] { "wildFeature" }, false);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(16);
            }
        }

        private void ShowScatterChoiseLO(string[] sChoise)
        {
            EditorGUILayout.BeginHorizontal();
            EditorExt.ShowProperties(serializedObject, new string[] { "useScatter" }, false);
            if (slotController.useScatter)
            {
                int choiseIndex = slotController.scatter_id;
                int oldIndex = choiseIndex;
                choiseIndex = EditorGUILayout.Popup(choiseIndex, sChoise);
                slotController.scatter_id = choiseIndex;
                if (oldIndex != choiseIndex) EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }
            EditorGUILayout.EndHorizontal();
        }
        #endregion showChoise EditorGuiLayOut

        #region showChoise payline EditorGui
        private void ShowChoisePL(string[] choises, Rect rect, float width, float height, float dx, float dy, PayLine pLine, int index)
        {
            if (!haveIcons || pLine.line == null || pLine.line.Length == 0 || pLine.line.Length <= index) return;
          
            int choiseIndex = pLine.line[index]+1; // any == 0;
            int oldIndex = choiseIndex;
            choiseIndex = EditorGUI.Popup(new Rect(rect.x + dx, rect.y+dy, width, height), choiseIndex, choises);
            pLine.line[index] = choiseIndex-1;
            if(oldIndex != choiseIndex) EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private void ShowPayLine(string[] choises, Rect rect, int count, float width, float height, int index, bool isActive, bool isFocused, PayLine pLine)
        {
            if (pLine == null) return;
            var element = payTableList.serializedProperty.GetArrayElementAtIndex(index);
            for (int i = 0; i < count; i++)
            {
                ShowChoisePL(choises, rect, width, height, i * width + i * 1.0f, 0, pLine, i);
            }
            float dx = rect.x + count * width + count;
            float w = 40;
            float h = EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(new Rect(dx, rect.y, w, h), "Pay ");
            dx += w;
            w = 50;

            EditorGUI.PropertyField(new Rect(dx, rect.y, w, h),
                        element.FindPropertyRelative("pay"), GUIContent.none);
            dx += w; w = 75;
            EditorGUI.LabelField(new Rect(dx, rect.y, w,h), "FreeSpins");
            dx += w; w = 50;
            EditorGUI.PropertyField(new Rect(dx, rect.y, w,h),
                        element.FindPropertyRelative("freeSpins"), GUIContent.none);
            dx += w; w = 65;
            EditorGUI.LabelField(new Rect(dx, rect.y, w, h), "PayMult");
            dx += w; w = 40;
            EditorGUI.PropertyField(new Rect(dx, rect.y, w, h),
                        element.FindPropertyRelative("payMult"), GUIContent.none);
            dx += w; w = 100;
            EditorGUI.LabelField(new Rect(dx, rect.y, w, h), "FreeSpinsMult");
            dx += w; w = 40;
            EditorGUI.PropertyField(new Rect(dx, rect.y, w, h),
                        element.FindPropertyRelative("freeSpinsMult"), GUIContent.none);

            dx += w; w = 105;
            int evCounts = 0;
            if (slotController && slotController.payTable!= null )
            {
                evCounts = slotController.payTable[index].LineEvent.GetPersistentEventCount();
                //Debug.Log(evCounts);
            }
            string sE = (evCounts > 0) ? "ShowEvent(" + evCounts + "):" : "ShowEvent :";
            EditorGUI.LabelField(new Rect(dx, rect.y, w, h), sE);
            dx += w; w = 40;
            EditorGUI.PropertyField(new Rect(dx, rect.y, w, h),
                        element.FindPropertyRelative("showEvent"), GUIContent.none);
            if (element.FindPropertyRelative("showEvent").boolValue && isActive)
            {
                dx += w; w = 170;
                EditorGUI.PropertyField(new Rect(dx, rect.y, w, h),
                 element.FindPropertyRelative("LineEvent"), GUIContent.none);
            }
        }
        #endregion showChoise payline EditorGui

        static bool shuffled = false;
        public static Color32 GetColor(int index)
        {
            /*
            if (!shuffled)
            {
                List<Color32> cl = new List<Color32>();

                for (int i = 0; i < colorMap.Length; i+=2)
                {
                    cl.Add(colorMap[i]);
                }

                cl.Shuffle(); cl.Shuffle();
                string res = "";
                for (int i = 0; i < cl.Count; i++)
                {
                    res += ("new Color32(" + cl[i].r +"," + cl[i].g + "," + cl[i].b + "," + cl[i].a + "),");
                }
                shuffled = true;
                Debug.Log(res);
            }
            */
            int ind = index % colorMapShuffled18.Length;
            return colorMapShuffled18[ind];
        }
    }
}

