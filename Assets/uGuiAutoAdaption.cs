using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class uGuiAutoAdaption : MonoBehaviour
{
    [Serializable]
    private class Adaption
    {
        [SerializeField, Header("刘海设备型号")]
        public string deviceModel;
        [SerializeField, Header("该型号的刘海占比")]
        public float adaptionRatio = 0.033f;
    }
    [SerializeField, Header("UI摄像机")]
    private Camera uiCamera;

    [SerializeField, Header("标准分辨率: 宽")]
    private int standardWidth = 1920;
    [SerializeField, Header("标准分辨率: 高")]
    private int standardHeight = 1080;

    [SerializeField, Header("最大自适应分辨率: 宽"), Tooltip("自适应拓展的范围 一定保证大于刘海的大小")]
    private int maxWidth = 1920 + 10;
    [SerializeField, Header("最大自适应分辨率: 高"), Tooltip("自适应拓展的范围 一定保证大于刘海的大小")]
    private int maxHeight = 1080 + 10;

    [SerializeField, Header("刘海屏适应列表")]
    private Adaption[] adaptions;

#if UNITY_EDITOR
    public string testSysDeviceModel = "test";
#endif

    private string getSysDeviceModel()
    {
#if UNITY_EDITOR
        return testSysDeviceModel;
#else
        return SystemInfo.deviceModel;
#endif
    }

    private int screenWidth;
    private int screenHeight;

    private static Vector2 TMP_VECTOR2;

    private RectTransform cachedTran;
    public RectTransform CachedTran { get { if (cachedTran == null) cachedTran = GetComponent<RectTransform>(); return cachedTran; } }

    // Start is called before the first frame update
    void Start()
    {
        Update();
    }

    // Update is called once per frame
    void Update()
    {
        if (screenWidth != Screen.width || screenHeight != Screen.height)
        {
            screenWidth = Screen.width;
            screenHeight = Screen.height;

            float width = screenWidth;
            float height = screenHeight;
            float standardCameraSize = standardHeight / 2.0f;
            float standardAspect = standardWidth / (float)standardHeight;
            float screenAspect = width / height;
            float cameraScaleRatio = standardAspect / screenAspect;
            if (screenAspect < standardAspect)
            {
                uiCamera.orthographicSize = standardCameraSize * cameraScaleRatio;
                width = standardWidth;
                height = Math.Min(maxHeight, standardHeight * cameraScaleRatio);
            }
            else
            {
                uiCamera.orthographicSize = standardCameraSize;
                height = standardHeight;
                width = Math.Min(maxWidth, standardWidth / cameraScaleRatio);
            }

            #region 计算自适应和刘海占用比
            {
                if (adaptions != null && adaptions.Length > 0)
                {
                    string deviceModel = getSysDeviceModel();
                    for (int i = 0; i < adaptions.Length; ++i)
                    {
                        if (deviceModel == adaptions[i].deviceModel)
                        {
                            if (screenAspect > 1)
                            {
                                float adaptionPer = adaptions[i].adaptionRatio;

                                TMP_VECTOR2.x = width * adaptionPer * 0.5f;
                                TMP_VECTOR2.y = 0;

                                width -= width * adaptionPer;                                
                            }
                            else
                            {
                                float adaptionPer = adaptions[i].adaptionRatio;
                                TMP_VECTOR2.x = 0;
                                TMP_VECTOR2.y = -height * adaptionPer * 0.5f;

                                height -= height * adaptionPer;
                            }
                            CachedTran.anchoredPosition = TMP_VECTOR2;
                            break;
                        }
                    }
                }
            }
            #endregion

            TMP_VECTOR2.x = width;
            TMP_VECTOR2.y = height;
            CachedTran.sizeDelta = TMP_VECTOR2;
        }        
    }
}
